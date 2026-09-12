#!/usr/bin/env python3
from pathlib import Path
import argparse
import hashlib
import sys
import zipfile


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def fail(message: str) -> None:
    print(f"ARTIFACT INSPECTION FAILED: {message}")
    raise SystemExit(1)


def nonempty_file(path: Path, label: str) -> Path:
    if not path.is_file():
        fail(f"{label} is missing: {path}")
    if path.stat().st_size <= 0:
        fail(f"{label} is empty: {path}")
    return path


def inspect_windows(root: Path) -> None:
    exe = nonempty_file(root / "ClickDungeon2.exe", "Windows executable")
    data_dir = root / "ClickDungeon2_Data"
    if not data_dir.is_dir():
        fail(f"Windows data directory is missing: {data_dir}")
    files = [p for p in data_dir.rglob("*") if p.is_file() and p.stat().st_size > 0]
    if not files:
        fail("Windows data directory contains no non-empty files")
    total = exe.stat().st_size + sum(p.stat().st_size for p in files)
    print(
        "WINDOWS ARTIFACT OK: "
        f"exe={exe.stat().st_size} bytes data_files={len(files)} total_bytes={total} "
        f"exe_sha256={sha256(exe)}"
    )


def inspect_android(root: Path) -> None:
    bundle = nonempty_file(root / "ClickDungeon2.aab", "Android App Bundle")
    if not zipfile.is_zipfile(bundle):
        fail("Android App Bundle is not a valid ZIP container")
    with zipfile.ZipFile(bundle) as archive:
        corrupt = archive.testzip()
        if corrupt:
            fail(f"Android App Bundle contains a corrupt entry: {corrupt}")
        names = archive.namelist()
        if not any(name == "base/manifest/AndroidManifest.xml" or name.startswith("base/manifest/") for name in names):
            fail("Android App Bundle does not contain a base manifest")
        if not any(name.startswith("base/dex/") or name.startswith("base/root/") or name.startswith("base/lib/") for name in names):
            fail("Android App Bundle does not contain expected base application payload")
        signatures = [
            name
            for name in names
            if name.upper().startswith("META-INF/")
            and name.upper().endswith((".RSA", ".DSA", ".EC"))
        ]
        signature_manifests = [
            name
            for name in names
            if name.upper().startswith("META-INF/")
            and name.upper().endswith(".SF")
        ]
        if not signatures or not signature_manifests:
            fail("Android App Bundle does not contain signing metadata")
    print(
        "ANDROID ARTIFACT OK: "
        f"bytes={bundle.stat().st_size} entries={len(names)} sha256={sha256(bundle)}"
    )


def inspect_android_apk(root: Path) -> None:
    apk = nonempty_file(root / "ClickDungeon2.apk", "Android APK")
    if not zipfile.is_zipfile(apk):
        fail("Android APK is not a valid ZIP container")
    with zipfile.ZipFile(apk) as archive:
        corrupt = archive.testzip()
        if corrupt:
            fail(f"Android APK contains a corrupt entry: {corrupt}")
        names = archive.namelist()
        if "AndroidManifest.xml" not in names:
            fail("Android APK does not contain AndroidManifest.xml")
        if not any(name.endswith(".dex") or name.startswith("lib/") or name.startswith("assets/") for name in names):
            fail("Android APK does not contain expected application payload")
    print(
        "ANDROID APK ARTIFACT OK: "
        f"bytes={apk.stat().st_size} entries={len(names)} sha256={sha256(apk)}"
    )


def inspect_webgl(root: Path) -> None:
    index = nonempty_file(root / "index.html", "WebGL index.html")
    build_dir = root / "Build"
    if not build_dir.is_dir():
        fail(f"WebGL Build directory is missing: {build_dir}")
    files = [p for p in build_dir.rglob("*") if p.is_file() and p.stat().st_size > 0]
    wasm = [p for p in files if ".wasm" in p.name]
    loaders = [p for p in files if ".loader.js" in p.name]
    data = [p for p in files if ".data" in p.name]
    framework = [p for p in files if ".framework.js" in p.name]
    if not wasm:
        fail("WebGL build has no non-empty WASM payload")
    if not loaders:
        fail("WebGL build has no non-empty loader JavaScript")
    if not data:
        fail("WebGL build has no non-empty data payload")
    if not framework:
        fail("WebGL build has no non-empty framework JavaScript")
    total = index.stat().st_size + sum(p.stat().st_size for p in files)
    print(
        "WEBGL ARTIFACT OK: "
        f"build_files={len(files)} total_bytes={total} "
        f"wasm={len(wasm)} loader={len(loaders)} data={len(data)} framework={len(framework)}"
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate ClickDungeon2 player-build artifact structure.")
    parser.add_argument("platform", choices=("windows", "android", "android-apk", "webgl"))
    parser.add_argument("root", help="Platform artifact root directory")
    args = parser.parse_args()
    root = Path(args.root)
    if not root.is_dir():
        fail(f"artifact root is missing: {root}")
    if args.platform == "windows":
        inspect_windows(root)
    elif args.platform == "android":
        inspect_android(root)
    elif args.platform == "android-apk":
        inspect_android_apk(root)
    else:
        inspect_webgl(root)
    return 0


if __name__ == "__main__":
    sys.exit(main())
