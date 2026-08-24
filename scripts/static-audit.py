#!/usr/bin/env python3
from pathlib import Path
import json,re,sys
ROOT=Path(__file__).resolve().parents[1]
errors=[];warnings=[]

def brace_balance(text):
    depth=0;i=0;state='code'
    while i<len(text):
        c=text[i];n=text[i+1] if i+1<len(text) else ''
        if state=='code':
            if c=='/' and n=='/':state='line';i+=2;continue
            if c=='/' and n=='*':state='block';i+=2;continue
            if c=='@' and n=='"':state='verbatim';i+=2;continue
            if c=='"':state='string';i+=1;continue
            if c=="'":state='char';i+=1;continue
            if c=='{':depth+=1
            elif c=='}':
                depth-=1
                if depth<0:return depth
        elif state=='line':
            if c=='\n':state='code'
        elif state=='block':
            if c=='*' and n=='/':state='code';i+=2;continue
        elif state=='string':
            if c=='\\':i+=2;continue
            if c=='"':state='code'
        elif state=='verbatim':
            if c=='"' and n=='"':i+=2;continue
            if c=='"':state='code'
        elif state=='char':
            if c=='\\':i+=2;continue
            if c=="'":state='code'
        i+=1
    return depth

for p in sorted((ROOT/'Assets/ClickDungeon/Content/Json').glob('*.json')):
    try:json.loads(p.read_text())
    except Exception as e:errors.append(f'{p.relative_to(ROOT)} invalid JSON: {e}')

sim=ROOT/'Assets/ClickDungeon/Simulation'
for p in sim.rglob('*.cs'):
    text=p.read_text()
    for pat,label in [(r'\bUnityEngine\b','UnityEngine dependency'),(r'\bSystem\.Random\b','System.Random'),(r'\bUnityEngine\.Random\b','UnityEngine.Random'),(r'\bTime\.(deltaTime|time|realtimeSinceStartup)','frame/wall clock')]:
        if re.search(pat,text):errors.append(f'{p.relative_to(ROOT)}: forbidden simulation {label}')

for p in (ROOT/'Assets/ClickDungeon').rglob('*.cs'):
    text=p.read_text()
    if any(m in text for m in ('<<<<<<<','>>>>>>>')):errors.append(f'{p.relative_to(ROOT)}: unresolved merge marker')
    d=brace_balance(text)
    if d!=0:errors.append(f'{p.relative_to(ROOT)}: brace imbalance {d}')
    if re.search(r'\bTODO\b|\bFIXME\b|NotImplementedException',text):warnings.append(f'{p.relative_to(ROOT)} contains TODO/FIXME/not-implemented marker')

run=(sim/'Model/RunState.cs').read_text();session=(sim/'GameSession.cs').read_text()
for token in ['CampaignFloorLimit','ItemInstances','AbilityStates','AbyssDepth']:
    if token not in run:errors.append(f'RunState missing {token}')
for token in ['entitlement.full_game_required','LivingMonstersAdjacentTo','DefeatMonster(chained','tile.not_adjacent']:
    if token not in session:errors.append(f'GameSession integration contract missing {token}')

build_path=ROOT/'Assets/ClickDungeon/Editor/BuildAutomation.cs';build=build_path.read_text()
for token in ['TextMeshProResourceBootstrap.Ensure','ContentAssetGenerator.Generate','PresentationAssetGenerator.Generate','SceneScaffolder.EnsureCoreScenes','AssetDatabase.SaveAssets','ImportAssetOptions.ForceUpdate']:
    if token not in build:errors.append(f'BuildAutomation missing {token}')
tmp_bootstrap=ROOT/'Assets/ClickDungeon/Editor/TextMeshProResourceBootstrap.cs'
if not tmp_bootstrap.exists():errors.append('TextMeshProResourceBootstrap.cs is missing')
else:
    tmp=tmp_bootstrap.read_text()
    for token in ['TMP Settings.asset','TMPro.TMP_PackageUtilities','ImportProjectResourcesMenu']:
        if token not in tmp:errors.append(f'TMP resource bootstrap missing {token}')

manifest_path=ROOT/'Packages/manifest.json'
try:
    packages=json.loads(manifest_path.read_text()).get('dependencies',{})
    expected={
        'com.unity.nuget.newtonsoft-json':'3.2.2',
        'com.unity.render-pipelines.universal':'17.5.0',
        'com.unity.test-framework':'1.7.0',
        'com.unity.ugui':'2.0.0'
    }
    for package,version in expected.items():
        if packages.get(package)!=version:errors.append(f'Packages/manifest.json expected {package} {version}, found {packages.get(package)}')
except Exception as e:errors.append(f'Packages/manifest.json could not be validated: {e}')

release_path=ROOT/'scripts/release-check.py';release=release_path.read_text()
if "store_root=ROOT/'Store'" not in release:errors.append('release-check.py does not gate store placeholders')
if 'validate-assets.py' not in release:errors.append('release-check.py does not run validate-assets.py')
if not (ROOT/'scripts/validate-assets.py').exists():errors.append('scripts/validate-assets.py is missing')

pres=(ROOT/'Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs').read_text()
for token in ['Abyss Depth','ShowInventory','ClickDungeonPresentationAssets','RefreshIntent']:
    if token not in pres:errors.append(f'Runtime presentation contract missing {token}')

# In ClickDungeon.Application.* namespaces, Unity's Application type must be fully qualified to avoid namespace shadowing.
for p in (ROOT/'Assets/ClickDungeon/Application').rglob('*.cs'):
    text=p.read_text()
    for m in re.finditer(r'(?<!UnityEngine\.)\bApplication\.(persistentDataPath|platform|isEditor|isMobilePlatform)',text):errors.append(f'{p.relative_to(ROOT)}: unqualified Unity Application API may resolve to ClickDungeon.Application namespace')

print(f'STATIC AUDIT: {len(errors)} errors, {len(warnings)} warnings')
for e in errors:print('ERROR:',e)
for w in warnings:print('WARN:',w)
sys.exit(1 if errors else 0)
