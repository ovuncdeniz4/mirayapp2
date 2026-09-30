#!/usr/bin/env python3
"""Generate PluginImporter .meta for DLLs under Assets/_Project/Scripts/."""
from __future__ import annotations

import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "unity/Kut/Assets/_Project/Scripts"

META = """fileFormatVersion: 2
guid: {guid}
PluginImporter:
  externalObjects: {{}}
  serializedVersion: 2
  iconMap: {{}}
  executionOrder: {{}}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 1
  isExplicitlyReferenced: 0
  validateReferences: 1
  platformData:
  - first:
      Any: 
    second:
      enabled: 1
      settings: {{}}
  - first:
      Editor: Editor
    second:
      enabled: 1
      settings:
        DefaultValueInitialized: true
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def main() -> None:
  for dll in sorted(ROOT.glob("*.dll")):
    meta = dll.with_suffix(dll.suffix + ".meta")
    if meta.exists():
      continue
    meta.write_text(META.format(guid=uuid.uuid4().hex), encoding="utf-8")
    print("wrote", meta.name)


if __name__ == "__main__":
  main()
