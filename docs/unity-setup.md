# Unity setup — KUT MVP UI

## Which Unity version?

| Your Mac | Unity Editor |
|----------|----------------|
| **macOS Monterey 12** (e.g. MacBook Air 2015) | **`2022.3.62f1`** only for **Personal** — see [license note](#personal-license-not-extended-lts) below |
| **macOS Ventura 13+** | Optional **Unity 6.3 LTS** (see [`architecture.md`](architecture.md)); primary repo target is **2022.3** for older Macs |

GitHub Desktop is **not** required — only the cloned repo folder.

## Prerequisites

- Unity Hub + **Unity 2022.3 LTS** (Intel editor on older Macs)
- **.NET SDK 8** (for `./tools/sync-core-to-unity.sh`)
- Repo cloned

## Steps (2022.3 — Monterey / 2015 Mac)

1. **`Kut.Core.dll`** must be at **`Assets/_Project/Scripts/Kut.Core.dll`** (next to `Kut.Unity.asmdef`). ZIP users: [`unity-zip-setup.md`](unity-zip-setup.md).  
   After you change C# in `packages/Kut.Core`, run from repo root:

```bash
./tools/sync-core-to-unity.sh
```

2. Hub → **Add** → select folder **`unity/Kut`** (open with **2022.3 LTS**).
3. First open may take several minutes (import art + scripts). Open the project with **`2022.3.62f1`** (not Extended LTS builds).
4. Open the committed **`Assets/_Project/Scenes/Main.unity`** scene.
5. **Play**. Use **KUT → Create Main Scene And Open** only if the scene needs to be repaired.

UI is built at runtime per [`ux-ui-spec.md`](ux-ui-spec.md). Sprites: [`ART_DIRECTION.md`](ART_DIRECTION.md).

## Save & content

- Save: `Application.persistentDataPath/kut_save.json`
- Levels/config: `Assets/_Project/Resources/Content/` (sync script copies from `content/`)

## Board input (MVP)

- **`BoardGridUi`**: tap cell → tap adjacent to swap; tap special twice to activate.
- Legacy prototype: **`GameplaySessionHost` + `BoardView` + `SwapInputController`**.

## Architecture

**Input → LevelSession / GameplaySession → Kut.Core → events → UI** (presentation never mutates engine state).

## Personal license — not Extended LTS

Unity **Personal / Pro** cannot run **Extended LTS** installers (e.g. **2022.3.63+**, **74f1**, **76f1** — often labeled **“3-year LTS”**). You will see:

> *This build of Unity 2022 is part of an Extended LTS release, which requires Unity Industry or Enterprise.*

**Fix (Monterey + Personal):**

1. Hub → **Installs** → remove the Extended LTS 2022 build if installed.
2. Install **`2022.3.62f1`** — [Download Archive](https://unity.com/releases/editor/archive) → **2022.3.62f1** → **macOS Intel** (2015 MacBook Air).
3. Hub **3.10+** may list only license-compatible builds under **Installs**; if Hub still offers a too-new 2022 patch, use **Visit Download Archive** from the error dialog or the link above.
4. Open `unity/Kut` with **2022.3.62f1** (repo `ProjectVersion.txt` matches this pin).

Public **2022.3 LTS** for Personal ended at **62f1** (May 2025); later patches are Enterprise/Industry only.

## Troubleshooting

| Issue | Fix |
|-------|-----|
| `Kut.Core` / missing types | Run `./tools/sync-core-to-unity.sh` again. |
| Pink sprites | Select `Resources/Art` → Reimport. |
| `com.unity.ugui` errors | Use **2022.3**; do not open this project in Unity 6 without a separate branch. |
