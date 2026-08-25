#!/usr/bin/env python3
from __future__ import annotations

from pathlib import Path
import binascii
import colorsys
import hashlib
import json
import math
import struct
import wave
import zlib

ROOT = Path(__file__).resolve().parents[1]
CONTENT = ROOT / "Assets" / "ClickDungeon" / "Content" / "Json"
ART_RUNTIME = ROOT / "Assets" / "ClickDungeon" / "Art" / "Runtime"
ART_SOURCE = ROOT / "Assets" / "ClickDungeon" / "Art" / "Source"
AUDIO_RUNTIME = ROOT / "Assets" / "ClickDungeon" / "Audio" / "Runtime"
AUDIO_SOURCE = ROOT / "Assets" / "ClickDungeon" / "Audio" / "Source"

USAGE_TERMS = (
    "Original deterministic code-generated asset for ClickDungeon2; no third-party "
    "source material, stock media, trademarks, or external training references."
)

ICON_IDS = [
    "big_key",
    "small_key",
    "gold",
    "chest_closed",
    "chest_open",
    "sealed_vault",
    "safe_exit",
    "forbidden_exit",
    "merchant",
    "healing_potion",
    "trap_disarm_kit",
    "clue_danger",
    "clue_opportunity",
    "clue_passage",
    "shrine_hp",
    "shrine_attack",
    "shrine_defense",
]

SFX_IDS = [
    "ability",
    "attack_player",
    "boss_phase",
    "chest_open",
    "defeat",
    "defend_player",
    "forbidden_exit",
    "merchant",
    "monster_hit",
    "pickup_gold",
    "pickup_key",
    "safe_exit",
    "shrine",
    "ui_reveal",
    "victory",
]

MUSIC_IDS = [
    "music_menu",
    "music_exploration",
    "music_combat",
    "music_boss",
    "music_final_boss",
    "music_victory",
    "music_defeat",
]


def content_ids(filename: str, key: str) -> list[str]:
    data = json.loads((CONTENT / filename).read_text(encoding="utf-8"))
    return [row["id"] for row in data[key]]


def slug(content_id: str) -> str:
    return content_id.split(".", 1)[1]


def seed(value: str) -> int:
    return int(hashlib.sha256(value.encode("utf-8")).hexdigest()[:8], 16)


def palette(value: str) -> tuple[tuple[int, int, int, int], tuple[int, int, int, int], tuple[int, int, int, int]]:
    h = (seed(value) % 360) / 360.0
    r, g, b = colorsys.hsv_to_rgb(h, 0.62, 0.78)
    r2, g2, b2 = colorsys.hsv_to_rgb((h + 0.11) % 1.0, 0.46, 0.93)
    r3, g3, b3 = colorsys.hsv_to_rgb((h + 0.55) % 1.0, 0.52, 0.36)
    return (
        (int(r * 255), int(g * 255), int(b * 255), 255),
        (int(r2 * 255), int(g2 * 255), int(b2 * 255), 255),
        (int(r3 * 255), int(g3 * 255), int(b3 * 255), 255),
    )


def blank(width: int, height: int, color: tuple[int, int, int, int] = (0, 0, 0, 0)) -> bytearray:
    return bytearray(color * (width * height))


def set_pixel(img: bytearray, width: int, height: int, x: int, y: int, color: tuple[int, int, int, int]) -> None:
    if 0 <= x < width and 0 <= y < height:
        idx = (y * width + x) * 4
        img[idx : idx + 4] = bytes(color)


def fill_rect(
    img: bytearray,
    width: int,
    height: int,
    x: int,
    y: int,
    w: int,
    h: int,
    color: tuple[int, int, int, int],
) -> None:
    x0, x1 = max(0, x), min(width, x + w)
    y0, y1 = max(0, y), min(height, y + h)
    row = bytes(color) * max(0, x1 - x0)
    for py in range(y0, y1):
        start = (py * width + x0) * 4
        img[start : start + len(row)] = row


def fill_circle(
    img: bytearray,
    width: int,
    height: int,
    cx: int,
    cy: int,
    rx: int,
    ry: int,
    color: tuple[int, int, int, int],
) -> None:
    for y in range(cy - ry, cy + ry + 1):
        for x in range(cx - rx, cx + rx + 1):
            if ((x - cx) * (x - cx) * ry * ry) + ((y - cy) * (y - cy) * rx * rx) <= rx * rx * ry * ry:
                set_pixel(img, width, height, x, y, color)


def write_png(path: Path, width: int, height: int, rgba: bytearray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)

    def chunk(kind: bytes, payload: bytes) -> bytes:
        crc = binascii.crc32(kind + payload) & 0xFFFFFFFF
        return struct.pack(">I", len(payload)) + kind + payload + struct.pack(">I", crc)

    rows = bytearray()
    stride = width * 4
    for y in range(height):
        rows.append(0)
        start = y * stride
        rows.extend(rgba[start : start + stride])
    header = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    path.write_bytes(
        b"\x89PNG\r\n\x1a\n"
        + chunk(b"IHDR", header)
        + chunk(b"IDAT", zlib.compress(bytes(rows), 9))
        + chunk(b"IEND", b"")
    )


def draw_sheet(path: Path, asset_id: str, width: int, height: int, frame: int) -> None:
    img = blank(width, height)
    primary, accent, shadow = palette(asset_id)
    columns = width // frame
    rows = height // frame
    s = seed(asset_id)
    for row in range(rows):
        for col in range(columns):
            ox, oy = col * frame, row * frame
            bob = ((s >> ((row + col) % 8)) & 3) - 1
            fill_circle(img, width, height, ox + frame // 2, oy + frame // 2 + bob, frame // 4, frame // 3, shadow)
            fill_circle(img, width, height, ox + frame // 2, oy + frame // 2 - 3 + bob, frame // 5, frame // 4, primary)
            fill_rect(img, width, height, ox + frame // 2 - frame // 8, oy + frame // 5, frame // 4, frame // 5, accent)
            fill_rect(img, width, height, ox + frame // 3, oy + frame * 2 // 3, frame // 10, frame // 5, primary)
            fill_rect(img, width, height, ox + frame * 3 // 5, oy + frame * 2 // 3, frame // 10, frame // 5, primary)
            set_pixel(img, width, height, ox + frame // 2 - 5, oy + frame // 2 - 8 + bob, (245, 250, 255, 255))
            set_pixel(img, width, height, ox + frame // 2 + 5, oy + frame // 2 - 8 + bob, (245, 250, 255, 255))
    write_png(path, width, height, img)


def draw_biome(path: Path, asset_id: str) -> None:
    width = height = 1024
    primary, accent, shadow = palette(asset_id)
    img = blank(width, height, (primary[0] // 4, primary[1] // 4, primary[2] // 4, 255))
    s = seed(asset_id)
    for y in range(height):
        for x in range(width):
            band = ((x * 7 + y * 5 + s) >> 5) & 31
            ridge = int(18 * math.sin((x + (s & 127)) / 37.0) + 14 * math.cos((y + (s & 63)) / 51.0))
            mix = max(0, min(255, band * 4 + ridge + 64))
            color = (
                (shadow[0] * (255 - mix) + primary[0] * mix) // 255,
                (shadow[1] * (255 - mix) + primary[1] * mix) // 255,
                (shadow[2] * (255 - mix) + primary[2] * mix) // 255,
                255,
            )
            set_pixel(img, width, height, x, y, color)
    for i in range(24):
        cx = (s * (i + 3) * 37) % width
        cy = (s * (i + 5) * 53) % height
        radius = 22 + ((s >> (i % 12)) & 31)
        fill_circle(img, width, height, cx, cy, radius, radius // 2 + 8, (accent[0], accent[1], accent[2], 150))
    write_png(path, width, height, img)


def draw_icon(path: Path, asset_id: str) -> None:
    width = height = 64
    primary, accent, shadow = palette(asset_id)
    img = blank(width, height)
    fill_circle(img, width, height, 32, 32, 24, 24, shadow)
    fill_circle(img, width, height, 32, 28, 16, 18, primary)
    fill_rect(img, width, height, 18, 42, 28, 8, accent)
    fill_rect(img, width, height, 28, 12, 8, 36, (245, 250, 255, 255))
    write_png(path, width, height, img)


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def unity_guid(path: Path) -> str:
    relative = path.relative_to(ROOT).as_posix()
    return hashlib.sha256(f"clickdungeon2:{relative}".encode("utf-8")).hexdigest()[:32]


def write_default_meta(path: Path, folder: bool = False) -> None:
    meta = path.with_name(path.name + ".meta")
    folder_line = "folderAsset: yes\n" if folder else ""
    meta.write_text(
        "fileFormatVersion: 2\n"
        f"guid: {unity_guid(path)}\n"
        f"{folder_line}"
        "DefaultImporter:\n"
        "  externalObjects: {}\n"
        "  userData:\n"
        "  assetBundleName:\n"
        "  assetBundleVariant:\n",
        encoding="utf-8",
    )


def write_texture_meta(path: Path) -> None:
    meta = path.with_name(path.name + ".meta")
    sprite_mode = 2 if "_core" in path.stem else 1
    pixels_per_unit = 128 if path.name.startswith("boss_") else 64
    meta.write_text(
        "fileFormatVersion: 2\n"
        f"guid: {unity_guid(path)}\n"
        "TextureImporter:\n"
        "  internalIDToNameTable: []\n"
        "  externalObjects: {}\n"
        "  serializedVersion: 13\n"
        "  mipmaps:\n"
        "    mipMapMode: 0\n"
        "    enableMipMap: 0\n"
        "    sRGBTexture: 1\n"
        "    linearTexture: 0\n"
        "    fadeOut: 0\n"
        "    borderMipMap: 0\n"
        "    mipMapsPreserveCoverage: 0\n"
        "    alphaTestReferenceValue: 0.5\n"
        "    mipMapFadeDistanceStart: 1\n"
        "    mipMapFadeDistanceEnd: 3\n"
        "  bumpmap:\n"
        "    convertToNormalMap: 0\n"
        "    externalNormalMap: 0\n"
        "    heightScale: 0.25\n"
        "    normalMapFilter: 0\n"
        "  isReadable: 0\n"
        "  streamingMipmaps: 0\n"
        "  ignoreMipmapLimit: 0\n"
        "  grayScaleToAlpha: 0\n"
        "  generateCubemap: 6\n"
        "  cubemapConvolution: 0\n"
        "  seamlessCubemap: 0\n"
        "  textureFormat: 1\n"
        "  maxTextureSize: 2048\n"
        "  textureSettings:\n"
        "    serializedVersion: 2\n"
        "    filterMode: 0\n"
        "    aniso: 1\n"
        "    mipBias: 0\n"
        "    wrapU: 1\n"
        "    wrapV: 1\n"
        "    wrapW: 1\n"
        "  nPOTScale: 0\n"
        "  lightmap: 0\n"
        "  compressionQuality: 50\n"
        "  spriteMode: "
        f"{sprite_mode}\n"
        "  spriteExtrude: 1\n"
        "  spriteMeshType: 1\n"
        "  alignment: 0\n"
        "  spritePivot: {x: 0.5, y: 0.5}\n"
        f"  spritePixelsToUnits: {pixels_per_unit}\n"
        "  spriteBorder: {x: 0, y: 0, z: 0, w: 0}\n"
        "  spriteGenerateFallbackPhysicsShape: 1\n"
        "  alphaUsage: 1\n"
        "  alphaIsTransparency: 1\n"
        "  spriteTessellationDetail: -1\n"
        "  textureType: 8\n"
        "  textureShape: 1\n"
        "  singleChannelComponent: 0\n"
        "  flipbookRows: 1\n"
        "  flipbookColumns: 1\n"
        "  maxTextureSizeSet: 0\n"
        "  compressionQualitySet: 0\n"
        "  textureFormatSet: 0\n"
        "  ignorePngGamma: 0\n"
        "  applyGammaDecoding: 0\n"
        "  cookieLightType: 0\n"
        "  platformSettings:\n"
        "  - serializedVersion: 3\n"
        "    buildTarget: DefaultTexturePlatform\n"
        "    maxTextureSize: 2048\n"
        "    resizeAlgorithm: 0\n"
        "    textureFormat: -1\n"
        "    textureCompression: 0\n"
        "    compressionQuality: 50\n"
        "    crunchedCompression: 0\n"
        "    allowsAlphaSplitting: 0\n"
        "    overridden: 0\n"
        "    ignorePlatformSupport: 0\n"
        "    androidETC2FallbackOverride: 0\n"
        "    forceMaximumCompressionQuality_BC6H_BC7: 0\n"
        "  spriteSheet:\n"
        "    serializedVersion: 2\n"
        "    sprites: []\n"
        "    outline: []\n"
        "    physicsShape: []\n"
        "    bones: []\n"
        "    spriteID:\n"
        "    internalID: 0\n"
        "    vertices: []\n"
        "    indices:\n"
        "    edges: []\n"
        "    weights: []\n"
        "    secondaryTextures: []\n"
        "  spritePackingTag:\n"
        "  pSDRemoveMatte: 0\n"
        "  pSDShowRemoveMatteOption: 0\n"
        "  userData:\n"
        "  assetBundleName:\n"
        "  assetBundleVariant:\n",
        encoding="utf-8",
    )


def write_audio_meta(path: Path) -> None:
    meta = path.with_name(path.name + ".meta")
    meta.write_text(
        "fileFormatVersion: 2\n"
        f"guid: {unity_guid(path)}\n"
        "AudioImporter:\n"
        "  externalObjects: {}\n"
        "  serializedVersion: 7\n"
        "  defaultSettings:\n"
        "    serializedVersion: 2\n"
        "    loadType: 0\n"
        "    sampleRateSetting: 0\n"
        "    sampleRateOverride: 44100\n"
        "    compressionFormat: 1\n"
        "    quality: 1\n"
        "    conversionMode: 0\n"
        "  platformSettingOverrides: {}\n"
        "  forceToMono: 0\n"
        "  normalize: 1\n"
        "  preloadAudioData: 1\n"
        "  loadInBackground: 0\n"
        "  ambisonic: 0\n"
        "  3D: 1\n"
        "  userData:\n"
        "  assetBundleName:\n"
        "  assetBundleVariant:\n",
        encoding="utf-8",
    )


def manifest_row(asset_id: str, path: Path, prompt_ref: str) -> dict[str, str]:
    return {
        "asset_id": asset_id,
        "filename": path.name,
        "sha256": sha256(path),
        "usage_terms": USAGE_TERMS,
        "prompt_ref": prompt_ref,
        "status": "production",
    }


def frequency(value: str) -> float:
    return 164.0 + float(seed(value) % 480)


def write_wav(path: Path, asset_id: str, seconds: float, amplitude: float) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    sample_rate = 48000
    samples = max(1, int(sample_rate * seconds))
    base = frequency(asset_id)
    wobble = 0.35 + ((seed(asset_id) >> 8) % 30) / 100.0
    with wave.open(str(path), "wb") as stream:
        stream.setnchannels(1)
        stream.setsampwidth(2)
        stream.setframerate(sample_rate)
        frames = bytearray()
        for i in range(samples):
            t = i / sample_rate
            attack = min(1.0, i / (sample_rate * 0.018))
            release = min(1.0, (samples - i) / (sample_rate * 0.08))
            envelope = max(0.0, min(attack, release))
            tone = math.sin(2.0 * math.pi * base * t)
            harmonic = 0.45 * math.sin(2.0 * math.pi * base * 1.5 * t + wobble)
            pulse = 0.18 * math.sin(2.0 * math.pi * 7.0 * t)
            value = (tone + harmonic + pulse) * amplitude * envelope
            frames.extend(struct.pack("<h", int(max(-1.0, min(1.0, value)) * 32767)))
        stream.writeframes(bytes(frames))


def remove_development_placeholders() -> None:
    for root in (ART_RUNTIME, AUDIO_RUNTIME):
        if not root.exists():
            continue
        for path in root.rglob("*"):
            if path.is_file() and "placeholder" in path.name.lower():
                path.unlink()


def generate_art() -> list[dict[str, str]]:
    monsters = [slug(value) for value in content_ids("monsters.json", "monsters")]
    heroes = [slug(value).replace("class.", "") for value in content_ids("classes.json", "classes")]
    bosses = [slug(value) for value in content_ids("bosses.json", "bosses")]
    biomes = [slug(value) for value in content_ids("biomes.json", "biomes")]
    traps = [slug(value) for value in content_ids("traps.json", "traps")]

    rows: list[dict[str, str]] = []
    for name in monsters:
        path = ART_RUNTIME / f"monster_{name}_core.png"
        draw_sheet(path, f"monster.{name}", 256, 192, 64)
        rows.append(manifest_row(f"art.monster.{name}.core", path, "procedural release sprite sheet: monster core"))
    for name in heroes:
        path = ART_RUNTIME / f"hero_{name}_core.png"
        draw_sheet(path, f"hero.{name}", 256, 256, 64)
        rows.append(manifest_row(f"art.hero.{name}.core", path, "procedural release sprite sheet: hero core"))
    for name in bosses:
        path = ART_RUNTIME / f"boss_{name}_core.png"
        draw_sheet(path, f"boss.{name}", 512, 384, 128)
        rows.append(manifest_row(f"art.boss.{name}.core", path, "procedural release sprite sheet: boss core"))
    for name in biomes:
        path = ART_RUNTIME / f"biome_{name}_master.png"
        draw_biome(path, f"biome.{name}")
        rows.append(manifest_row(f"art.biome.{name}.master", path, "procedural release biome master"))
    for name in traps:
        path = ART_RUNTIME / f"trap_{name}.png"
        draw_icon(path, f"trap.{name}")
        rows.append(manifest_row(f"art.trap.{name}", path, "procedural release icon: trap"))
    for name in ICON_IDS:
        path = ART_RUNTIME / f"{name}.png"
        draw_icon(path, name)
        rows.append(manifest_row(f"art.icon.{name}", path, "procedural release icon"))
    return rows


def generate_audio() -> list[dict[str, str]]:
    traps = [slug(value) for value in content_ids("traps.json", "traps")]
    biomes = [slug(value) for value in content_ids("biomes.json", "biomes")]

    rows: list[dict[str, str]] = []
    for name in SFX_IDS:
        path = AUDIO_RUNTIME / f"{name}.wav"
        write_wav(path, name, 0.32, 0.18)
        rows.append(manifest_row(f"audio.sfx.{name}", path, "procedural release sfx"))
    for name in traps:
        path = AUDIO_RUNTIME / f"trap_{name}.wav"
        write_wav(path, f"trap.{name}", 0.36, 0.2)
        rows.append(manifest_row(f"audio.trap.{name}", path, "procedural release trap sfx"))
    for name in biomes:
        path = AUDIO_RUNTIME / f"ambience_{name}.wav"
        write_wav(path, f"ambience.{name}", 1.25, 0.08)
        rows.append(manifest_row(f"audio.ambience.{name}", path, "procedural release ambience loop seed"))
    for name in MUSIC_IDS:
        path = AUDIO_RUNTIME / f"{name}.wav"
        write_wav(path, name, 1.8, 0.09)
        rows.append(manifest_row(f"audio.music.{name}", path, "procedural release music loop seed"))
    return rows


def write_manifest(path: Path, rows: list[dict[str, str]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    data = {
        "schema": "clickdungeon.asset_manifest.v1",
        "generated_by": "scripts/generate-release-assets.py",
        "usage_terms": USAGE_TERMS,
        "assets": sorted(rows, key=lambda row: row["asset_id"]),
    }
    path.write_text(json.dumps(data, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def write_unity_metadata() -> None:
    for directory in (ART_RUNTIME, ART_SOURCE, ART_RUNTIME.parent, AUDIO_RUNTIME, AUDIO_SOURCE, AUDIO_RUNTIME.parent):
        directory.mkdir(parents=True, exist_ok=True)
        write_default_meta(directory, folder=True)
    for path in sorted(ART_RUNTIME.glob("*.png")):
        write_texture_meta(path)
    for path in sorted(AUDIO_RUNTIME.glob("*.wav")):
        write_audio_meta(path)
    for path in (ART_SOURCE / "asset_manifest.json", AUDIO_SOURCE / "audio_manifest.json"):
        write_default_meta(path)


def main() -> None:
    remove_development_placeholders()
    art_rows = generate_art()
    audio_rows = generate_audio()
    write_manifest(ART_SOURCE / "asset_manifest.json", art_rows)
    write_manifest(AUDIO_SOURCE / "audio_manifest.json", audio_rows)
    write_unity_metadata()
    print(f"Generated release assets: {len(art_rows)} art files, {len(audio_rows)} audio files")


if __name__ == "__main__":
    main()
