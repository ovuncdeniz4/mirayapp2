#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"
dotnet build "$ROOT/packages/Kut.Core/Kut.Core.csproj" -c Release
ASM_DEST="$ROOT/unity/Kut/Assets/_Project/Scripts"
NUGET="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
mkdir -p "$ASM_DEST"
dotnet restore "$ROOT/packages/Kut.Core/Kut.Core.csproj" -v q
cp "$ROOT/packages/Kut.Core/bin/Release/netstandard2.1/Kut.Core.dll" "$ASM_DEST/Kut.Core.dll"
cp "$ROOT/packages/Kut.Core/bin/Release/netstandard2.1/Kut.Core.pdb" "$ASM_DEST/" 2>/dev/null || true
cp "$NUGET/system.text.json/8.0.5/lib/netstandard2.0/System.Text.Json.dll" "$ASM_DEST/"
cp "$NUGET/system.text.encodings.web/8.0.0/lib/netstandard2.0/System.Text.Encodings.Web.dll" "$ASM_DEST/"
cp "$NUGET/microsoft.bcl.asyncinterfaces/8.0.0/lib/netstandard2.0/Microsoft.Bcl.AsyncInterfaces.dll" "$ASM_DEST/"
cp "$NUGET/system.runtime.compilerservices.unsafe/6.0.0/lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll" "$ASM_DEST/"
python3 "$ROOT/tools/generate-unity-plugin-meta.py" 2>/dev/null || true
rm -f "$ROOT/unity/Kut/Assets/Plugins/Kut.Core.dll" "$ROOT/unity/Kut/Assets/Plugins/Kut.Core.dll.meta"
LEVEL_DEST="$ROOT/unity/Kut/Assets/_Project/Content/Levels"
RES_DEST="$ROOT/unity/Kut/Assets/_Project/Resources/Content"
mkdir -p "$LEVEL_DEST"
mkdir -p "$RES_DEST/levels"
mkdir -p "$RES_DEST/collection"
mkdir -p "$RES_DEST/Config"
cp "$ROOT/content/levels/"*.json "$LEVEL_DEST/"
cp "$ROOT/content/levels/"*.json "$RES_DEST/levels/"
cp "$ROOT/content/chapters.json" "$RES_DEST/chapters.json"
cp "$ROOT/content/collection/catalog.json" "$RES_DEST/collection/catalog.json"
cp "$ROOT/content/config/"*.json "$RES_DEST/Config/"
cp "$ROOT/content/config/tutorials.json" "$RES_DEST/Config/tutorials.json" 2>/dev/null || true
echo "Synced Kut.Core.dll + System.Text.Json deps to $ASM_DEST"
echo "Synced level JSON to $LEVEL_DEST and $RES_DEST/levels"
echo "Synced chapters + collection + config to $RES_DEST"
echo "Art sprites live under unity/Kut/Assets/_Project/Resources/Art (see docs/ART_DIRECTION.md)"
