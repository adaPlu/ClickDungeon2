#!/usr/bin/env python3
from pathlib import Path
import json, subprocess, sys
ROOT=Path(__file__).resolve().parents[1]
checks=[['python',str(ROOT/'scripts/validate-content.py')],['python',str(ROOT/'scripts/validate-assets.py')]]
for cmd in checks:
    if subprocess.call(cmd,cwd=ROOT)!=0: sys.exit(1)
errors=[]
for manifest in [ROOT/'Assets/ClickDungeon/Art/Source/asset_manifest.json',ROOT/'Assets/ClickDungeon/Audio/Source/audio_manifest.json']:
    data=json.loads(manifest.read_text())
    placeholders=[x['asset_id'] for x in data.get('assets',[]) if x.get('status')!='production']
    if placeholders: errors.append(f'{manifest.name}: {len(placeholders)} assets are not marked production-ready')
store_root=ROOT/'Store'
store_placeholders=[]
if store_root.exists():
    store_placeholders=[str(x.relative_to(ROOT)) for x in store_root.rglob('*') if x.is_file() and ('Placeholder' in x.parts or 'placeholder' in x.name.lower())]
    if store_placeholders: errors.append(f'Store assets: {len(store_placeholders)} placeholder files remain')
if errors:
    print('RELEASE CHECK BLOCKED (expected during development)')
    for e in errors: print(' -',e)
    sys.exit(2)
print('RELEASE CHECK PASSED')
