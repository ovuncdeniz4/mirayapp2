# Kut.Core plugin

`Kut.Core.dll` is **committed** so Unity opens without .NET on your Mac.

After changing **`packages/Kut.Core`**, rebuild from repo root:

```bash
./tools/sync-core-to-unity.sh
```

If Unity still shows missing `Kut.Core` types: reimport this folder or restart the Editor.
