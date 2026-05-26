from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from datetime import datetime
from pathlib import Path
from typing import Iterable

import matplotlib.pyplot as plt
import pandas as pd
from matplotlib import font_manager


ROOT = Path(__file__).resolve().parent
DATA_DIR = ROOT / "data"
REPORT_DIR = ROOT / "report"
ASSETS_DIR = ROOT / "assets"
DETAIL_OUTPUT_FILES = [
    "summary.json",
    "department_stats.csv",
    "category_stats.csv",
    "semester_stats.csv",
    "course_rankings.csv",
    "top_rated_courses.csv",
    "keyword_frequency.csv",
    "teacher_stats.csv",
]
STALE_FIGURE_FILES = [
    "rating_distribution.png",
    "semester_trend.png",
]
STALE_FIGURE_PATTERNS = [
    "department_review_count_top*.png",
]
RATING_COLORS = {
    1: "#B64D4D",
    2: "#D9825B",
    3: "#E6C35C",
    4: "#7BAE78",
    5: "#3C8D6A",
}

STOPWORDS = {
    "一个",
    "一些",
    "不是",
    "不能",
    "不用",
    "东西",
    "主要",
    "也是",
    "以及",
    "作为",
    "只有",
    "可以",
    "因为",
    "所以",
    "如果",
    "就是",
    "已经",
    "很多",
    "感觉",
    "我们",
    "还是",
    "比较",
    "没有",
    "然后",
    "真的",
    "自己",
    "这个",
    "这些",
    "这种",
    "课程",
    "上课",
    "考核",
    "授课",
    "老师",
    "同学",
    "学期",
    "时候",
    "作业",
    "一次",
    "非常",
    "内容"
}

TEMPLATE_LABEL_RE = re.compile(
    r"(?m)^[\s#>*_\-`]*(课程内容|上课自由度|考核标准|授课质量|签到和课程要求|作业形式、主要内容和评价方式|答疑|考核和成绩占比|主观评价|如果你是优，你是怎么拿到的)\s*[:：]\s*"
)
SEMESTER_NAME_ORDER = {
    "一": 1,
    "二": 2,
    "三": 3,
    "四": 4,
}


def load_json(path: Path) -> list[dict]:
    if not path.exists():
        raise FileNotFoundError(
            f"Input file not found: {path}\n"
            "Run crawl.py first, or pass --courses/--ratings with explicit JSON paths."
        )
    return json.loads(path.read_text(encoding="utf-8"))


def write_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def default_path(kind: str) -> Path:
    return DATA_DIR / f"wlc.{kind}.json"


def find_chinese_font() -> Path | None:
    candidates = [
        Path("C:/Windows/Fonts/HarmonyOS_Sans_SC_Regular_0.ttf"),
        Path("C:/Windows/Fonts/msyh.ttc"),
        Path("C:/Windows/Fonts/simhei.ttf"),
        Path("C:/Windows/Fonts/simsun.ttc"),
        Path("/System/Library/Fonts/PingFang.ttc"),
        Path("/System/Library/Fonts/STHeiti Light.ttc"),
        Path("/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc"),
        Path("/usr/share/fonts/truetype/noto/NotoSansCJK-Regular.ttc"),
        Path("/usr/share/fonts/truetype/wqy/wqy-microhei.ttc"),
    ]
    for candidate in candidates:
        if candidate.exists():
            return candidate
    return None


def configure_matplotlib() -> Path | None:
    font_path = find_chinese_font()
    if font_path:
        font_manager.fontManager.addfont(str(font_path))
        font_name = font_manager.FontProperties(fname=str(font_path)).get_name()
        plt.rcParams["font.sans-serif"] = [font_name, "DejaVu Sans"]
    plt.rcParams["axes.unicode_minus"] = False
    return font_path


def course_frame(courses: list[dict]) -> pd.DataFrame:
    rows = []
    for course in courses:
        rating = course.get("rating") or {}
        rows.append(
            {
                "course_id": course.get("id"),
                "code": course.get("code"),
                "name": course.get("name"),
                "teacher": course.get("teacher"),
                "department": course.get("department") or "未知学院",
                "categories": course.get("categories") or ["未分类"],
                "credit": course.get("credit"),
                "catalog_review_count": rating.get("count", 0),
                "catalog_avg_rating": rating.get("avg", 0),
            }
        )
    return pd.DataFrame(rows)


def rating_frame(ratings: list[dict], course_lookup: dict[int, dict]) -> pd.DataFrame:
    rows = []
    for item in ratings:
        course_ref = item.get("course") or {}
        course_id = course_ref.get("id")
        course = course_lookup.get(course_id, {})
        reactions = item.get("reactions") or {}
        rows.append(
            {
                "review_id": item.get("id"),
                "course_id": course_id,
                "code": course_ref.get("code") or course.get("code"),
                "name": course_ref.get("name") or course.get("name"),
                "teacher": course_ref.get("teacher") or course.get("teacher"),
                "department": course.get("department") or "未知学院",
                "categories": course.get("categories") or ["未分类"],
                "rating": item.get("rating"),
                "score": item.get("score"),
                "semester": item.get("semester") or "未知学期",
                "comment": item.get("comment") or "",
                "approves": reactions.get("approves", 0),
                "disapproves": reactions.get("disapproves", 0),
                "created_at": item.get("created_at"),
                "modified_at": item.get("modified_at"),
                "matched_course": bool(course),
            }
        )
    df = pd.DataFrame(rows)
    if not df.empty:
        df["created_at_dt"] = pd.to_datetime(df["created_at"], errors="coerce")
        df["rating"] = pd.to_numeric(df["rating"], errors="coerce")
    return df


def weighted_average(values: pd.Series, weights: pd.Series) -> float:
    values = pd.to_numeric(values, errors="coerce").fillna(0)
    weights = pd.to_numeric(weights, errors="coerce").fillna(0)
    total_weight = weights.sum()
    if total_weight <= 0:
        return 0.0
    return float((values * weights).sum() / total_weight)


def semester_sort_key(value: object) -> tuple[int, int, int, str]:
    text = str(value)
    if text == "老乌龙茶搬运":
        return (-1, 0, 0, text)
    match = re.search(r"(\d{4})-\d{4}第([一二三四])学期", text)
    if match:
        return (0, int(match.group(1)), SEMESTER_NAME_ORDER.get(match.group(2), 9), text)
    return (1, 9999, 9999, text)


def ordered_semesters(values: Iterable[object]) -> list[str]:
    return sorted({str(value) for value in values if pd.notna(value)}, key=semester_sort_key)


def build_department_stats(courses_df: pd.DataFrame, ratings_df: pd.DataFrame) -> pd.DataFrame:
    review_stats = (
        ratings_df.groupby("department", dropna=False)
        .agg(
            review_count=("review_id", "count"),
            avg_review_rating=("rating", "mean"),
            median_review_rating=("rating", "median"),
            reviewed_courses=("course_id", "nunique"),
            avg_approves=("approves", "mean"),
        )
        .reset_index()
    )

    catalog_stats = (
        courses_df.groupby("department", dropna=False)
        .agg(
            catalog_courses=("course_id", "count"),
            catalog_review_count=("catalog_review_count", "sum"),
        )
        .reset_index()
    )
    catalog_avg = (
        courses_df.groupby("department", dropna=False)
        .apply(
            lambda group: weighted_average(
                group["catalog_avg_rating"],
                group["catalog_review_count"],
            ),
            include_groups=False,
        )
        .rename("catalog_weighted_avg_rating")
        .reset_index()
    )

    result = catalog_stats.merge(catalog_avg, on="department", how="left").merge(
        review_stats,
        on="department",
        how="left",
    )
    numeric_cols = [
        "review_count",
        "avg_review_rating",
        "median_review_rating",
        "reviewed_courses",
        "avg_approves",
    ]
    for col in numeric_cols:
        result[col] = result[col].fillna(0)
    result["course_coverage"] = result.apply(
        lambda row: 0
        if row["catalog_courses"] <= 0
        else row["reviewed_courses"] / row["catalog_courses"],
        axis=1,
    )
    result["reviews_per_reviewed_course"] = result.apply(
        lambda row: 0
        if row["reviewed_courses"] <= 0
        else row["review_count"] / row["reviewed_courses"],
        axis=1,
    )
    return result.sort_values(["review_count", "avg_review_rating"], ascending=[False, False])


def build_category_stats(courses_df: pd.DataFrame, ratings_df: pd.DataFrame) -> pd.DataFrame:
    course_categories = courses_df[["course_id", "categories"]].explode("categories")
    course_categories = course_categories.rename(columns={"categories": "category"})
    review_categories = ratings_df.merge(course_categories, on="course_id", how="left")
    review_categories["category"] = review_categories["category"].fillna("未分类")

    stats = (
        review_categories.groupby("category", dropna=False)
        .agg(
            review_count=("review_id", "count"),
            avg_review_rating=("rating", "mean"),
            reviewed_courses=("course_id", "nunique"),
        )
        .reset_index()
        .sort_values(["review_count", "avg_review_rating"], ascending=[False, False])
    )
    return stats


def build_semester_stats(ratings_df: pd.DataFrame) -> pd.DataFrame:
    stats = (
        ratings_df.groupby("semester", dropna=False)
        .agg(
            review_count=("review_id", "count"),
            avg_review_rating=("rating", "mean"),
            unique_courses=("course_id", "nunique"),
        )
        .reset_index()
    )
    stats["_semester_order"] = stats["semester"].map(semester_sort_key)
    return stats.sort_values("_semester_order").drop(columns=["_semester_order"])


def build_course_stats(ratings_df: pd.DataFrame) -> pd.DataFrame:
    cols = ["course_id", "code", "name", "teacher", "department"]
    return (
        ratings_df.groupby(cols, dropna=False)
        .agg(
            review_count=("review_id", "count"),
            avg_review_rating=("rating", "mean"),
            median_review_rating=("rating", "median"),
            latest_review=("created_at_dt", "max"),
            total_approves=("approves", "sum"),
        )
        .reset_index()
        .sort_values(["review_count", "avg_review_rating"], ascending=[False, False])
    )


def build_teacher_stats(ratings_df: pd.DataFrame) -> pd.DataFrame:
    return (
        ratings_df.groupby("teacher", dropna=False)
        .agg(
            review_count=("review_id", "count"),
            avg_review_rating=("rating", "mean"),
            median_review_rating=("rating", "median"),
            reviewed_courses=("course_id", "nunique"),
            departments=("department", lambda values: "、".join(sorted(set(map(str, values)))[:3])),
        )
        .reset_index()
        .sort_values(["review_count", "avg_review_rating"], ascending=[False, False])
    )


def tokenize_comments(comments: Iterable[str]) -> Counter[str]:
    text = "\n".join(comment for comment in comments if isinstance(comment, str))
    text = TEMPLATE_LABEL_RE.sub(" ", text)
    text = re.sub(r"[#*_`~>\[\]()+\-=/\\|:：，。！？、；;,.!?]", " ", text)

    try:
        import jieba

        tokens = jieba.lcut(text)
    except ImportError:
        tokens = re.findall(r"[\u4e00-\u9fff]{2,}|[A-Za-z][A-Za-z0-9+#.]{1,}", text)

    counter: Counter[str] = Counter()
    for token in tokens:
        word = token.strip().lower()
        if not word or word in STOPWORDS:
            continue
        if re.fullmatch(r"\d+", word):
            continue
        if len(word) < 2:
            continue
        counter[word] += 1
    return counter


def clean_markdown_for_wordcloud(markdown: str) -> str:
    kept_lines: list[str] = []
    for line in markdown.splitlines():
        stripped = line.strip()
        if not stripped:
            continue
        if stripped.startswith("#"):
            continue
        if (stripped.startswith("**") and stripped.endswith("**")) or (
            stripped.startswith("__") and stripped.endswith("__")
        ):
            continue
        kept_lines.append(line)
    return "\n".join(kept_lines)


def load_asset_markdown_texts(assets_dir: Path) -> tuple[list[str], list[Path]]:
    if not assets_dir.exists():
        return [], []

    texts: list[str] = []
    loaded_files: list[Path] = []
    for path in sorted(assets_dir.glob("*.md")):
        try:
            markdown = path.read_text(encoding="utf-8")
        except (OSError, UnicodeDecodeError):
            continue
        cleaned = clean_markdown_for_wordcloud(markdown)
        if cleaned.strip():
            texts.append(cleaned)
            loaded_files.append(path)
    return texts, loaded_files


def save_barh(
    df: pd.DataFrame,
    label_col: str,
    value_col: str,
    title: str,
    path: Path,
    top_n: int = 20,
    value_format: str = "{:.0f}",
    x_limit: float | None = None,
) -> None:
    plot_df = df.sort_values(value_col, ascending=True).tail(top_n)
    height = max(4.5, len(plot_df) * 0.34)
    fig, ax = plt.subplots(figsize=(9, height))
    bars = ax.barh(plot_df[label_col].astype(str), plot_df[value_col])
    ax.set_title(title)
    ax.set_xlabel(value_col)
    ax.grid(axis="x", alpha=0.25)
    values = plot_df[value_col].fillna(0).astype(float)
    labels = [value_format.format(value) for value in values]
    ax.bar_label(bars, labels=labels, padding=4, fontsize=8)
    max_value = float(values.max()) if len(values) else 0
    if x_limit is not None:
        ax.set_xlim(0, x_limit)
    elif max_value > 0:
        ax.set_xlim(0, max_value * 1.14)
    fig.tight_layout()
    fig.savefig(path, dpi=180)
    plt.close(fig)


def rating_count_table(
    ratings_df: pd.DataFrame,
    index_col: str,
    index_order: list[str],
) -> pd.DataFrame:
    plot_df = ratings_df.dropna(subset=[index_col, "rating"]).copy()
    plot_df[index_col] = plot_df[index_col].astype(str)
    plot_df["rating"] = plot_df["rating"].astype(int)
    rating_values = [rating for rating in range(1, 6) if rating in set(plot_df["rating"])]
    return pd.crosstab(plot_df[index_col], plot_df["rating"]).reindex(
        index=index_order,
        columns=rating_values,
        fill_value=0,
    )


def save_semester_rating_stack(ratings_df: pd.DataFrame, path: Path) -> None:
    semesters = ordered_semesters(ratings_df["semester"])
    counts = rating_count_table(ratings_df, "semester", semesters)
    labels = semesters
    totals = counts.sum(axis=1)

    fig_width = max(10, len(semesters) * 1.1)
    fig, ax = plt.subplots(figsize=(fig_width, 5.8))
    bottom = pd.Series([0] * len(counts), index=counts.index)

    for rating in counts.columns:
        values = counts[rating]
        ax.bar(
            labels,
            values,
            bottom=bottom,
            label=f"{rating} 星",
            color=RATING_COLORS.get(rating),
            width=0.72,
        )
        bottom += values

    for idx, total in enumerate(totals):
        ax.text(idx, total + max(totals.max() * 0.015, 8), f"{int(total)}", ha="center", va="bottom", fontsize=8)

    ax.set_title("各学期评价数量与星级结构")
    ax.set_xlabel("学期")
    ax.set_ylabel("评价数量")
    ax.set_ylim(0, totals.max() * 1.14)
    ax.grid(axis="y", alpha=0.25)
    ax.tick_params(axis="x", rotation=45)
    for label in ax.get_xticklabels():
        label.set_horizontalalignment("right")
    ax.legend(title="评分", loc="upper left", bbox_to_anchor=(1.01, 1))
    fig.tight_layout()
    fig.savefig(path, dpi=180)
    plt.close(fig)


def save_group_rating_stack(
    ratings_df: pd.DataFrame,
    group_col: str,
    title: str,
    path: Path,
    top_n: int,
    group_order: list[str] | None = None,
    label_map: dict[str, str] | None = None,
) -> None:
    plot_df = ratings_df.dropna(subset=[group_col, "rating"]).copy()
    plot_df[group_col] = plot_df[group_col].astype(str)

    if group_order is None:
        group_order = (
            plot_df.groupby(group_col)["review_id"]
            .count()
            .sort_values(ascending=False)
            .head(top_n)
            .index.tolist()
        )
    else:
        group_order = group_order[:top_n]

    group_order = [group for group in group_order if group and group != "nan"]
    groups = list(reversed(group_order))
    counts = rating_count_table(plot_df, group_col, groups)
    counts = counts[counts.sum(axis=1) > 0]
    y_labels = [label_map.get(group, group) if label_map else group for group in counts.index]
    y_positions = list(range(len(counts)))

    row_height = 0.46 if any("\n" in label for label in y_labels) else 0.36
    height = max(5.5, len(counts) * row_height)
    fig, ax = plt.subplots(figsize=(10.5, height))
    left = pd.Series([0] * len(counts), index=counts.index)

    for rating in counts.columns:
        values = counts[rating]
        ax.barh(
            y_positions,
            values,
            left=left,
            label=f"{rating} 星",
            color=RATING_COLORS.get(rating),
            height=0.72,
        )
        left += values

    totals = counts.sum(axis=1)
    for idx, total in enumerate(totals):
        ax.annotate(
            f"{int(total)}",
            xy=(total, idx),
            xytext=(3, 0),
            textcoords="offset points",
            va="center",
            ha="left",
            fontsize=8,
        )

    ax.set_title(title)
    ax.set_xlabel("评价数量")
    ax.set_yticks(y_positions, y_labels, fontsize=8)
    ax.grid(axis="x", alpha=0.25)
    ax.set_xlim(0, totals.max() * 1.08)
    ax.legend(title="评分", loc="lower right")
    fig.tight_layout()
    fig.savefig(path, dpi=180)
    plt.close(fig)


def save_wordcloud(counter: Counter[str], font_path: Path | None, path: Path) -> bool:
    try:
        from wordcloud import WordCloud
    except ImportError:
        return False

    if not counter:
        return False

    wc = WordCloud(
        width=2200,
        height=1400,
        background_color="white",
        font_path=str(font_path) if font_path else None,
        max_words=500,
        colormap="viridis",
        prefer_horizontal=0.92,
    )
    wc.generate_from_frequencies(dict(counter))
    wc.to_file(str(path))
    return True


def write_markdown_report(
    path: Path,
    summary: dict,
    department_df: pd.DataFrame,
    category_df: pd.DataFrame,
    semester_df: pd.DataFrame,
    course_df: pd.DataFrame,
    teacher_df: pd.DataFrame,
    keywords: Counter[str],
    wordcloud_created: bool,
    write_details: bool,
) -> None:
    def table(df: pd.DataFrame, columns: list[str], limit: int = 8) -> str:
        integer_markers = ("count", "courses", "approves", "total", "unique")
        selected = df.loc[:, columns].head(limit).copy()
        for col in selected.columns:
            if any(marker in col for marker in integer_markers):
                selected[col] = selected[col].map(lambda x: "" if pd.isna(x) else f"{float(x):.0f}")
            elif "coverage" in col:
                selected[col] = selected[col].map(lambda x: "" if pd.isna(x) else f"{float(x) * 100:.1f}%")
            elif pd.api.types.is_float_dtype(selected[col]):
                selected[col] = selected[col].map(lambda x: f"{x:.2f}")
        header = "| " + " | ".join(columns) + " |"
        sep = "| " + " | ".join(["---"] * len(columns)) + " |"
        rows = ["| " + " | ".join(str(v) for v in row) + " |" for row in selected.to_numpy()]
        return "\n".join([header, sep, *rows])

    top_keywords = "、".join(f"{word}({count})" for word, count in keywords.most_common(20))
    asset_count = len(summary.get("wordcloud_asset_files", []))
    asset_note = f"- 词云额外加入旧资料 Markdown：{asset_count} 个文件\n" if asset_count else ""
    if write_details:
        output_files = f"""- `summary.json`：总体统计摘要
- `department_stats.csv`：按学院聚合的评价数量与平均分
- `category_stats.csv`：按课程类别聚合的评价数量与平均分
- `semester_stats.csv`：按学期聚合的评价数量与平均分
- `course_rankings.csv`：按课程聚合的评价数量与平均分
- `teacher_stats.csv`：按教师聚合的评价数量与平均分
- `keyword_frequency.csv`：评论分词后的高频词
- `department_review_rating_stack_top{summary["department_top_n"]}.png`、`teacher_review_rating_stack_top{summary["teacher_top_n"]}.png`、`department_average_rating_top{summary["department_top_n"]}.png`、`semester_rating_stack.png`：统计图
"""
    else:
        output_files = f"""- `report.md`：面向作业报告的文字版摘要
- `department_review_rating_stack_top{summary["department_top_n"]}.png`、`teacher_review_rating_stack_top{summary["teacher_top_n"]}.png`、`department_average_rating_top{summary["department_top_n"]}.png`、`semester_rating_stack.png`：统计图
- 默认不输出 CSV/JSON 明细；需要时使用 `--write-details`
"""

    body = f"""# 乌龙茶课程评价数据分析

## 数据概况

- 课程记录数：{summary["courses_total"]}
- 评价记录数：{summary["ratings_total"]}
- 成功匹配到课程表的评价：{summary["matched_ratings"]} / {summary["ratings_total"]}
- 评价平均分：{summary["avg_rating"]:.2f}
- 评价时间范围：{summary["first_review_at"]} 至 {summary["last_review_at"]}

## 学院评价数量排名

{table(department_df, ["department", "review_count", "avg_review_rating", "reviewed_courses", "catalog_courses", "course_coverage"], limit=15)}

## 课程类别评价排名

{table(category_df, ["category", "review_count", "avg_review_rating", "reviewed_courses"])}

## 学期评价分布

{table(semester_df, ["semester", "review_count", "avg_review_rating", "unique_courses"])}

## 评价最多的课程

{table(course_df, ["name", "teacher", "department", "review_count", "avg_review_rating"], limit=10)}

## 评价最多的老师

{table(teacher_df, ["teacher", "departments", "review_count", "avg_review_rating", "reviewed_courses"], limit=10)}

## 高频词

{top_keywords}

## 输出文件

{output_files}"""
    if wordcloud_created:
        body += "- `wordcloud.png`：评论词云\n"
        body += asset_note
    else:
        body += "- 词云未生成：缺少 `wordcloud` 依赖、字体或有效文本时会跳过\n"

    path.write_text(body, encoding="utf-8")


def remove_detail_outputs(out_dir: Path) -> None:
    for filename in DETAIL_OUTPUT_FILES:
        path = out_dir / filename
        if path.exists():
            path.unlink()


def remove_stale_figures(out_dir: Path) -> None:
    for filename in STALE_FIGURE_FILES:
        path = out_dir / filename
        if path.exists():
            path.unlink()
    for pattern in STALE_FIGURE_PATTERNS:
        for path in out_dir.glob(pattern):
            path.unlink()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Analyze Wulongcha course and review snapshots.")
    parser.add_argument("--courses", type=Path, default=default_path("courses"), help="Course JSON file.")
    parser.add_argument("--ratings", type=Path, default=default_path("ratings"), help="Rating JSON file.")
    parser.add_argument("--out-dir", type=Path, default=REPORT_DIR, help="Report output directory.")
    parser.add_argument("--assets-dir", type=Path, default=ASSETS_DIR, help="Optional Markdown assets for wordcloud.")
    parser.add_argument("--min-course-reviews", type=int, default=5, help="Minimum reviews for top-rated list.")
    parser.add_argument("--department-top-n", type=int, default=50, help="Number of departments shown in charts.")
    parser.add_argument("--teacher-top-n", type=int, default=50, help="Number of teachers shown in charts.")
    parser.add_argument(
        "--write-details",
        action="store_true",
        help="Also write detailed CSV and JSON files. By default only Markdown and figures are written.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    args.out_dir.mkdir(parents=True, exist_ok=True)
    font_path = configure_matplotlib()

    courses = load_json(args.courses)
    ratings = load_json(args.ratings)
    course_lookup = {course.get("id"): course for course in courses}

    courses_df = course_frame(courses)
    ratings_df = rating_frame(ratings, course_lookup)

    department_df = build_department_stats(courses_df, ratings_df)
    category_df = build_category_stats(courses_df, ratings_df)
    semester_df = build_semester_stats(ratings_df)
    course_df = build_course_stats(ratings_df)
    teacher_df = build_teacher_stats(ratings_df)
    asset_texts, asset_files = load_asset_markdown_texts(args.assets_dir)
    keywords = tokenize_comments([*ratings_df["comment"], *asset_texts])

    if args.write_details:
        department_df.to_csv(args.out_dir / "department_stats.csv", index=False, encoding="utf-8-sig")
        category_df.to_csv(args.out_dir / "category_stats.csv", index=False, encoding="utf-8-sig")
        semester_df.to_csv(args.out_dir / "semester_stats.csv", index=False, encoding="utf-8-sig")
        course_df.to_csv(args.out_dir / "course_rankings.csv", index=False, encoding="utf-8-sig")
        teacher_df.to_csv(args.out_dir / "teacher_stats.csv", index=False, encoding="utf-8-sig")

        top_rated = course_df[course_df["review_count"] >= args.min_course_reviews].sort_values(
            ["avg_review_rating", "review_count"],
            ascending=[False, False],
        )
        top_rated.to_csv(args.out_dir / "top_rated_courses.csv", index=False, encoding="utf-8-sig")

        keyword_df = pd.DataFrame(keywords.most_common(300), columns=["word", "count"])
        keyword_df.to_csv(args.out_dir / "keyword_frequency.csv", index=False, encoding="utf-8-sig")
    else:
        remove_detail_outputs(args.out_dir)
    remove_stale_figures(args.out_dir)

    save_group_rating_stack(
        ratings_df,
        "department",
        "学院评价数量与星级结构",
        args.out_dir / f"department_review_rating_stack_top{args.department_top_n}.png",
        top_n=args.department_top_n,
        group_order=department_df[department_df["review_count"] > 0]["department"].astype(str).tolist(),
    )
    teacher_label_map = {
        str(row["teacher"]): f"{row['teacher']}\n{row['departments']}"
        for _, row in teacher_df.iterrows()
    }
    save_group_rating_stack(
        ratings_df,
        "teacher",
        "教师评价数量与星级结构",
        args.out_dir / f"teacher_review_rating_stack_top{args.teacher_top_n}.png",
        top_n=args.teacher_top_n,
        group_order=teacher_df[teacher_df["review_count"] > 0]["teacher"].astype(str).tolist(),
        label_map=teacher_label_map,
    )
    rated_departments = department_df[department_df["review_count"] >= 10].sort_values(
        "avg_review_rating",
        ascending=False,
    )
    save_barh(
        rated_departments,
        "department",
        "avg_review_rating",
        f"学院平均评分 (至少 10 条评价)",
        args.out_dir / f"department_average_rating_top{args.department_top_n}.png",
        top_n=args.department_top_n,
        value_format="{:.2f}",
        x_limit=5.35,
    )
    save_semester_rating_stack(ratings_df, args.out_dir / "semester_rating_stack.png")
    wordcloud_created = save_wordcloud(keywords, font_path, args.out_dir / "wordcloud.png")

    first_review = ratings_df["created_at_dt"].min()
    last_review = ratings_df["created_at_dt"].max()
    summary = {
        "generated_at": datetime.now().isoformat(timespec="seconds"),
        "courses_file": str(args.courses),
        "ratings_file": str(args.ratings),
        "courses_total": int(len(courses_df)),
        "ratings_total": int(len(ratings_df)),
        "matched_ratings": int(ratings_df["matched_course"].sum()),
        "unmatched_ratings": int((~ratings_df["matched_course"]).sum()),
        "departments_total": int(courses_df["department"].nunique()),
        "categories_total": int(courses_df["categories"].explode().nunique()),
        "avg_rating": float(ratings_df["rating"].mean()),
        "first_review_at": "" if pd.isna(first_review) else str(first_review),
        "last_review_at": "" if pd.isna(last_review) else str(last_review),
        "font": str(font_path) if font_path else None,
        "wordcloud_created": wordcloud_created,
        "department_top_n": args.department_top_n,
        "teacher_top_n": args.teacher_top_n,
        "wordcloud_asset_files": [str(path) for path in asset_files],
    }
    if args.write_details:
        write_json(args.out_dir / "summary.json", summary)
    write_markdown_report(
        args.out_dir / "report.md",
        summary,
        department_df,
        category_df,
        semester_df,
        course_df,
        teacher_df,
        keywords,
        wordcloud_created,
        args.write_details,
    )

    print(f"Analyzed {len(courses_df)} courses and {len(ratings_df)} ratings.")
    print(f"Report written to {args.out_dir.resolve()}")
    if args.write_details:
        print("Detailed CSV/JSON files were written.")
    else:
        print("Skipped detailed CSV/JSON files. Use --write-details to include them.")


if __name__ == "__main__":
    main()
