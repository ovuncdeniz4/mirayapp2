# KUT — Chapter 1 Game Design (CLI / Core)

Fictional shamanic **match-3** tutorial chapter: **Earth** and **Water** only on the board. **Spirit** is meta (animal totem), not a match color in Ch1.

## Player journey

1. **Ruh Töreni** — birth month/day → permanent spirit animal (saved as `animalId` only).
2. **Home** — continue, map (levels 1–10), animal info, totem/collection after finale.
3. **Levels 1–10** — mechanics introduced one at a time, then combined on the shrine level.

Completing **level_010** unlocks **Totem** and **Collection** tabs (save flags; full UI in Unity later).

## Level roster

| # | ID | Teach / focus | Notable rules |
|---|-----|----------------|---------------|
| 1 | level_001 | Basic matches | No specials |
| 2 | level_002 | Cascades | `cascade_depth` objective |
| 3 | level_003 | Collect element | Earth/water quotas |
| 4 | level_004 | Wind Chime | Create + activate chime |
| 5 | level_005 | Mud + Water reaction | Clear mud obstacles |
| 6 | level_006 | Stone blockers | `#` cells block swaps |
| 7 | level_007 | Vine + Earth reaction | Clear vines |
| 8 | level_008 | Shaman Drum | Straight-5 create + activate |
| 9 | level_009 | Dual collect | Mud + vine pressure |
| 10 | level_010 | Shrine finale | Mud + vine combined |

## Objectives (data-driven)

| type | Meaning |
|------|---------|
| `make_matches` | Count of match steps |
| `cascade_depth` | Max cascade depth in a single turn |
| `collect_element` | Tiles cleared of `earth` / `water` |
| `clear_obstacle` | Mud or vine layers removed |
| `create_special` | Wind chime or shaman drum spawned |
| `activate_special` | Player-triggered special clear |

## CLI playtest

```bash
dotnet run --project tools/Kut.Playtest -- --slice
```

In-level: `swap x1 y1 x2 y2`, `activate x y`, `board`, `quit`.

Legend: `E` earth, `W` water, `M` mud, `V` vine, `C` chime, `D` drum, `#` stone.
