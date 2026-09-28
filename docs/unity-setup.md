# Unity setup — KUT MVP UI

## Prerequisites

- Unity Hub + **Unity 6.3 LTS (6.3.3)**
- Repo cloned; on branch with Ch1–Ch2 content

## Steps

1. From repo root:

```bash
./tools/sync-core-to-unity.sh
```

2. Hub → **Add** → `unity/Kut`
3. Create scene **Main** (or use existing):
   - Empty GameObject → Add component **`KutAppBootstrap`**
4. **Play**

UI is built at runtime per [`ux-ui-spec.md`](ux-ui-spec.md). Colors from [`visual-design-system.md`](visual-design-system.md).

## Save & content

- Save file: `Application.persistentDataPath/kut_save.json`
- JSON: `Assets/_Project/Resources/Content/` (sync script copies from `content/`)

## Gameplay input (MVP)

- Board uses **`BoardView`** colored cells.
- Demo: press **`1`** in Play mode triggers a sample swap (0,0)↔(1,0). Wire **`SwapInputController`** + colliders for touch next.

## Architecture

Same as CLI: **Input → GameplaySession / LevelSession → Kut.Core → GameEvents → BoardView**
