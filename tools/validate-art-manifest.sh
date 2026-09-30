#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export ROOT
python3 <<'PY'
import json, os, sys
root = os.environ["ROOT"]
art = os.path.join(root, "unity/Kut/Assets/_Project/Resources/Art")
manifest = os.path.join(root, "content/config/assets_manifest.json")
data = json.load(open(manifest))
missing = []
for e in data["entries"]:
    cat = e["category"]
    aid = e["id"]
    path = os.path.join(art, cat, f"{aid}.png")
    if not os.path.isfile(path):
        missing.append(path)
if missing:
    print("Missing art files:", *missing, sep="\n")
    sys.exit(1)
print(f"OK: {len(data['entries'])} manifest entries have PNGs")
PY
