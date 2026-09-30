# KUT — Architecture

KUT (Beş Element) separates **simulation** from **presentation**. All board mutations live in `Kut.Core`; clients (CLI playtest, Unity shell) send commands and consume `GameEvent`s.

## Layers

```
Input (swap / activate)
    → LevelSession / GameplaySession
    → BoardEngine (Kut.Core)
    → GameEvents
    → BoardView / ASCII renderer (read-only replay)
```

**Rule:** `BoardView` and UI never call `BoardState.SetCell` or otherwise mutate engine state. They replay events or refresh from a read-only snapshot after the engine step completes.

## Projects

| Path | Role |
|------|------|
| `packages/Kut.Core` | Board, match, gravity, reactions, specials, objectives, save, levels |
| `tests/Kut.Core.Tests` | NUnit regression tests |
| `tools/Kut.Playtest` | Terminal demo + vertical slice (`--slice`) |
| `tools/Kut.LevelValidator` | Validates all JSON under `content/levels` |
| `content/levels` | Chapter level data |
| `unity/Kut` | **Unity 2022.3 LTS** shell (Monterey+); optional Unity 6.3 on Ventura+; loads `Kut.Core.dll` from `Assets/Plugins` |

## Element reactions (Chapter 1)

When a tile is cleared, its **former cell** is the reaction source:

- **Water** → cleanses adjacent **mud** (layered HP).
- **Earth** → breaks adjacent **vine** (layered HP).

## Specials (Chapter 1)

- **Wind Chime:** L/T match creation (when enabled); tap/`activate` clears row+column cross.
- **Shaman Drum:** straight line of 5 (when enabled); `activate` clears tiles matching its resonance group.
- **Fire Bomb:** Chapter 2+ (`enableFireBomb`); L/T 5-match spawns bomb; activate clears all tiles in **Manhattan distance ≤ 2** (13 cells on open board; clamped at edges).

## Element reactions (Chapter 2)

- **Fire** cleared → burns adjacent **mud** and breaks adjacent **vine** (same adjacency rules as water/earth).

## Meta (CLI)

- `content/chapters.json` — chapter / level order and gates.
- `content/collection/catalog.json` — relic unlocks on level complete.
- Save schema v2: `TotemTier`, `UnlockedCollectionIds`, `Chapter2Complete`.

## Save model

Persistent meta stores `animalId`, progress, and unlock flags (`TotemTabUnlocked`, `CollectionTabUnlocked`). Birth date is **not** stored—only deterministic assignment inputs at onboarding time.

## Sync to Unity

```bash
./tools/sync-core-to-unity.sh
```

Copies Release `Kut.Core.dll` and all `content/levels/*.json` into the Unity project.

## Content validation

```bash
dotnet run --project tools/Kut.LevelValidator
```

Schema reference: `content/schemas/level.schema.json`.
