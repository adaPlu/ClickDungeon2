#!/usr/bin/env python3
from pathlib import Path
import json
import shutil
import subprocess
import sys
import tempfile
import zipfile

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

def test_gameplay_art_bindings_reject_unknown_item_id():
    temp=Path(tempfile.mkdtemp(prefix='cd2-art-bindings-'))
    try:
        source=temp/'Assets'/'ClickDungeon'/'Art'/'Source'
        content=temp/'Assets'/'ClickDungeon'/'Content'/'Json'
        source.mkdir(parents=True,exist_ok=True)
        content.mkdir(parents=True,exist_ok=True)
        (content/'items.json').write_text(json.dumps({'items':[{'id':'item.real'}]}),encoding='utf-8')
        (content/'monsters.json').write_text(json.dumps({'monsters':[{'id':'monster.real'}]}),encoding='utf-8')
        (content/'biomes.json').write_text(json.dumps({'biomes':[{'id':'biome.real'}]}),encoding='utf-8')
        (source/'gameplay_art_bindings.json').write_text(json.dumps({
            'schema':'clickdungeon.gameplay_art_bindings.v1',
            'game':'ClickDungeon',
            'items':[{'gameplay_id':'item.missing','status':'unresolved'}],
            'monsters':[],
            'heroes':[],
            'environment':[]
        }),encoding='utf-8')
        result=subprocess.run([sys.executable,str(ROOT/'scripts'/'validate-gameplay-art-bindings.py'),str(temp)],capture_output=True,text=True)
        output=result.stdout+result.stderr
        if result.returncode==0 or 'unknown item gameplay_id item.missing' not in output:
            print(output)
            raise AssertionError('gameplay art binding validator did not reject an unknown item ID')
    finally:
        shutil.rmtree(temp,ignore_errors=True)

def test_release_artifacts_require_all_platform_outputs():
    temp=Path(tempfile.mkdtemp(prefix='cd2-release-artifacts-'))
    try:
        (temp/'ClickDungeon2-Windows').mkdir()
        result=subprocess.run([sys.executable,str(ROOT/'scripts'/'verify-release-artifacts.py'),str(temp)],capture_output=True,text=True)
        output=result.stdout+result.stderr
        if result.returncode==0 or 'missing downloaded artifact directory: ClickDungeon2-Android-AAB' not in output:
            print(output)
            raise AssertionError('release artifact verification did not reject missing platform artifacts')
    finally:
        shutil.rmtree(temp,ignore_errors=True)

def test_android_artifact_requires_signing_metadata():
    temp=Path(tempfile.mkdtemp(prefix='cd2-android-artifact-'))
    try:
        bundle=temp/'ClickDungeon2.aab'
        with zipfile.ZipFile(bundle,'w') as archive:
            archive.writestr('base/manifest/AndroidManifest.xml',b'manifest')
            archive.writestr('base/dex/classes.dex',b'dex')
        result=subprocess.run([sys.executable,str(ROOT/'scripts'/'inspect-build-artifact.py'),'android',str(temp)],capture_output=True,text=True)
        output=result.stdout+result.stderr
        if result.returncode==0 or 'signing metadata' not in output:
            print(output)
            raise AssertionError('Android artifact inspection did not reject unsigned AAB metadata')
    finally:
        shutil.rmtree(temp,ignore_errors=True)

def test_android_apk_artifact_requires_manifest():
    temp=Path(tempfile.mkdtemp(prefix='cd2-android-apk-artifact-'))
    try:
        apk=temp/'ClickDungeon2.apk'
        with zipfile.ZipFile(apk,'w') as archive:
            archive.writestr('classes.dex',b'dex')
        result=subprocess.run([sys.executable,str(ROOT/'scripts'/'inspect-build-artifact.py'),'android-apk',str(temp)],capture_output=True,text=True)
        output=result.stdout+result.stderr
        if result.returncode==0 or 'AndroidManifest.xml' not in output:
            print(output)
            raise AssertionError('Android APK artifact inspection did not reject missing manifest')
    finally:
        shutil.rmtree(temp,ignore_errors=True)

def test_android_signer_verifier_rejects_unsigned_bundle():
    temp=Path(tempfile.mkdtemp(prefix='cd2-android-signer-'))
    try:
        bundle=temp/'ClickDungeon2.aab'
        with zipfile.ZipFile(bundle,'w') as archive:
            archive.writestr('base/manifest/AndroidManifest.xml',b'manifest')
            archive.writestr('base/dex/classes.dex',b'dex')
        result=subprocess.run([sys.executable,str(ROOT/'scripts'/'verify-android-signer.py'),str(bundle)],capture_output=True,text=True)
        output=result.stdout+result.stderr
        if result.returncode==0 or 'could not read signer certificate fingerprint' not in output:
            print(output)
            raise AssertionError('Android signer verifier did not reject unsigned AAB')
    finally:
        shutil.rmtree(temp,ignore_errors=True)

def main():
    test_nested_runtime_asset_requires_manifest_coverage()
    test_gameplay_art_bindings_reject_unknown_item_id()
    test_release_artifacts_require_all_platform_outputs()
    test_android_artifact_requires_signing_metadata()
    test_android_apk_artifact_requires_manifest()
    test_android_signer_verifier_rejects_unsigned_bundle()
    print('VALIDATOR TESTS PASSED')

if __name__=='__main__':
    main()
