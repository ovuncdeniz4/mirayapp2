# KUT — Chapter 2 Game Design (Core / CLI)

Chapter 2 adds **Fire** and **Wind** match colors and **Fire Bomb** (2×2 match → 3×3 blast on activate).

## Unlock

Complete **level_010** (Chapter 1 finale) to access Chapter 2 on the map (`requiresChapterComplete` in `content/chapters.json`).

## New mechanics

| Mechanic | Rule |
|----------|------|
| **Fire tile** | Match group `fire`; cleared fire cells **burn adjacent mud** and **break adjacent vine** (like water/earth reactions). |
| **Wind tile** | Match group `wind`; collect via `collect_element`. |
| **Fire Bomb** | Enabled per level (`enableFireBomb`); 2×2 square match spawns bomb; `activate x y` clears 3×3 tiles (costs 1 move). |

Special creation priority: **Drum (5-line) > Chime (4-line) > Fire Bomb (2×2)**.

## Level roster (11–20)

| # | ID | Focus |
|---|-----|--------|
| 11 | level_011 | Fire on board |
| 12 | level_012 | Collect fire |
| 13 | level_013 | Create + activate fire bomb |
| 14 | level_014 | Wind + collect wind |
| 15 | level_015 | Fire burns mud |
| 16 | level_016 | Cascades with 4 elements |
| 17 | level_017 | Vines + fire/wind |
| 18 | level_018 | Stone + bomb + wind |
| 19 | level_019 | Shaman drum returns |
| 20 | level_020 | Shrine mix — chapter finale |

## Meta

- **Collection relics** unlock on specific level completes (`content/collection/catalog.json`).
- **Totem tier 2** after **level_020**.
- **Animal bonus** adds extra moves at level start (see `AnimalBonus`).

## CLI legend (extended)

`F` fire, `A` wind (Air), `B` fire bomb — plus Chapter 1 symbols.
