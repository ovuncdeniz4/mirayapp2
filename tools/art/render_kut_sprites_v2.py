#!/usr/bin/env python3
"""Regenerate KUT art PNGs (v2 pass) per docs/ART_DIRECTION.md — stable asset ids."""
from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[2] / "unity/Kut/Assets/_Project/Resources/Art"
SIZE_TILE = 128


def hex_rgb(h: str) -> tuple[int, int, int]:
    h = h.lstrip("#")
    return tuple(int(h[i : i + 2], 16) for i in (0, 2, 4))


COLORS = {
    "earth": hex_rgb("#4CAF6A"),
    "water": hex_rgb("#3B7BDB"),
    "fire": hex_rgb("#E85D3B"),
    "air": hex_rgb("#7EC8E3"),
    "gold": hex_rgb("#C9A227"),
    "deep": hex_rgb("#1A1520"),
    "panel": hex_rgb("#2A2235"),
    "mud": hex_rgb("#6B4A2E"),
    "vine": hex_rgb("#2E7D4A"),
    "stone": hex_rgb("#5C5C66"),
}


def save(img: Image.Image, category: str, asset_id: str) -> None:
    dest = ROOT / category / f"{asset_id}.png"
    dest.parent.mkdir(parents=True, exist_ok=True)
    img.save(dest, optimize=True)
    print("wrote", dest.relative_to(ROOT.parents[3]))


def enamel_tile(base: tuple[int, int, int], glyph_fn) -> Image.Image:
    img = Image.new("RGBA", (SIZE_TILE, SIZE_TILE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((8, 8, 119, 119), radius=18, fill=base + (255,))
    d.rounded_rectangle((12, 12, 115, 115), radius=16, outline=(255, 255, 255, 80), width=2)
    glyph_fn(d)
    return img.filter(ImageFilter.SHARPEN)


def draw_earth_glyph(d: ImageDraw.ImageDraw) -> None:
    d.ellipse((44, 44, 84, 84), fill=(255, 255, 255, 90))
    d.polygon([(64, 38), (78, 58), (50, 58)], fill=(255, 255, 255, 120))


def draw_water_glyph(d: ImageDraw.ImageDraw) -> None:
    d.polygon([(64, 36), (88, 70), (40, 70)], fill=(255, 255, 255, 110))
    d.arc((48, 72, 80, 92), 0, 180, fill=(255, 255, 255, 100), width=3)


def draw_fire_glyph(d: ImageDraw.ImageDraw) -> None:
    d.polygon([(64, 34), (78, 62), (64, 88), (50, 62)], fill=(255, 255, 255, 120))


def draw_air_glyph(d: ImageDraw.ImageDraw) -> None:
    d.arc((40, 48, 88, 96), 200, 340, fill=(255, 255, 255, 120), width=4)


def render_tiles() -> None:
    save(enamel_tile(COLORS["earth"], draw_earth_glyph), "Tiles", "tile_earth_moss")
    save(enamel_tile(COLORS["water"], draw_water_glyph), "Tiles", "tile_water_drop")
    save(enamel_tile(COLORS["fire"], draw_fire_glyph), "Tiles", "tile_fire_ember")
    save(enamel_tile(COLORS["air"], draw_air_glyph), "Tiles", "tile_air_wisp")
    save(enamel_tile(COLORS["stone"], draw_earth_glyph), "Tiles", "tile_metal_ingot")


def render_specials() -> None:
    chime = enamel_tile(COLORS["gold"], lambda d: (d.line((64, 30, 64, 98), fill=(255, 255, 255, 140), width=4), d.line((30, 64, 98, 64), fill=(255, 255, 255, 140), width=4)))
    save(chime, "Specials", "special_wind_chime")
    drum_e = enamel_tile(COLORS["earth"], lambda d: d.ellipse((36, 36, 92, 92), outline=(255, 255, 255, 130), width=5))
    save(drum_e, "Specials", "special_shaman_drum_earth")
    drum_w = enamel_tile(COLORS["water"], lambda d: d.ellipse((36, 36, 92, 92), outline=(255, 255, 255, 130), width=5))
    save(drum_w, "Specials", "special_shaman_drum_water")
    bomb = Image.new("RGBA", (SIZE_TILE, SIZE_TILE), (0, 0, 0, 0))
    bd = ImageDraw.Draw(bomb)
    bd.ellipse((16, 16, 112, 112), fill=hex_rgb("#FF6B2C") + (255,))
    bd.ellipse((32, 32, 96, 96), outline=(255, 220, 180, 200), width=4)
    save(bomb, "Specials", "special_fire_bomb")


def render_obstacles() -> None:
    for oid, col in [("obstacle_mud", COLORS["mud"]), ("obstacle_vine", COLORS["vine"]), ("obstacle_stone", COLORS["stone"])]:
        img = Image.new("RGBA", (SIZE_TILE, SIZE_TILE), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.rounded_rectangle((10, 10, 118, 118), radius=12, fill=col + (255,))
        d.rounded_rectangle((18, 18, 110, 110), radius=10, fill=tuple(max(0, c - 30) for c in col) + (255,))
        save(img, "Obstacles", oid)


def render_ui() -> None:
    tray = Image.new("RGBA", (512, 640), COLORS["deep"] + (255,))
    td = ImageDraw.Draw(tray)
    td.rounded_rectangle((24, 48, 488, 592), radius=32, fill=COLORS["panel"] + (255,))
    td.rounded_rectangle((40, 64, 472, 576), radius=24, outline=COLORS["gold"] + (120,), width=3)
    for cx, cy in [(56, 72), (456, 72), (56, 568), (456, 568)]:
        td.ellipse((cx - 8, cy - 8, cx + 8, cy + 8), fill=COLORS["gold"] + (180,))
    save(tray, "UI", "ui_board_tray")

    frame = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    fd = ImageDraw.Draw(frame)
    fd.rounded_rectangle((8, 8, 248, 248), radius=28, fill=COLORS["panel"] + (240,))
    fd.rounded_rectangle((16, 16, 240, 240), radius=22, outline=COLORS["gold"] + (160,), width=6)
    save(frame, "UI", "ui_panel_frame")

    btn = Image.new("RGBA", (256, 96), (0, 0, 0, 0))
    bd = ImageDraw.Draw(btn)
    bd.rounded_rectangle((4, 4, 252, 92), radius=20, fill=COLORS["gold"] + (255,))
    bd.rounded_rectangle((8, 8, 248, 88), radius=18, outline=(255, 255, 255, 80), width=2)
    save(btn, "UI", "ui_button_primary")

    recess = Image.new("RGBA", (SIZE_TILE, SIZE_TILE), (0, 0, 0, 0))
    rd = ImageDraw.Draw(recess)
    rd.rounded_rectangle((6, 6, 122, 122), radius=14, fill=(20, 16, 28, 255))
    rd.rounded_rectangle((10, 10, 118, 118), radius=12, outline=(0, 0, 0, 180), width=3)
    save(recess, "UI", "ui_cell_recess")

    wheel = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    wd = ImageDraw.Draw(wheel)
    wd.ellipse((32, 32, 480, 480), fill=COLORS["deep"] + (230,), outline=COLORS["gold"] + (200,), width=4)
    for i, col in enumerate([COLORS["earth"], COLORS["water"], COLORS["fire"], COLORS["air"]]):
        a0 = i * math.pi / 2
        a1 = (i + 1) * math.pi / 2
        wd.pieslice((64, 64, 448, 448), math.degrees(a0), math.degrees(a1), fill=col + (200,))
    save(wheel, "UI", "ui_glyph_wheel")


def render_map() -> None:
    for cid, top, bottom in [("map_bg_ch1", hex_rgb("#1A2830"), hex_rgb("#2A4038")), ("map_bg_ch2", hex_rgb("#281A18"), hex_rgb("#403028"))]:
        img = Image.new("RGBA", (1080, 1920), top + (255,))
        d = ImageDraw.Draw(img)
        for y in range(0, 1920, 80):
            t = y / 1920
            c = tuple(int(top[i] * (1 - t) + bottom[i] * t) for i in range(3))
            d.rectangle((0, y, 1080, y + 80), fill=c + (255,))
        save(img, "Map", cid)

    mist = Image.new("RGBA", (1080, 400), (200, 210, 220, 90))
    save(mist, "Map", "map_mist_overlay")


def render_simple_icon(category: str, asset_id: str, accent: tuple[int, int, int]) -> None:
    img = Image.new("RGBA", (256, 256), COLORS["deep"] + (255,))
    d = ImageDraw.Draw(img)
    d.ellipse((48, 48, 208, 208), fill=accent + (255,))
    d.ellipse((64, 64, 192, 192), outline=(255, 255, 255, 100), width=4)
    save(img, category, asset_id)


def render_meta_icons() -> None:
    animals = ["wolf", "eagle", "bear", "deer", "salamander"]
    accents = [COLORS["stone"], COLORS["gold"], COLORS["mud"], COLORS["vine"], COLORS["fire"]]
    for a, ac in zip(animals, accents):
        render_simple_icon("Animals", f"animal_{a}", ac)
    for i in range(1, 6):
        render_simple_icon("Totem", f"totem_stage_{i}", COLORS["gold"])
    relics = [
        "relic_chime_shard", "relic_mud_bead", "relic_stone_mark", "relic_vine_knot", "relic_drum_skin",
        "relic_shrine_ember", "relic_fire_seed", "relic_bomb_crystal", "relic_wind_feather", "relic_ch2_finale",
    ]
    for r in relics:
        render_simple_icon("Relics", r, COLORS["gold"])


def main() -> None:
    render_tiles()
    render_specials()
    render_obstacles()
    render_ui()
    render_map()
    render_meta_icons()
    print("Done v2 art pass.")


if __name__ == "__main__":
    main()
