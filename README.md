# KUT — Beş Element

Stage A (technical prototype): deterministic **Kut.Core** Match-3 engine + Unity **6.3 LTS** shell (**editor 6.3.3**).

## Kut.Core (no Unity required)

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet test
dotnet run --project tools/Kut.Playtest -- --demo
dotnet run --project tools/Kut.Playtest -- --slice
```

Interactive terminal board: `dotnet run --project tools/Kut.Playtest`  
Legend: `E` earth, `W` water, `M` mud, `C` wind chime, `·` empty

## Unity shell

1. Install **Unity 6.3 LTS** editor **6.3.3**.
2. Open `unity/Kut`.
3. Sync Core DLL:

```bash
./tools/sync-core-to-unity.sh
```

4. Create/open a scene with `GameplaySessionHost`, `BoardView`, and `SwapInputController`.

Architecture: **Input → GameplaySession → Kut.Core → GameEvents → BoardView** (presentation never mutates engine state).
