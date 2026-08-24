#!/usr/bin/env python3
from pathlib import Path
import argparse, os, shutil, subprocess, sys

ROOT=Path(__file__).resolve().parents[1]
EXPECTED_VERSION='6000.5.9f1'

def find_unity():
    candidates=[]
    if os.environ.get('UNITY_PATH'): candidates.append(os.environ['UNITY_PATH'])
    for name in ('Unity','unity-editor','unity'):
        p=shutil.which(name)
        if p:candidates.append(p)
    candidates.extend([
        '/opt/unity/Editor/Unity',
        f'/Applications/Unity/Hub/Editor/{EXPECTED_VERSION}/Unity.app/Contents/MacOS/Unity',
    ])
    for value in candidates:
        p=Path(value)
        if p.exists() and os.access(p,os.X_OK): return str(p)
    return None

def run(cmd,label):
    print(label)
    rc=subprocess.call(cmd)
    if rc!=0:
        print(f'{label} FAILED: exit {rc}')
        sys.exit(rc)

def main():
    parser=argparse.ArgumentParser(description='Verify ClickDungeon2 with Unity batch mode.')
    parser.add_argument('--build',choices=('none','windows','android','web','ios'),default='none',help='Optional real player-build verification after EditMode tests.')
    args=parser.parse_args()
    unity=find_unity()
    if not unity:
        print(f'UNITY VERIFICATION UNAVAILABLE: no Unity editor executable found. Set UNITY_PATH to Unity {EXPECTED_VERSION} to run this verifier.')
        return 3
    logs=ROOT/'Builds'/'Verification';logs.mkdir(parents=True,exist_ok=True)
    base=[unity,'-batchmode','-nographics','-projectPath',str(ROOT)]
    run(base+['-quit','-executeMethod','ClickDungeon.EditorTools.BuildVerification.Verify','-logFile',str(logs/'compile.log')],'Running Unity compile/content verification...')
    run(base+['-runTests','-testPlatform','EditMode','-testResults',str(logs/'editmode-results.xml'),'-logFile',str(logs/'editmode.log'),'-quit'],'Running Unity EditMode tests...')
    if args.build!='none':
        method={
            'windows':'ClickDungeon.EditorTools.BuildAutomation.BuildWindows',
            'android':'ClickDungeon.EditorTools.BuildAutomation.BuildAndroid',
            'web':'ClickDungeon.EditorTools.BuildAutomation.BuildWeb',
            'ios':'ClickDungeon.EditorTools.BuildAutomation.ExportIos',
        }[args.build]
        run(base+['-quit','-executeMethod',method,'-logFile',str(logs/f'build-{args.build}.log')],f'Running Unity {args.build} player-build verification...')
    print('UNITY VERIFICATION PASSED'+(f' WITH {args.build.upper()} BUILD' if args.build!='none' else ''))
    return 0

if __name__=='__main__': sys.exit(main())
