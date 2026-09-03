#!/usr/bin/env python3
"""Verify that an Android release artifact is signed by the pinned upload certificate."""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
from pathlib import Path


EXPECTED_CERT_SHA256 = "D8:11:9B:59:52:1A:7F:06:94:90:7C:00:BF:D1:F8:AA:9E:85:46:75:83:27:C9:15:95:0B:4A:E1:E7:14:36:64"


def normalize(fingerprint: str) -> str:
    compact = re.sub(r"[^0-9a-fA-F]", "", fingerprint).upper()
    if len(compact) != 64:
        raise ValueError(f"not a SHA-256 certificate fingerprint: {fingerprint}")
    return ":".join(compact[index:index + 2] for index in range(0, len(compact), 2))


def fail(message: str) -> None:
    print(f"ANDROID SIGNER VERIFICATION FAILED: {message}")
    raise SystemExit(1)


def run_tool(args: list[str]) -> str:
    try:
        result = subprocess.run(args, check=False, capture_output=True, text=True)
    except FileNotFoundError:
        return ""
    return result.stdout + result.stderr


def fingerprints_from_keytool(bundle: Path) -> set[str]:
    output = run_tool(["keytool", "-printcert", "-jarfile", str(bundle)])
    found = set()
    for match in re.finditer(r"SHA256:\s*([0-9A-Fa-f:]{95})", output):
        found.add(normalize(match.group(1)))
    return found


def fingerprints_from_apksigner(bundle: Path) -> set[str]:
    output = run_tool(["apksigner", "verify", "--print-certs", str(bundle)])
    found = set()
    for match in re.finditer(r"SHA-256 digest:\s*([0-9A-Fa-f:]+)", output, re.IGNORECASE):
        found.add(normalize(match.group(1)))
    return found


def fingerprints_from_jarsigner(bundle: Path) -> set[str]:
    output = run_tool(["jarsigner", "-verify", "-verbose", "-certs", str(bundle)])
    found = set()
    for match in re.finditer(r"SHA256(?: digest)?(?: fingerprint)?:\s*([0-9A-Fa-f:]{95})", output, re.IGNORECASE):
        found.add(normalize(match.group(1)))
    return found


def main() -> int:
    parser = argparse.ArgumentParser(description="Verify ClickDungeon2 Android upload-certificate fingerprint.")
    parser.add_argument("bundle", help="Path to ClickDungeon2 Android release artifact")
    parser.add_argument("--expected", default=EXPECTED_CERT_SHA256, help="Expected SHA-256 certificate fingerprint")
    args = parser.parse_args()

    bundle = Path(args.bundle)
    if not bundle.is_file():
        fail(f"Android artifact is missing: {bundle}")

    expected = normalize(args.expected)
    fingerprints = fingerprints_from_keytool(bundle)
    if not fingerprints:
        fingerprints = fingerprints_from_apksigner(bundle)
    if not fingerprints:
        fingerprints = fingerprints_from_jarsigner(bundle)
    if not fingerprints:
        fail("could not read signer certificate fingerprint from Android artifact; ensure keytool, apksigner, or jarsigner is installed")
    if expected not in fingerprints:
        listed = ", ".join(sorted(fingerprints))
        fail(f"expected {expected}, found {listed}")

    print(f"ANDROID SIGNER VERIFICATION PASSED: {expected}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
