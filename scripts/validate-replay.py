#!/usr/bin/env python3
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[1]
errors=[]

def require(path,*tokens):
    p=ROOT/path
    if not p.exists():
        errors.append(f'missing {path}')
        return ''
    text=p.read_text(encoding='utf-8')
    for token in tokens:
        if token not in text:errors.append(f'{path} missing contract: {token}')
    return text

envelope=require('Assets/ClickDungeon/Application/Replay/ReplayEnvelope.cs','RootSeed','HeroClassId','Mode','CampaignFloorLimit','UnlockedAbilityIds','Commands','FinalStateHash','GameVersionInfo.SimulationVersion','GameVersionInfo.ContentRevision')
codec=require('Assets/ClickDungeon/Application/Replay/ReplayCodec.cs','ValidateCompatibility','GZipStream','Convert.ToBase64String')
commands=require('Assets/ClickDungeon/Application/Replay/ReplayCommandCodec.cs','RevealTileCommand','MoveCommand','InteractCommand','AttackCommand','DefendCommand','UseAbilityCommand','UseItemCommand','ChooseShrineCommand','BuyItemCommand','EquipItemCommand','TakeSafeExitCommand','TakeForbiddenExitCommand','UnlockVaultCommand')
recorder=require('Assets/ClickDungeon/Application/Replay/ReplayRecorder.cs','ReplayCommandCodec.Encode','StateHasher.Hash')
runner=require('Assets/ClickDungeon/Application/Replay/ReplayRunner.cs','ReplayCommandCodec.Decode','GameSession','StateHasher.Hash','Replay diverged')
repository=require('Assets/ClickDungeon/Application/Replay/ReplayRepository.cs','last.replay','.bak','.tmp','PersistentDataSync.RequestSync')
ui=require('Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs','Action<GameCommand,CommandResult> CommandExecuted','CommandExecuted?.Invoke(command,result)')
bootstrap=require('Assets/ClickDungeon/Presentation/GameBootstrap.cs','new ReplayRecorder(Session.State)','new ReplayRepository()','ui.CommandExecuted+=OnCommandExecuted','_replayRepository.SaveLast')

# Active-run ability state must not be mutated by meta mastery outside the command stream.
unlock_start=bootstrap.find('private void UnlockMasteryAbilities')
if unlock_start>=0:
    unlock_end=bootstrap.find('private SlotMetaState',unlock_start)
    unlock_block=bootstrap[unlock_start:unlock_end if unlock_end>=0 else len(bootstrap)]
    if 'Session.State.AbilityStates.Add' in unlock_block:errors.append('GameBootstrap mastery unlock mutates active RunState outside replay command stream')
else:errors.append('GameBootstrap missing UnlockMasteryAbilities')

if errors:
    print('REPLAY CONTRACT VALIDATION FAILED')
    for e in errors:print(' -',e)
    sys.exit(1)
print('REPLAY CONTRACT VALIDATION PASSED')
