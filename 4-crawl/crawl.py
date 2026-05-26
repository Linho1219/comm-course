from __future__ import annotations

import argparse
import asyncio
import json
import math
import subprocess
import urllib.error
import urllib.request
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from playwright.async_api import Page, async_playwright


BASE_URL = "https://1.tongji.icu"
DEFAULT_CDP_HOST = "127.0.0.1"
DEFAULT_CDP_PORT = 9222
BROWSER_CANDIDATES = [
    ("chrome", Path("C:/Program Files/Google/Chrome/Application/chrome.exe")),
    ("edge", Path("C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe")),
    ("edge", Path("C:/Program Files/Microsoft/Edge/Application/msedge.exe")),
    ("chrome", Path("C:/Program Files (x86)/Google/Chrome/Application/chrome.exe")),
]
ENDPOINTS = {
    "courses": "course",
    "ratings": "review",
}


def write_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


async def fetch_api_json(page: Page, url: str) -> dict[str, Any]:
    result = await page.evaluate(
        """
        async (url) => {
          const response = await fetch(url, { credentials: "include" });
          const text = await response.text();
          let body = null;
          try {
            body = JSON.parse(text);
          } catch {
            body = null;
          }
          return {
            ok: response.ok,
            status: response.status,
            statusText: response.statusText,
            url: response.url,
            body,
            preview: text.slice(0, 500)
          };
        }
        """,
        url,
    )
    if not result["ok"] or result["body"] is None:
        raise RuntimeError(
            f"API request failed: {result['status']} {result['statusText']} at {url}\n"
            f"Response preview: {result['preview']}"
        )
    return result["body"]


async def crawl_endpoint(
    page: Page,
    base_url: str,
    endpoint_name: str,
    api_name: str,
    page_size: int,
    max_pages: int | None,
    delay: float,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], dict[str, Any]]:
    pages: list[dict[str, Any]] = []
    records: list[dict[str, Any]] = []
    page_no = 1
    total_pages: int | None = None

    while True:
        api_url = f"{base_url.rstrip('/')}/api/{api_name}/?&page={page_no}&size={page_size}"
        body = await fetch_api_json(page, api_url)

        page_records = body.get("results")
        if not isinstance(page_records, list):
            raise RuntimeError(f"Unexpected API shape for {api_url}: missing list field 'results'")

        pages.append(body)
        records.extend(page_records)

        total_count = body.get("count")
        if isinstance(total_count, int) and total_count >= 0:
            total_pages = max(1, math.ceil(total_count / page_size))

        total_label = str(total_pages) if total_pages is not None else "?"
        print(
            f"[{endpoint_name}] page {page_no}/{total_label}: "
            f"{len(page_records)} records, {len(records)} accumulated"
        )

        if max_pages is not None and page_no >= max_pages:
            break
        if total_pages is not None and page_no >= total_pages:
            break
        if body.get("next") in (None, "") and len(page_records) < page_size:
            break
        if not page_records:
            break

        page_no += 1
        if delay > 0:
            await asyncio.sleep(delay)

    metadata = {
        "endpoint": endpoint_name,
        "api": f"/api/{api_name}/",
        "page_size": page_size,
        "pages": len(pages),
        "records": len(records),
        "captured_at": datetime.now(timezone.utc).isoformat(),
    }
    return pages, records, metadata


def find_browser_executable(executable_path: Path | None) -> tuple[str, Path]:
    if executable_path:
        if executable_path.exists():
            return (executable_path.stem, executable_path)
        raise FileNotFoundError(f"Browser executable does not exist: {executable_path}")

    for browser_name, candidate in BROWSER_CANDIDATES:
        if candidate.exists():
            return browser_name, candidate

    candidates = "\n".join(f"- {path}" for _, path in BROWSER_CANDIDATES)
    raise FileNotFoundError(
        "No Chrome/Edge executable found. Checked:\n"
        f"{candidates}\n"
        "Install Chrome/Edge or pass --executable-path."
    )


def launch_browser_for_cdp(
    executable_path: Path,
    profile_dir: Path | None,
    base_url: str,
    host: str,
    port: int,
    headless: bool,
) -> subprocess.Popen:
    resolved_profile_dir: Path | None = None
    if profile_dir is not None:
        resolved_profile_dir = profile_dir.resolve()
        resolved_profile_dir.mkdir(parents=True, exist_ok=True)

    args = [
        str(executable_path),
        f"--remote-debugging-address={host}",
        f"--remote-debugging-port={port}",
        "--no-first-run",
        "--no-default-browser-check",
    ]
    if resolved_profile_dir is not None:
        args.append(f"--user-data-dir={resolved_profile_dir}")
    if headless:
        args.append("--headless=new")
    args.append(base_url)

    return subprocess.Popen(
        args,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )


def probe_cdp(cdp_url: str) -> bool:
    try:
        with urllib.request.urlopen(f"{cdp_url.rstrip('/')}/json/version", timeout=1) as response:
            return response.status == 200
    except (OSError, urllib.error.URLError):
        return False


async def wait_for_cdp(cdp_url: str, timeout_seconds: float) -> None:
    deadline = asyncio.get_running_loop().time() + timeout_seconds
    while asyncio.get_running_loop().time() < deadline:
        if await asyncio.to_thread(probe_cdp, cdp_url):
            return
        await asyncio.sleep(0.25)
    raise TimeoutError(
        f"Timed out waiting for Chrome DevTools Protocol at {cdp_url}.\n"
        "If you launched the browser with the default profile, close all existing Chrome/Edge windows "
        "and run the command again, or pass a separate --profile-dir."
    )


async def connect_cdp_page(p, cdp_url: str, base_url: str) -> tuple[Page, Any, Any]:
    browser = await p.chromium.connect_over_cdp(cdp_url)
    context = browser.contexts[0] if browser.contexts else await browser.new_context()
    page = next(
        (candidate for candidate in context.pages if base_url in candidate.url),
        context.pages[0] if context.pages else await context.new_page(),
    )
    if page.url in ("about:blank", "") or base_url not in page.url:
        await page.goto(base_url, wait_until="domcontentloaded")
    return page, browser, context


async def open_crawl_page(p, args: argparse.Namespace) -> tuple[Page, Any, Any, subprocess.Popen | None]:
    if args.cdp_url:
        page, browser, context = await connect_cdp_page(p, args.cdp_url, args.base_url)
        return page, browser, context, None

    browser_name, executable_path = find_browser_executable(args.executable_path)
    cdp_url = f"http://{args.remote_debugging_host}:{args.remote_debugging_port}"
    cdp_profile_dir = None if args.use_default_profile else args.profile_dir / f"{browser_name}-cdp"
    process = launch_browser_for_cdp(
        executable_path=executable_path,
        profile_dir=cdp_profile_dir,
        base_url=args.base_url,
        host=args.remote_debugging_host,
        port=args.remote_debugging_port,
        headless=args.headless,
    )
    print(f"Opened {browser_name} via CDP: {executable_path}")
    if cdp_profile_dir is None:
        print("Profile: default browser profile")
    else:
        print(f"Profile: {cdp_profile_dir.resolve()}")
    print(f"CDP endpoint: {cdp_url}")
    print("Waiting for the browser CDP endpoint...")

    await wait_for_cdp(cdp_url, args.cdp_timeout)
    page, browser, context = await connect_cdp_page(p, cdp_url, args.base_url)
    return page, browser, context, process


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Crawl Wulongcha course and review data after a human finishes Cloudflare/login."
    )
    parser.add_argument("--base-url", default=BASE_URL, help=f"Site origin, default: {BASE_URL}")
    parser.add_argument(
        "--targets",
        nargs="+",
        default=["courses", "ratings"],
        choices=[*ENDPOINTS.keys(), "all"],
        help="Data targets to crawl.",
    )
    parser.add_argument("--out-dir", type=Path, default=Path("data"), help="Output directory.")
    parser.add_argument(
        "--profile-dir",
        type=Path,
        default=Path(".browser-state"),
        help="Persistent browser profile for local login state.",
    )
    parser.add_argument("--page-size", type=int, default=100, help="API page size.")
    parser.add_argument("--max-pages", type=int, default=None, help="Debug limit for each endpoint.")
    parser.add_argument("--delay", type=float, default=0.25, help="Delay between API pages in seconds.")
    parser.add_argument("--headless", action="store_true", help="Launch the external browser headlessly.")
    parser.add_argument("--executable-path", type=Path, help="Path to a Chromium-based browser executable.")
    parser.add_argument(
        "--use-default-profile",
        action="store_true",
        help="Do not pass --user-data-dir; use the browser's default profile. Close existing browser windows first.",
    )
    parser.add_argument(
        "--cdp-url",
        help="Connect to an already running Chrome/Edge instance, e.g. http://127.0.0.1:9222.",
    )
    parser.add_argument(
        "--remote-debugging-host",
        default=DEFAULT_CDP_HOST,
        help=f"Host for the browser CDP endpoint, default: {DEFAULT_CDP_HOST}.",
    )
    parser.add_argument(
        "--remote-debugging-port",
        type=int,
        default=DEFAULT_CDP_PORT,
        help=f"Port for the browser CDP endpoint, default: {DEFAULT_CDP_PORT}.",
    )
    parser.add_argument(
        "--cdp-timeout",
        type=float,
        default=20,
        help="Seconds to wait for the browser CDP endpoint.",
    )
    parser.add_argument(
        "--skip-login-wait",
        action="store_true",
        help="Start API requests immediately without waiting for Enter.",
    )
    return parser.parse_args()


def resolve_targets(targets: list[str]) -> list[str]:
    if "all" in targets:
        return list(ENDPOINTS.keys())
    resolved: list[str] = []
    for target in targets:
        if target not in resolved:
            resolved.append(target)
    return resolved


async def main() -> None:
    args = parse_args()
    targets = resolve_targets(args.targets)

    async with async_playwright() as p:
        page, browser, _context, launched_process = await open_crawl_page(p, args)

        if not args.headless and not args.skip_login_wait:
            print("\nBrowser opened. Finish Cloudflare challenge and login manually.")
            print("When the Wulongcha page is usable, return here and press Enter.")
            input("> ")

        all_metadata: list[dict[str, Any]] = []
        for target in targets:
            raw_pages, records, metadata = await crawl_endpoint(
                page=page,
                base_url=args.base_url,
                endpoint_name=target,
                api_name=ENDPOINTS[target],
                page_size=args.page_size,
                max_pages=args.max_pages,
                delay=args.delay,
            )
            write_json(args.out_dir / f"wlc.{target}.raw.json", raw_pages)
            write_json(args.out_dir / f"wlc.{target}.json", records)
            all_metadata.append(metadata)

        write_json(args.out_dir / "metadata.json", all_metadata)
        if launched_process is not None and launched_process.poll() is None:
            print("Leaving the browser window open. Close it manually when finished.")

    print(f"\nDone. Wrote data to {args.out_dir.resolve()}")


if __name__ == "__main__":
    asyncio.run(main())
