# Unity setup — KUT MVP UI

## Which Unity version?

| Your Mac | Unity Editor |
|----------|----------------|
| **macOS Monterey 12** (e.g. MacBook Air 2015) | **2022.3 LTS** — repo pinned **`2022.3.62f1`**; any **2022.3.x** from Hub is OK |
| **macOS Ventura 13+** | Optional **Unity 6.3 LTS** (see [`architecture.md`](architecture.md)); primary repo target is **2022.3** for older Macs |

GitHub Desktop is **not** required — only the cloned repo folder.

## Prerequisites

- Unity Hub + **Unity 2022.3 LTS** (Intel editor on older Macs)
- **.NET SDK 8** (for `./tools/sync-core-to-unity.sh`)
- Repo cloned

## Steps (2022.3 — Monterey / 2015 Mac)

1. From repo root:

```bash
./tools/sync-core-to-unity.sh
```

2. Hub → **Add** → select folder **`unity/Kut`** (open with **2022.3 LTS**).
3. First open may take several minutes (import art + scripts). If Hub shows a **patch mismatch** (e.g. you installed 2022.3.76f1), allow Unity to **retarget** the project — stay on 2022.3 LTS.
4. Menu **KUT → Create Main Scene And Open** (creates `Assets/_Project/Scenes/Main.unity` with **`KutAppBootstrap`**).
5. **Play**

UI is built at runtime per [`ux-ui-spec.md`](ux-ui-spec.md). Sprites: [`ART_DIRECTION.md`](ART_DIRECTION.md).

## Save & content

- Save: `Application.persistentDataPath/kut_save.json`
- Levels/config: `Assets/_Project/Resources/Content/` (sync script copies from `content/`)

## Board input (MVP)

- **`BoardGridUi`**: tap cell → tap adjacent to swap; tap special twice to activate.
- Legacy prototype: **`GameplaySessionHost` + `BoardView` + `SwapInputController`**.

## Architecture

**Input → LevelSession / GameplaySession → Kut.Core → events → UI** (presentation never mutates engine state).

## Troubleshooting

| Issue | Fix |
|-------|-----|
| `Kut.Core` / missing types | Run `./tools/sync-core-to-unity.sh` again. |
| Pink sprites | Select `Resources/Art` → Reimport. |
| `com.unity.ugui` errors | Use **2022.3**; do not open this project in Unity 6 without a separate branch. |
