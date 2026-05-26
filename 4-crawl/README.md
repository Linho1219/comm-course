# 乌龙茶课程评价采集与分析

这是数据通讯课爬虫实践的小项目。项目目标不是绕过 Cloudflare 或自动化登录，而是在人类完成 challenge 和登录之后，复用浏览器登录态调用乌龙茶公开给前端使用的 JSON API，并把采集结果整理成可复现的统计分析。

## 目录结构

```text
4-crawl/
  crawl.py                 # 启动真实 Chrome/Edge，人工登录后分页导出 API 数据
  analyze.py               # 读取 JSON 快照，输出 CSV、图表、词云和 Markdown 报告
  requirements.txt         # Python 依赖
  data/                    # 当前采集的数据快照
  assets/                  # 词云可选补充语料，例如旧乌龙茶 Markdown
  report/                  # 分析输出
```

## 环境准备

```powershell
cd D:\Github\comm-course\4-crawl
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

不需要运行 `python -m playwright install chromium`。采集脚本默认连接本机已安装的 Chrome/Edge，只使用 Playwright 的 CDP 客户端能力。

## 采集数据

```powershell
python crawl.py --targets courses ratings
```

脚本会按顺序寻找并启动真实浏览器：

1. `C:\Program Files\Google\Chrome\Application\chrome.exe`
2. `C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe`
3. `C:\Program Files\Microsoft\Edge\Application\msedge.exe`
4. `C:\Program Files (x86)\Google\Chrome\Application\chrome.exe`

启动时会自动带上 `--remote-debugging-port=9222` 和独立 profile：`.browser-state/<browser>-cdp`。脚本会把 profile 转成绝对路径传给浏览器，避免 Chrome 对相对路径的数据目录报读写错误。先在打开的浏览器里手动完成 Cloudflare challenge 和乌龙茶登录，确认页面可正常访问后回到终端按 Enter。之后脚本会通过 CDP 连接这个浏览器，并顺序请求：

- `https://1.tongji.icu/api/course/?&page=<page>&size=100`
- `https://1.tongji.icu/api/review/?&page=<page>&size=100`

输出文件位于 `data/`：

- `wlc.courses.raw.json`、`wlc.ratings.raw.json`：保留 API 原始分页结构
- `wlc.courses.json`、`wlc.ratings.json`：将每页 `results` 展平后的记录
- `metadata.json`：采集时间、页数和记录数

调试时可以限制页数：

```powershell
python crawl.py --targets ratings --max-pages 3
```

如果 9222 端口被占用，可以换一个端口：

```powershell
python crawl.py --remote-debugging-port 9333 --targets courses ratings
```

如果浏览器安装在别的位置，可以显式指定：

```powershell
python crawl.py --executable-path "D:\Tools\Chrome\Application\chrome.exe"
```

如果想直接使用 Chrome/Edge 的默认配置文件，可以这样运行：

```powershell
python crawl.py --use-default-profile --targets courses ratings
```

使用默认配置文件时，如果 Chrome/Edge 已经在运行，Windows 通常只会把新网址交给已有进程，新的 `--remote-debugging-port` 不会生效。遇到脚本一直等不到 CDP 端口时，先关闭所有对应浏览器窗口再运行，或者改回独立 profile。

如果你已经手动启动好了带 remote debugging 的浏览器，也可以让脚本直接连接：

```powershell
python crawl.py --cdp-url http://127.0.0.1:9222 --targets courses ratings
```

## 离线分析

直接使用新采集数据：

```powershell
python analyze.py
```

默认读取 `data/wlc.courses.json` 和 `data/wlc.ratings.json`。也可以显式指定其它 JSON 快照：

```powershell
python analyze.py --courses data/wlc.courses.json --ratings data/wlc.ratings.json
```

默认分析输出位于 `report/`：

- `report.md`：面向作业报告的文字版摘要
- `*.png`：学院排名、按学期的星级比例堆积图、学期趋势和词云图。学院图默认展示 Top 50，并在柱子末端标出具体数值。

词云会额外尝试读取 `assets/*.md` 作为旧资料语料。读取失败或目录不存在时会自动跳过。Markdown 清洗时会去掉 `#` 开头的标题行，以及整行都是 `**加粗**` 或 `__加粗__` 的分隔/标记行。

如果需要保留 CSV/JSON 明细，使用：

```powershell
python analyze.py --write-details
```

这会额外输出：

- `summary.json`：总体统计
- `department_stats.csv`：按学院统计评价数量、平均分、课程数
- `category_stats.csv`：按课程类别统计
- `semester_stats.csv`：按学期统计
- `course_rankings.csv`、`top_rated_courses.csv`：课程维度排名
- `keyword_frequency.csv`：评论高频词

## 方法说明

采集脚本使用真实 Chrome/Edge 的独立 profile 保存本地登录态，并通过 Chrome DevTools Protocol 执行同源 `fetch`。Cloudflare challenge 和登录都由人手完成。这样能避免把课程作业写成反爬绕过工具，也更符合“观察网页前端如何调用后端 API，然后复现数据请求”的教学目标。

分析脚本把评价中的 `course.id` 回连到课程表，补齐学院和课程类别，再做聚合统计。评论分词前会先清理乌龙茶评价模板里的 `课程内容：`、`上课自由度：`、`考核标准：`、`授课质量：` 标题，避免模板词污染词云。`老乌龙茶搬运` 会按历史资料处理，排在学期时间线最前面。主要分析维度包括：

- 学院评价数量排名与平均评分
- 课程类别评价数量与平均评分
- 各学期评价数量和平均评分变化
- 评价最多、评分最高的课程
- 评论文本高频词和词云

## 数据边界

乌龙茶评价里可能包含个人化表达。报告输出默认只保留聚合统计和词频，不把原始评论复制进 Markdown 报告。提交作业时建议说明数据来源、采集时间和登录态处理方式，并避免公开传播原始评论全文。
