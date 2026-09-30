# KUT Unity — ZIP download (no Git)

## 1. Download

GitHub → **Code** → **Download ZIP** on branch **`main`** (latest).

## 2. Required file (fixes “Kut.Core does not exist”)

This file **must** exist:

In **`unity/Kut/Assets/_Project/Scripts/`** (same folder as `Kut.Unity.asmdef`):

| File | ~size |
|------|--------|
| `Kut.Core.dll` | 56 KB |
| `System.Text.Json.dll` | 595 KB |
| `System.Text.Encodings.Web.dll` | 79 KB |
| `Microsoft.Bcl.AsyncInterfaces.dll` | 27 KB |
| `System.Runtime.CompilerServices.Unsafe.dll` | 18 KB |

Also remove **`Assets/Plugins/Kut.Core.dll`** if present (duplicate causes extra errors).

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

Copy all **`*.dll`** and **`*.dll.meta`** from a fresh ZIP’s **`Assets/_Project/Scripts/`** into yours. Delete **`Assets/Plugins/Kut.Core.dll`** if it exists.
