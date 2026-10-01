#!/usr/bin/env python3
"""Patch Unity .meta spriteBorder for nine-slice UI sprites."""
from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2] / "unity/Kut/Assets/_Project/Resources/Art"

BORDERS = {
    "UI/ui_panel_frame.png": (32, 32, 32, 32),
    "UI/ui_button_primary.png": (24, 24, 24, 24),
    "UI/ui_board_tray.png": (40, 48, 40, 48),
}


def patch(meta_path: Path, border: tuple[int, int, int, int]) -> None:
    text = meta_path.read_text(encoding="utf-8")
    x, y, z, w = border
    new = f"spriteBorder: {{x: {x}, y: {y}, z: {z}, w: {w}}}"
    text = re.sub(r"spriteBorder: \{x: [^}]+\}", new, text)
    meta_path.write_text(text, encoding="utf-8")
    print("patched", meta_path.name)


def main() -> None:
    for rel, border in BORDERS.items():
        png = ROOT / rel
        meta = png.with_suffix(png.suffix + ".meta")
        if meta.exists():
            patch(meta, border)
        else:
            print("skip missing meta", meta)


if __name__ == "__main__":
    main()
