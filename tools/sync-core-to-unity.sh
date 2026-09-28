#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"
dotnet build "$ROOT/packages/Kut.Core/Kut.Core.csproj" -c Release
DEST="$ROOT/unity/Kut/Assets/Plugins"
mkdir -p "$DEST"
cp "$ROOT/packages/Kut.Core/bin/Release/netstandard2.1/Kut.Core.dll" "$DEST/Kut.Core.dll"
cp "$ROOT/packages/Kut.Core/bin/Release/netstandard2.1/Kut.Core.pdb" "$DEST/" 2>/dev/null || true
echo "Synced Kut.Core.dll to $DEST"
