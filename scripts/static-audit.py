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
for token in ['BuildPlayerSettings.Apply','TextMeshProResourceBootstrap.Ensure','ContentAssetGenerator.Generate','PresentationAssetGenerator.Generate','SceneScaffolder.EnsureCoreScenes','AssetDatabase.SaveAssets','ImportAssetOptions.ForceUpdate']:
    if token not in build:errors.append(f'BuildAutomation missing {token}')

player_settings=ROOT/'Assets/ClickDungeon/Editor/BuildPlayerSettings.cs'
if not player_settings.exists():errors.append('BuildPlayerSettings.cs is missing')
else:
    settings=player_settings.read_text()
    for token in ['GameVersionInfo.GameVersion','com.adaplu.clickdungeon','AndroidApiLevel23','AndroidApiLevel36','AndroidArchitecture.ARMv7|AndroidArchitecture.ARM64','targetOSVersionString="13.0"','UIOrientation.Portrait']:
        if token not in settings:errors.append(f'BuildPlayerSettings missing {token}')

version_info=ROOT/'Assets/ClickDungeon/Application/Versioning/GameVersionInfo.cs'
if not version_info.exists():errors.append('GameVersionInfo.cs is missing')
else:
    version_text=version_info.read_text()
    for token in ['GameVersion="0.2.0"','SaveSchemaVersion=2','SimulationVersion=2','ContentRevision=2']:
        if token not in version_text:errors.append(f'GameVersionInfo missing {token}')

save_doc=(ROOT/'Assets/ClickDungeon/Application/Persistence/SaveDocument.cs').read_text()
for token in ['GameVersionInfo.SaveSchemaVersion','GameVersionInfo.GameVersion','GameVersionInfo.SimulationVersion','GameVersionInfo.ContentRevision']:
    if token not in save_doc:errors.append(f'SaveDocument version contract missing {token}')
replay=(ROOT/'Assets/ClickDungeon/Application/Replay/ReplayEnvelope.cs').read_text()
for token in ['GameVersionInfo.SimulationVersion','GameVersionInfo.ContentRevision']:
    if token not in replay:errors.append(f'ReplayEnvelope version contract missing {token}')
if (ROOT/'Assets/ClickDungeon/Simulation/Replay/ReplayRecord.cs').exists():errors.append('Legacy duplicate ReplayRecord.cs still exists')

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

# WebGL persistence bridge must stay symbol-compatible across C# and JavaScript.
sync_cs=ROOT/'Assets/ClickDungeon/Application/Platform/PersistentDataSync.cs'
sync_js=ROOT/'Assets/Plugins/WebGL/ClickDungeonPersistence.jslib'
if not sync_cs.exists():errors.append('PersistentDataSync.cs is missing')
if not sync_js.exists():errors.append('ClickDungeonPersistence.jslib is missing')
if sync_cs.exists():
    sync_text=sync_cs.read_text()
    if 'ClickDungeonSyncPersistentData' not in sync_text:errors.append('PersistentDataSync C# extern name changed')
    if 'ClickDungeonGetPersistentDataSyncStatus' not in sync_text or 'PollStatus' not in sync_text:errors.append('PersistentDataSync no longer exposes WebGL sync status polling')
if sync_js.exists():
    js=sync_js.read_text()
    if 'ClickDungeonSyncPersistentData' not in js:errors.append('WebGL persistence export name changed')
    if 'ClickDungeonGetPersistentDataSyncStatus' not in js:errors.append('WebGL persistence status export name changed')
    for token in ['ClickDungeonSyncPersistentData__deps', 'ClickDungeonGetPersistentDataSyncStatus__deps', '$ClickDungeonPersistentDataSyncState']:
        if token not in js:errors.append(f'WebGL persistence bridge missing Emscripten dependency token {token}')
    for token in ['status = 1','status = 2','status = 3']:
        if token not in js:errors.append(f'WebGL persistence bridge missing sync state transition {token}')
    if 'FS.syncfs(false' not in js:errors.append('WebGL persistence bridge no longer flushes to IndexedDB')

# Generated Resources names are runtime contracts.
content_gen=(ROOT/'Assets/ClickDungeon/Editor/ContentAssetGenerator.cs').read_text()
presentation_gen=(ROOT/'Assets/ClickDungeon/Editor/PresentationAssetGenerator.cs').read_text()
pixel_importer=(ROOT/'Assets/ClickDungeon/Editor/PixelAssetImporter.cs').read_text()
animation_gen=(ROOT/'Assets/ClickDungeon/Editor/AnimationClipGenerator.cs').read_text()
game_boot=(ROOT/'Assets/ClickDungeon/Presentation/GameBootstrap.cs').read_text()
menu=(ROOT/'Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs').read_text()
runtime_ui=(ROOT/'Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs').read_text()
for token in ['ClickDungeonGeneratedContent.asset']:
    if token not in content_gen:errors.append(f'Content generator missing resource {token}')
for token in ['ClickDungeonPresentationAssets.asset']:
    if token not in presentation_gen:errors.append(f'Presentation generator missing resource {token}')
for token in ['Resources.Load<GeneratedContentDatabase>("ClickDungeonGeneratedContent")']:
    if token not in game_boot or token not in menu:errors.append('Runtime generated-content Resources name does not match generator')
for token in ['Resources.Load<PresentationAssetDatabase>("ClickDungeonPresentationAssets")']:
    if token not in runtime_ui:errors.append('Runtime presentation Resources name does not match generator')
for name,text in [('PixelAssetImporter.cs',pixel_importer),('AnimationClipGenerator.cs',animation_gen)]:
    if 'EndsWith("_core",StringComparison.Ordinal)' not in text:
        errors.append(f'{name} does not recognize production *_core sprite sheet names')
    if 'Contains("_core_",StringComparison.Ordinal)' not in text:
        errors.append(f'{name} does not preserve compatibility with *_core_* development sheet names')

release_path=ROOT/'scripts/release-check.py';release=release_path.read_text()
store_gate=re.search(r"store_root\s*=\s*ROOT\s*/\s*['\"]Store['\"]",release)
if not store_gate or 'placeholder' not in release.lower():errors.append('release-check.py does not gate store placeholders')
if 'validate-assets.py' not in release:errors.append('release-check.py does not run validate-assets.py')
if 'validate-replay.py' not in release:errors.append('release-check.py does not run validate-replay.py')
if 'static-audit.py' not in release:errors.append('release-check.py does not run static-audit.py')
if 'verify-release-artifacts.py' not in release:errors.append('release-check.py does not require release artifact verification')
if 'validate-unity-metadata.py' not in release or '--strict' not in release:errors.append('release-check.py does not strictly gate Unity metadata reproducibility')
if 'Packages' not in release or 'packages-lock.json' not in release:errors.append('release-check.py does not require Unity package lock')
if not (ROOT/'scripts/validate-assets.py').exists():errors.append('scripts/validate-assets.py is missing')
if not (ROOT/'scripts/test-validators.py').exists():errors.append('scripts/test-validators.py is missing')
if not (ROOT/'scripts/verify-release-artifacts.py').exists():errors.append('scripts/verify-release-artifacts.py is missing')
if not (ROOT/'Packages/packages-lock.json').is_file():errors.append('Packages/packages-lock.json is missing')

artifact_inspector=ROOT/'scripts/inspect-build-artifact.py'
if artifact_inspector.exists():
    artifact_text=artifact_inspector.read_text()
    if 'signing metadata' not in artifact_text or 'META-INF/' not in artifact_text:
        errors.append('inspect-build-artifact.py does not verify Android signing metadata')
else:errors.append('scripts/inspect-build-artifact.py is missing')

release_workflow=ROOT/'.github/workflows/release-gate.yml'
if release_workflow.exists():
    release_workflow_text=release_workflow.read_text()
    release_gate_contracts={
        'manual Unity run input':'unity_run_id',
        'repository-scoped Actions API lookup':'gh api',
        'Actions run lookup path':'actions/runs/${UNITY_RUN_ID}',
        'run head SHA verification':'head_sha',
        'run status verification':'status',
        'run conclusion verification':'conclusion',
        'workflow name binding':'workflow_name',
        'workflow path binding':'workflow_path',
        'required Unity workflow name':'Unity Platform CI',
        'required Unity workflow path':'.github/workflows/unity-platform-ci.yml',
        'artifact download':'gh run download',
        'artifact verification':'verify-release-artifacts.py'
    }
    for contract,token in release_gate_contracts.items():
        if token not in release_workflow_text:errors.append(f'release-gate.yml missing {contract} token {token}')
else:errors.append('.github/workflows/release-gate.yml is missing')

workflow_dir=ROOT/'.github/workflows'
sha_ref=re.compile(r'^[0-9a-f]{40}$')
for p in sorted(workflow_dir.glob('*.yml')):
    text=p.read_text()
    for match in re.finditer(r'uses:\s*["\']?([^"\'\s#]+)',text):
        action=match.group(1)
        if action.startswith('./') or action.startswith('docker://'):continue
        if '@' not in action:
            errors.append(f'{p.relative_to(ROOT)} action is missing immutable ref: {action}')
            continue
        name,ref=action.rsplit('@',1)
        if not sha_ref.fullmatch(ref):errors.append(f'{p.relative_to(ROOT)} action {name} is not pinned to a full commit SHA')
    if re.search(r'^\s*pull_request\s*:',text,re.MULTILINE) and re.search(r'UNITY_(LICENSE|SERIAL|EMAIL|PASSWORD)\s*:\s*\$\{\{\s*secrets\.',text):
        errors.append(f'{p.relative_to(ROOT)} exposes Unity secrets to pull_request jobs')

unity_ci=workflow_dir/'unity-platform-ci.yml'
if unity_ci.exists() and 'allowDirtyBuild: true' in unity_ci.read_text():
    errors.append('unity-platform-ci.yml permits dirty Unity builds')
if unity_ci.exists():
    unity_ci_text=unity_ci.read_text()
    if 'checks: write' not in unity_ci_text:
        errors.append('unity-platform-ci.yml lacks checks: write permission for Unity test result publishing')
    if 'test-artifacts/unity-import' in unity_ci_text:
        errors.append('unity-platform-ci.yml writes metadata capture into root-owned test-artifacts')

for token in ['Abyss Depth','ShowInventory','ClickDungeonPresentationAssets','RefreshIntent']:
    if token not in runtime_ui:errors.append(f'Runtime presentation contract missing {token}')

for p in (ROOT/'Assets/ClickDungeon/Application').rglob('*.cs'):
    text=p.read_text()
    for m in re.finditer(r'(?<!UnityEngine\.)\bApplication\.(persistentDataPath|platform|isEditor|isMobilePlatform)',text):errors.append(f'{p.relative_to(ROOT)}: unqualified Unity Application API may resolve to ClickDungeon.Application namespace')

print(f'STATIC AUDIT: {len(errors)} errors, {len(warnings)} warnings')
for e in errors:print('ERROR:',e)
for w in warnings:print('WARN:',w)
sys.exit(1 if errors else 0)
