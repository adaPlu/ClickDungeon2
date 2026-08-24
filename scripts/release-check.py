#!/usr/bin/env python3
"""Aggregate ClickDungeon release blockers without hiding later failures.

Unlike fast CI checks, this gate is intentionally strict: canonical Unity
metadata, production media/provenance, and clean-build policy must all be ready
before a release can pass.
"""

from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
errors: list[str] = []


def run_check(label: str, command: list[str]) -> None:
    print(f"\n=== {label} ===")
    result = subprocess.run(command, cwd=ROOT, check=False)
    if result.returncode != 0:
        errors.append(f"{label} failed (exit {result.returncode})")


def check_production_manifest(path: Path) -> None:
    if not path.is_file():
        errors.append(f"missing release manifest: {path.relative_to(ROOT)}")
        return

    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        errors.append(f"invalid release manifest {path.relative_to(ROOT)}: {exc}")
        return

    assets = data.get("assets", [])
    if not isinstance(assets, list):
        errors.append(f"{path.relative_to(ROOT)}: assets must be a list")
        return

    placeholders = [
        str(row.get("asset_id", "<missing-id>"))
        for row in assets
        if row.get("status") != "production"
    ]
    if placeholders:
        errors.append(
            f"{path.name}: {len(placeholders)} assets are not marked production-ready"
        )


run_check(
    "Canonical content validation",
    [sys.executable, str(ROOT / "scripts" / "validate-content.py")],
)
run_check(
    "Media/provenance validation",
    [sys.executable, str(ROOT / "scripts" / "validate-assets.py")],
)
run_check(
    "Unity metadata reproducibility",
    [sys.executable, str(ROOT / "scripts" / "validate-unity-metadata.py"), "--strict"],
)

check_production_manifest(
    ROOT / "Assets" / "ClickDungeon" / "Art" / "Source" / "asset_manifest.json"
)
check_production_manifest(
    ROOT / "Assets" / "ClickDungeon" / "Audio" / "Source" / "audio_manifest.json"
)

store_root = ROOT / "Store"
if store_root.exists():
    store_placeholders = [
        str(path.relative_to(ROOT))
        for path in store_root.rglob("*")
        if path.is_file()
        and ("Placeholder" in path.parts or "placeholder" in path.name.lower())
    ]
    if store_placeholders:
        errors.append(f"Store assets: {len(store_placeholders)} placeholder files remain")

workflow = ROOT / ".github" / "workflows" / "unity-platform-ci.yml"
if workflow.is_file() and "allowDirtyBuild: true" in workflow.read_text(encoding="utf-8"):
    errors.append(
        "Unity Platform CI still permits dirty builds; disable allowDirtyBuild after canonical .meta/ProjectSettings are committed"
    )

if errors:
    print("\nRELEASE CHECK BLOCKED")
    for error in errors:
        print(" -", error)
    sys.exit(2)

print("\nRELEASE CHECK PASSED")
