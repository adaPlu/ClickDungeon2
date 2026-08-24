#!/usr/bin/env python3
"""Audit Unity metadata needed for reproducible imports.

Default mode is advisory so an incomplete first-import project can still reach
Unity activation/compile diagnostics. Pass --strict to make missing/orphaned
.meta files, duplicate GUIDs, or essential ProjectSettings fail the command.

This tool never generates .meta files or GUIDs. Canonical metadata must come
from the intended Unity Editor import and then be committed to version control.
"""

from __future__ import annotations

import argparse
import re
import sys
from collections import defaultdict
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
ASSETS_DIR = REPO_ROOT / "Assets"
PROJECT_SETTINGS_DIR = REPO_ROOT / "ProjectSettings"
ESSENTIAL_PROJECT_SETTINGS = (
    "ProjectVersion.txt",
    "ProjectSettings.asset",
    "EditorBuildSettings.asset",
)
GUID_RE = re.compile(r"^guid:\s*([0-9a-fA-F]{32})\s*$", re.MULTILINE)
IGNORED_NAMES = {".DS_Store", "Thumbs.db"}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Audit Unity .meta sidecars, GUID uniqueness, and ProjectSettings completeness."
    )
    parser.add_argument(
        "--strict",
        action="store_true",
        help="Exit non-zero when reproducibility defects are found.",
    )
    return parser.parse_args()


def asset_entries() -> list[Path]:
    entries: list[Path] = []
    if not ASSETS_DIR.is_dir():
        return entries

    for path in sorted(ASSETS_DIR.rglob("*")):
        if path.name in IGNORED_NAMES or path.name.endswith(".meta"):
            continue
        entries.append(path)
    return entries


def expected_meta_path(asset_path: Path) -> Path:
    return asset_path.with_name(asset_path.name + ".meta")


def existing_meta_files() -> list[Path]:
    if not ASSETS_DIR.is_dir():
        return []
    return sorted(path for path in ASSETS_DIR.rglob("*.meta") if path.is_file())


def relative(path: Path) -> str:
    return path.relative_to(REPO_ROOT).as_posix()


def read_guid(meta_path: Path) -> str | None:
    try:
        text = meta_path.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None
    match = GUID_RE.search(text)
    return match.group(1).lower() if match else None


def audit() -> list[str]:
    defects: list[str] = []

    if not ASSETS_DIR.is_dir():
        defects.append("Assets/ directory is missing")
        entries: list[Path] = []
    else:
        entries = asset_entries()

    missing_meta = [path for path in entries if not expected_meta_path(path).is_file()]
    if missing_meta:
        defects.append(f"{len(missing_meta)} Assets entries are missing .meta sidecars")
        for path in missing_meta[:20]:
            print(f"  missing: {relative(expected_meta_path(path))}")
        if len(missing_meta) > 20:
            print(f"  ... and {len(missing_meta) - 20} more")

    meta_files = existing_meta_files()
    orphaned_meta: list[Path] = []
    guid_to_paths: dict[str, list[Path]] = defaultdict(list)
    missing_guids: list[Path] = []

    for meta_path in meta_files:
        target = meta_path.with_name(meta_path.name[:-5])
        if not target.exists():
            orphaned_meta.append(meta_path)

        guid = read_guid(meta_path)
        if guid is None:
            missing_guids.append(meta_path)
        else:
            guid_to_paths[guid].append(meta_path)

    if orphaned_meta:
        defects.append(f"{len(orphaned_meta)} orphaned .meta files have no matching asset/folder")
        for path in orphaned_meta[:20]:
            print(f"  orphaned: {relative(path)}")

    if missing_guids:
        defects.append(f"{len(missing_guids)} .meta files do not contain a valid 32-hex GUID")
        for path in missing_guids[:20]:
            print(f"  invalid-guid: {relative(path)}")

    duplicate_guids = {
        guid: paths for guid, paths in guid_to_paths.items() if len(paths) > 1
    }
    if duplicate_guids:
        defects.append(f"{len(duplicate_guids)} duplicate Unity GUIDs detected")
        for guid, paths in sorted(duplicate_guids.items()):
            joined = ", ".join(relative(path) for path in paths)
            print(f"  duplicate-guid {guid}: {joined}")

    missing_settings = [
        name for name in ESSENTIAL_PROJECT_SETTINGS
        if not (PROJECT_SETTINGS_DIR / name).is_file()
    ]
    if missing_settings:
        defects.append(
            "essential ProjectSettings files missing: " + ", ".join(missing_settings)
        )
        for name in missing_settings:
            print(f"  missing: ProjectSettings/{name}")

    print(
        "Unity metadata audit: "
        f"assets={len(entries)}, metas={len(meta_files)}, "
        f"projectSettings={len(list(PROJECT_SETTINGS_DIR.glob('*'))) if PROJECT_SETTINGS_DIR.is_dir() else 0}"
    )

    return defects


def main() -> int:
    args = parse_args()
    defects = audit()

    if not defects:
        print("Unity metadata audit passed: canonical sidecars/settings are structurally complete.")
        return 0

    label = "ERROR" if args.strict else "WARNING"
    print(f"{label}: Unity metadata reproducibility defects detected:")
    for defect in defects:
        print(f"  - {defect}")

    if args.strict:
        print(
            "Strict metadata gate failed. Open the project with the intended Unity Editor, "
            "let Unity create canonical metadata/settings, review them, and commit them."
        )
        return 1

    print(
        "Advisory mode only: CI will continue so activation/compile failures remain observable. "
        "Use --strict for the release gate after canonical Unity metadata is committed."
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
