# KUT — Beş Element

Deterministic **Kut.Core** Match-3 + Unity **2022.3 LTS** shell (macOS Monterey / older Macs). Unity **6.3 LTS** remains optional on **macOS 13+**.

## Kut.Core (no Unity required)

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet test
dotnet run --project tools/Kut.LevelValidator
dotnet run --project tools/Kut.Playtest -- --demo
dotnet run --project tools/Kut.Playtest -- --slice
```

Design: [`docs/gdd-index.md`](docs/gdd-index.md). Level schema: `content/schemas/level.schema.json`.

**Chapters 1–2 (levels 1–20)** in `--slice`: Fire/Wind tiles, Fire Bomb, collection relics, totem tiers, animal move bonuses.

**Vertical slice (`--slice`):** onboarding → spirit animal → home / map → levels 1–20. CLI: `swap`, `activate`, `board`, `quit`. **level_010** unlocks totem/collection.

Legend: `E` earth, `W` water, `F` fire, `A` wind, `M` mud, `V` vine, `C` chime, `D` drum, `B` bomb, `#` stone, `·` empty

## Unity (MacBook Air 2015 / Monterey)

1. Install **Unity Hub** + **2022.3 LTS** (any 2022.3.x patch).
2. Clone repo; open folder **`unity/Kut`** in Hub.
3. Sync Core:

```bash
./tools/sync-core-to-unity.sh
```

4. Open **`Assets/_Project/Scenes/Main.unity`** and press **Play**. The **KUT → Create Main Scene And Open** menu remains available as a repair utility ([`docs/unity-setup.md`](docs/unity-setup.md)).

Screens S-01…S-07 match CLI `--slice` per [`docs/ux-ui-spec.md`](docs/ux-ui-spec.md).

Architecture: **Input → GameplaySession → Kut.Core → GameEvents → UI** (presentation never mutates engine state).
