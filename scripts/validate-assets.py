#!/usr/bin/env python3
from pathlib import Path
import hashlib, json, struct, wave, sys
ROOT=Path(__file__).resolve().parents[1]
errors=[]
art=ROOT/'Assets'/'ClickDungeon'/'Art'; audio=ROOT/'Assets'/'ClickDungeon'/'Audio'

def png_size(path):
    try:
        with path.open('rb') as f:
            sig=f.read(24)
        if sig[:8]!=b'\x89PNG\r\n\x1a\n': return None
        return struct.unpack('>II',sig[16:24])
    except Exception: return None

def sha256(path):
    h=hashlib.sha256()
    with path.open('rb') as f:
        for chunk in iter(lambda:f.read(1024*1024),b''): h.update(chunk)
    return h.hexdigest()

def validate_manifest(path,runtime):
    if not path.exists(): errors.append(f'missing manifest {path.relative_to(ROOT)}'); return []
    try: data=json.loads(path.read_text())
    except Exception as e: errors.append(f'invalid manifest {path}: {e}'); return []
    rows=data.get('assets',[]); ids=set(); files=set()
    for row in rows:
        aid=row.get('asset_id'); filename=row.get('filename')
        if not aid or aid in ids: errors.append(f'{path.name}: missing/duplicate asset_id {aid}')
        ids.add(aid)
        if not filename: errors.append(f'{aid}: missing filename'); continue
        files.add(filename); f=runtime/filename
        if not f.exists(): errors.append(f'{aid}: runtime file missing: {filename}'); continue
        expected=row.get('sha256','')
        if len(expected)!=64: errors.append(f'{aid}: missing SHA-256 provenance')
        elif sha256(f)!=expected: errors.append(f'{aid}: SHA-256 mismatch')
        if not row.get('usage_terms'): errors.append(f'{aid}: usage_terms missing')
        if not row.get('prompt_ref'): errors.append(f'{aid}: prompt_ref missing')
    return rows

artrows=validate_manifest(art/'Source'/'asset_manifest.json',art/'Runtime')
audiorows=validate_manifest(audio/'Source'/'audio_manifest.json',audio/'Runtime')

monster_files=sorted(p for p in (art/'Runtime').glob('monster_*_core*.png') if '_core' in p.stem)
if len(monster_files)!=23: errors.append(f'expected 23 monster core sheets, found {len(monster_files)}')
for p in monster_files:
    if png_size(p)!=(256,192): errors.append(f'{p.name}: monster core must be 256x192 (12 frames at 64x64)')
hero_files=sorted(p for p in (art/'Runtime').glob('hero_*_core*.png') if '_core' in p.stem)
if len(hero_files)!=4: errors.append(f'expected 4 hero core sheets, found {len(hero_files)}')
for p in hero_files:
    if png_size(p)!=(256,256): errors.append(f'{p.name}: hero core must be 256x256 (4x4 at 64x64)')
boss_files=sorted(p for p in (art/'Runtime').glob('boss_*_core*.png') if '_core' in p.stem)
if len(boss_files)!=5: errors.append(f'expected 5 boss core sheets, found {len(boss_files)}')
for p in boss_files:
    if png_size(p)!=(512,384): errors.append(f'{p.name}: boss core must be 512x384 (4x3 at 128x128)')
biome_files=sorted(p for p in (art/'Runtime').glob('biome_*_master*.png') if '_master' in p.stem)
if len(biome_files)!=10: errors.append(f'expected 10 biome masters, found {len(biome_files)}')
for p in biome_files:
    size=png_size(p)
    if size is None or size[0]!=size[1] or size[0]<1024: errors.append(f'{p.name}: biome master must be square and >=1024px')

wav_files=list((audio/'Runtime').glob('*.wav'))
if len(wav_files)<30: errors.append(f'expected broad audio coverage, found {len(wav_files)} WAVs')
for wav in wav_files:
    try:
        with wave.open(str(wav),'rb') as f:
            if f.getframerate()!=48000: errors.append(f'{wav.name}: expected 48000 Hz')
            if f.getnchannels() not in (1,2): errors.append(f'{wav.name}: unsupported channel count {f.getnchannels()}')
    except Exception as e: errors.append(f'{wav.name}: invalid WAV: {e}')

# Canonical content must have presentation audio coverage, not merely a large WAV count.
try:
    traps=json.loads((ROOT/'Assets'/'ClickDungeon'/'Content'/'Json'/'traps.json').read_text()).get('traps',[])
    for trap in traps:
        suffix=trap['id'].split('.',1)[1]
        if not any((audio/'Runtime').glob(f'trap_{suffix}*.wav')): errors.append(f"{trap['id']}: missing trap SFX")
    biomes=json.loads((ROOT/'Assets'/'ClickDungeon'/'Content'/'Json'/'biomes.json').read_text()).get('biomes',[])
    for biome in biomes:
        suffix=biome['id'].replace('biome.','')
        if not any((audio/'Runtime').glob(f'ambience_{suffix}*.wav')): errors.append(f"{biome['id']}: missing biome ambience")
    for music in ('menu','exploration','combat','boss','final_boss','victory','defeat'):
        if not any((audio/'Runtime').glob(f'music_{music}*.wav')): errors.append(f'music.{music}: missing music asset')
except Exception as e: errors.append(f'canonical audio coverage validation failed: {e}')

if errors:
    print('ASSET VALIDATION FAILED'); [print(' -',e) for e in errors]; sys.exit(1)
print(f'ASSET VALIDATION PASSED: {len(monster_files)} monsters, {len(hero_files)} heroes, {len(boss_files)} bosses, {len(biome_files)} biomes, {len(wav_files)} audio files')
