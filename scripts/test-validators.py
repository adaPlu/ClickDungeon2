#!/usr/bin/env python3
from pathlib import Path
import json
import shutil
import subprocess
import sys
import tempfile

ROOT=Path(__file__).resolve().parents[1]

def write_png(path:Path,width:int,height:int):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_bytes(
        b'\x89PNG\r\n\x1a\n'
        b'\x00\x00\x00\rIHDR'
        + width.to_bytes(4,'big')
        + height.to_bytes(4,'big')
        + b'\x08\x06\x00\x00\x00'
    )

def test_nested_runtime_asset_requires_manifest_coverage():
    temp=Path(tempfile.mkdtemp(prefix='cd2-validator-'))
    try:
        art_runtime=temp/'Assets'/'ClickDungeon'/'Art'/'Runtime'
        audio_runtime=temp/'Assets'/'ClickDungeon'/'Audio'/'Runtime'
        art_source=temp/'Assets'/'ClickDungeon'/'Art'/'Source'
        audio_source=temp/'Assets'/'ClickDungeon'/'Audio'/'Source'
        content=temp/'Assets'/'ClickDungeon'/'Content'/'Json'
        write_png(art_runtime/'nested'/'monster_rat_core.png',256,192)
        art_source.mkdir(parents=True,exist_ok=True)
        audio_source.mkdir(parents=True,exist_ok=True)
        audio_runtime.mkdir(parents=True,exist_ok=True)
        (art_source/'asset_manifest.json').write_text(json.dumps({'assets':[]}),encoding='utf-8')
        (audio_source/'audio_manifest.json').write_text(json.dumps({'assets':[]}),encoding='utf-8')
        content.mkdir(parents=True,exist_ok=True)
        (content/'traps.json').write_text(json.dumps({'traps':[]}),encoding='utf-8')
        (content/'biomes.json').write_text(json.dumps({'biomes':[]}),encoding='utf-8')
        result=subprocess.run([sys.executable,str(ROOT/'scripts'/'validate-assets.py'),str(temp)],capture_output=True,text=True)
        output=result.stdout+result.stderr
        if result.returncode==0 or 'runtime file is missing manifest coverage: nested/monster_rat_core.png' not in output:
            print(output)
            raise AssertionError('nested runtime media without manifest coverage was not rejected')
    finally:
        shutil.rmtree(temp,ignore_errors=True)

def main():
    test_nested_runtime_asset_requires_manifest_coverage()
    print('VALIDATOR TESTS PASSED')

if __name__=='__main__':
    main()
