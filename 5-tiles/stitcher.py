from __future__ import annotations

import argparse
import itertools
import sys
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass
from io import BytesIO
from pathlib import Path
from typing import Iterable
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen

from PIL import Image, UnidentifiedImageError


USER_AGENT = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) "
    "Chrome/125.0.0.0 Safari/537.36"
)

BASE_QUERY = {
    "lang": "zh_cn",
    "size": "1",
    "scl": "1",
    "style": "7",
}


@dataclass(frozen=True)
class Tile:
    z: int
    x: int
    y: int
    server: int

    @property
    def cache_name(self) -> Path:
        return Path(f"z{self.z}") / f"x{self.x}" / f"y{self.y}.tile"


@dataclass(frozen=True)
class DownloadedTile:
    tile: Tile
    data: bytes
    from_cache: bool


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Download AutoNavi map tiles and stitch them into one image."
    )
    parser.add_argument("--x-min", type=int, default=420, help="minimum x tile, inclusive")
    parser.add_argument("--x-max", type=int, default=450, help="maximum x tile, inclusive")
    parser.add_argument("--y-min", type=int, default=190, help="minimum y tile, inclusive")
    parser.add_argument("--y-max", type=int, default=220, help="maximum y tile, inclusive")
    parser.add_argument("--z", type=int, default=9, help="zoom level")
    parser.add_argument(
        "--concurrency",
        type=int,
        default=10,
        help="number of concurrent download workers",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=None,
        help="stitched image path, default is output/autonavi_*.png",
    )
    parser.add_argument(
        "--cache-dir",
        type=Path,
        default=Path("tiles"),
        help="directory used to cache downloaded tiles",
    )
    parser.add_argument(
        "--no-cache",
        action="store_true",
        help="download every tile again and do not write the tile cache",
    )
    parser.add_argument(
        "--timeout",
        type=float,
        default=15.0,
        help="HTTP timeout in seconds for each request",
    )
    parser.add_argument(
        "--retries",
        type=int,
        default=3,
        help="maximum attempts per tile",
    )
    args = parser.parse_args()

    if args.x_max < args.x_min:
        parser.error("--x-max must be greater than or equal to --x-min")
    if args.y_max < args.y_min:
        parser.error("--y-max must be greater than or equal to --y-min")
    if args.concurrency < 1:
        parser.error("--concurrency must be at least 1")
    if args.timeout <= 0:
        parser.error("--timeout must be greater than 0")
    if args.retries < 1:
        parser.error("--retries must be at least 1")

    if args.output is None:
        args.output = Path("output") / (
            f"autonavi_z{args.z}_x{args.x_min}-{args.x_max}_"
            f"y{args.y_min}-{args.y_max}.png"
        )

    return args


def build_tiles(z: int, x_values: Iterable[int], y_values: Iterable[int]) -> list[Tile]:
    servers = itertools.cycle(range(1, 5))
    tiles: list[Tile] = []

    for y in y_values:
        for x in x_values:
            tiles.append(Tile(z=z, x=x, y=y, server=next(servers)))

    return tiles


def build_url(tile: Tile) -> str:
    query = {
        **BASE_QUERY,
        "z": str(tile.z),
        "x": str(tile.x),
        "y": str(tile.y),
    }
    return f"https://wprd{tile.server:02d}.is.autonavi.com/appmaptile?{urlencode(query)}"


def download_tile(
    tile: Tile,
    cache_dir: Path,
    use_cache: bool,
    timeout: float,
    attempts: int,
) -> DownloadedTile:
    cache_path = cache_dir / tile.cache_name
    if use_cache and cache_path.exists() and cache_path.stat().st_size > 0:
        return DownloadedTile(tile=tile, data=cache_path.read_bytes(), from_cache=True)

    last_error: Exception | None = None
    for attempt in range(1, attempts + 1):
        try:
            request = Request(
                build_url(tile),
                headers={
                    "User-Agent": USER_AGENT,
                    "Accept": "image/avif,image/webp,image/apng,image/*,*/*;q=0.8",
                    "Accept-Language": "zh-CN,zh;q=0.9,en;q=0.8",
                    "Connection": "close",
                },
            )
            with urlopen(request, timeout=timeout) as response:
                data = response.read()
                if not data:
                    raise RuntimeError("empty response body")

            if use_cache:
                cache_path.parent.mkdir(parents=True, exist_ok=True)
                cache_path.write_bytes(data)

            return DownloadedTile(tile=tile, data=data, from_cache=False)
        except (HTTPError, URLError, TimeoutError, RuntimeError) as exc:
            last_error = exc
            if attempt < attempts:
                time.sleep(min(0.5 * (2 ** (attempt - 1)), 5.0))

    raise RuntimeError(
        f"failed to download z={tile.z}, x={tile.x}, y={tile.y} "
        f"from wprd{tile.server:02d}: {last_error}"
    )


def download_all(
    tiles: list[Tile],
    cache_dir: Path,
    use_cache: bool,
    concurrency: int,
    timeout: float,
    attempts: int,
) -> dict[tuple[int, int], bytes]:
    total = len(tiles)
    by_coord: dict[tuple[int, int], bytes] = {}
    cached = 0
    downloaded = 0

    with ThreadPoolExecutor(max_workers=concurrency) as pool:
        futures = [
            pool.submit(download_tile, tile, cache_dir, use_cache, timeout, attempts)
            for tile in tiles
        ]

        for future in as_completed(futures):
            item = future.result()
            by_coord[(item.tile.x, item.tile.y)] = item.data
            cached += int(item.from_cache)
            downloaded += int(not item.from_cache)
            finished = cached + downloaded
            print(
                f"\rtiles: {finished}/{total} "
                f"(downloaded {downloaded}, cached {cached})",
                end="",
                flush=True,
            )

    print()
    return by_coord


def stitch_tiles(
    tiles: dict[tuple[int, int], bytes],
    x_values: list[int],
    y_values: list[int],
    output: Path,
) -> None:
    first_key = (x_values[0], y_values[0])
    try:
        with Image.open(BytesIO(tiles[first_key])) as first_image:
            first_image.load()
            tile_width, tile_height = first_image.size
    except (KeyError, UnidentifiedImageError) as exc:
        raise RuntimeError(f"cannot read first tile {first_key}: {exc}") from exc

    canvas = Image.new(
        "RGBA",
        (tile_width * len(x_values), tile_height * len(y_values)),
    )

    for row, y in enumerate(y_values):
        for col, x in enumerate(x_values):
            data = tiles.get((x, y))
            if data is None:
                raise RuntimeError(f"missing tile x={x}, y={y}")

            try:
                with Image.open(BytesIO(data)) as tile_image:
                    tile_image.load()
                    if tile_image.size != (tile_width, tile_height):
                        raise RuntimeError(
                            f"tile x={x}, y={y} has size {tile_image.size}, "
                            f"expected {(tile_width, tile_height)}"
                        )
                    canvas.paste(tile_image.convert("RGBA"), (col * tile_width, row * tile_height))
            except UnidentifiedImageError as exc:
                raise RuntimeError(f"cannot read tile x={x}, y={y}: {exc}") from exc

    output.parent.mkdir(parents=True, exist_ok=True)
    if output.suffix.lower() in {".jpg", ".jpeg"}:
        canvas.convert("RGB").save(output)
    else:
        canvas.save(output)


def main() -> int:
    args = parse_args()
    x_values = list(range(args.x_min, args.x_max + 1))
    y_values = list(range(args.y_min, args.y_max + 1))
    tiles = build_tiles(args.z, x_values, y_values)

    print(
        f"Downloading {len(tiles)} tiles: "
        f"z={args.z}, x={args.x_min}-{args.x_max}, y={args.y_min}-{args.y_max}, "
        f"concurrency={args.concurrency}"
    )

    downloaded = download_all(
        tiles=tiles,
        cache_dir=args.cache_dir,
        use_cache=not args.no_cache,
        concurrency=args.concurrency,
        timeout=args.timeout,
        attempts=args.retries,
    )
    stitch_tiles(downloaded, x_values, y_values, args.output)

    print(f"Saved {args.output}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except KeyboardInterrupt:
        print("\nInterrupted", file=sys.stderr)
        raise SystemExit(130)
    except Exception as exc:
        print(f"\nError: {exc}", file=sys.stderr)
        raise SystemExit(1)
