#!/usr/bin/env python3
"""Validate a downloaded Unity CI artifact set for release readiness."""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


EXPECTED = (
    ("windows", "ClickDungeon2-Windows"),
    ("android", "ClickDungeon2-Android-AAB"),
    ("webgl", "ClickDungeon2-WebGL"),
)


def verify_android_release_signing(artifact_dir: Path, errors: list[str]) -> None:
    bundles = sorted(artifact_dir.rglob("*.aab"))
    if len(bundles) != 1:
        errors.append(
            f"expected exactly one Android App Bundle for release signing verification, found {len(bundles)}"
        )
        return

    result = subprocess.run(
        [
            sys.executable,
            str(ROOT / "scripts" / "verify-android-signer.py"),
            str(bundles[0]),
        ],
        cwd=ROOT,
        check=False,
    )
    if result.returncode != 0:
        errors.append("Android App Bundle upload certificate did not match the pinned release signer")


def main() -> int:
    artifact_root = Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "release-artifacts"
    artifact_root = artifact_root.resolve()
    errors: list[str] = []

    if not artifact_root.is_dir():
        errors.append(f"artifact root is missing: {artifact_root}")
    else:
        for platform, name in EXPECTED:
            path = artifact_root / name
            if not path.is_dir():
                errors.append(f"missing downloaded artifact directory: {name}")
                continue
            result = subprocess.run(
                [
                    sys.executable,
                    str(ROOT / "scripts" / "inspect-build-artifact.py"),
                    platform,
                    str(path),
                ],
                cwd=ROOT,
                check=False,
            )
            if result.returncode != 0:
                errors.append(f"{name} failed artifact inspection")
                continue
            if platform == "android":
                verify_android_release_signing(path, errors)

    if errors:
        print("RELEASE ARTIFACT VERIFICATION FAILED")
        for error in errors:
            print(" -", error)
        return 1

    print("RELEASE ARTIFACT VERIFICATION PASSED")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
