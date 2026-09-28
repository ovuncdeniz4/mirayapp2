# KUT — Beş Element

Stage A (technical prototype): deterministic **Kut.Core** Match-3 engine + Unity **6.3 LTS** shell (**editor 6.3.3**).

## Kut.Core (no Unity required)

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet test
dotnet run --project tools/Kut.LevelValidator
dotnet run --project tools/Kut.Playtest -- --demo
dotnet run --project tools/Kut.Playtest -- --slice
```

<<<<<<< HEAD
Design docs: [`docs/architecture.md`](docs/architecture.md), [`docs/game-design-ch1.md`](docs/game-design-ch1.md), [`docs/game-design-ch2.md`](docs/game-design-ch2.md). Level schema: `content/schemas/level.schema.json`.
=======
Design: [`docs/gdd-index.md`](docs/gdd-index.md) (UX/UI spec, visual system, chapters). Level schema: `content/schemas/level.schema.json`.
>>>>>>> cursor/non-unity-full-implementation-7e6e

**Chapters 1–2 (levels 1–20)** in `--slice`: Fire/Wind tiles, Fire Bomb (`B`), collection relics, totem tiers, animal move bonuses.

**Vertical slice (`--slice`):** onboarding → permanent spirit animal → home / map → **Chapter 1 levels 1–10** (Wind Chime, mud, stone, vine, Shaman Drum, shrine finale). In-level commands: `swap x1 y1 x2 y2`, `activate x y` (specials), `board`, `quit`. Completing **level_010** unlocks totem/collection tabs in save data.

Interactive terminal board: `dotnet run --project tools/Kut.Playtest`  
Legend: `E` earth, `W` water, `M` mud, `V` vine, `C` wind chime, `D` shaman drum, `#` stone, `·` empty

## Unity shell

1. Install **Unity 6.3 LTS** editor **6.3.3**.
2. Open `unity/Kut`.
3. Sync Core DLL:

```bash
./tools/sync-core-to-unity.sh
```

4. Run `./tools/sync-core-to-unity.sh`, then add **`KutAppBootstrap`** to a scene and press Play ([`docs/unity-setup.md`](docs/unity-setup.md)).

Screens S-01…S-07 match CLI `--slice` per [`docs/ux-ui-spec.md`](docs/ux-ui-spec.md). Optional low-level prototype: `GameplaySessionHost` + `BoardView` + `SwapInputController`.

Architecture: **Input → GameplaySession → Kut.Core → GameEvents → BoardView** (presentation never mutates engine state).
