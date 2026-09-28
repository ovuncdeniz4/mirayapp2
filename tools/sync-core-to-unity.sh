#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"
dotnet build "$ROOT/packages/Kut.Core/Kut.Core.csproj" -c Release
DEST="$ROOT/unity/Kut/Assets/Plugins"
mkdir -p "$DEST"
cp "$ROOT/packages/Kut.Core/bin/Release/netstandard2.1/Kut.Core.dll" "$DEST/Kut.Core.dll"
cp "$ROOT/packages/Kut.Core/bin/Release/netstandard2.1/Kut.Core.pdb" "$DEST/" 2>/dev/null || true
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
echo "Synced Kut.Core.dll to $DEST"
echo "Synced level JSON to $LEVEL_DEST and $RES_DEST/levels"
echo "Synced chapters + collection + config to $RES_DEST"
echo "Art sprites live under unity/Kut/Assets/_Project/Resources/Art (see docs/ART_DIRECTION.md)"
