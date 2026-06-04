# AutoNavi Tile Stitcher

Download AutoNavi tiles from `wprd01` through `wprd04` in round-robin order, then stitch them into one image.

## Setup

```powershell
python -m venv venv
.\venv\Scripts\python.exe -m pip install -r requirements.txt
```

## Usage

```powershell
.\venv\Scripts\python.exe .\tile_stitcher.py
```

Default range:

- `x`: `420-450`
- `y`: `190-220`
- `z`: `9`
- concurrency: `10`

Custom range and output:

```powershell
.\venv\Scripts\python.exe .\tile_stitcher.py --x-min 420 --x-max 450 --y-min 190 --y-max 220 --concurrency 16 --output output/map.png
```

Useful options:

- `--x-min`, `--x-max`, `--y-min`, `--y-max`: inclusive tile ranges
- `--z`: zoom level
- `--concurrency`: number of concurrent download workers
- `--output`: stitched image path
- `--cache-dir`: downloaded tile cache directory
- `--no-cache`: force fresh downloads and skip writing cache
