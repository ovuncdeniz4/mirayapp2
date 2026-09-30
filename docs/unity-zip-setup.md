# KUT Unity — ZIP download (no Git)

## 1. Download

GitHub → **Code** → **Download ZIP** on branch **`main`** (latest).

## 2. Required file (fixes “Kut.Core does not exist”)

This file **must** exist:

```text
unity/Kut/Assets/_Project/Scripts/Kut.Core.dll
```

Same folder as `Kut.Unity.asmdef`. Size about **56 KB**.

If it is missing, your ZIP is old — download **`main`** again.

## 3. Open in Hub

Folder: **`…/mirayapp2-main/unity/Kut`**  
Editor: **2022.3.62f1**

## 4. First run

**KUT → Create Main Scene And Open** → **Play**

## 5. Still red errors?

1. Quit Unity  
2. Delete **`unity/Kut/Library`**  
3. Reopen the project  
4. In Project window: **Assets → _Project → Scripts** — confirm **Kut.Core** (dll icon)

Manual fix (if DLL only exists under `Assets/Plugins`):

Copy **`Kut.Core.dll`** and **`Kut.Core.dll.meta`** into **`Assets/_Project/Scripts/`**, then restart Unity.
