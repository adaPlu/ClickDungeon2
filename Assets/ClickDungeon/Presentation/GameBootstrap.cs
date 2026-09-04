using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClickDungeon.Application.Content;
using ClickDungeon.Application.Persistence;
using ClickDungeon.Application.State;
using ClickDungeon.Application.Services;
using ClickDungeon.Application.Replay;
using ClickDungeon.Application.Heroes;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Progression;
using ClickDungeon.Presentation.UI;
using ClickDungeon.Presentation.Audio;

namespace ClickDungeon.Presentation
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private uint editorSeed = 12345;
        [SerializeField] private HeroClassId editorHeroClass = HeroClassId.Knight;
        [SerializeField] private bool unlockAllAbilitiesForDevelopment;
        private LocalSaveRepository _saves;
        private int _slot;
        private long _revision;
        private SlotMetaState _meta;
        private string _heroId;
        private AccountRepository _accounts;
        private AccountState _account;
        private bool _gameOverRecorded;
        private ServiceRegistry _services;
        private MusicAndAmbienceController _musicAndAmbience;
        private float _playStartedAt;
        private long _playSecondsBase;
        private ReplayRecorder _replayRecorder;
        private ReplayRepository _replayRepository;
        private const int FreeIntroLastFloor = 5;
        public GameSession Session { get; private set; }
        public GameContent Content { get; private set; }
        public string HeroId => _heroId;
        public string CampaignId => HeroIdentityCatalog.CampaignForHero(_heroId);

        private void Awake()
        {
            Content=LoadContent();_services=new ServiceRegistry();_services.Store.RefreshEntitlements();_saves=new LocalSaveRepository();_accounts=new AccountRepository();_account=_accounts.Load();_slot=Mathf.Clamp(PlayerPrefs.GetInt("cd2.slot",1),1,4);bool shouldContinue=PlayerPrefs.GetInt("cd2.continue",0)==1;bool startAbyss=PlayerPrefs.GetInt("cd2.abyss",0)==1;bool startedFresh=false;
            var generator=new FloorGenerator(Content);
            if(shouldContinue&&TryLoadExisting(out var loaded))
            {
                ContentMigrationService.Apply(loaded,Content);_meta=loaded.Meta??new SlotMetaState();Session=new GameSession(loaded.ActiveRun,generator,Content);_heroId=HeroIdentityCatalog.ResolveHeroId(Session.State.HeroClass,_meta.HeroId);_meta.HeroClassId=Session.State.HeroClass.ToString();_meta.HeroId=_heroId;ApplyEntitlementLimit(Session.State);
            }
            else
            {
                startedFresh=true;
                var cls=(HeroClassId)PlayerPrefs.GetInt("cd2.class",(int)editorHeroClass);uint seed=ParseSeed(PlayerPrefs.GetString("cd2.seed",editorSeed.ToString()),editorSeed);
                _meta=LoadMetaOrCreate(cls);string requestedHeroId=PlayerPrefs.GetString("cd2.hero",_meta.HeroId??string.Empty);_heroId=HeroIdentityCatalog.ResolveHeroId(cls,requestedHeroId);_meta.HeroClassId=cls.ToString();_meta.HeroId=_heroId;var unlocked=unlockAllAbilitiesForDevelopment?Content.Hero(cls).AbilityIds:UnlockedForMeta(cls,_meta);
                var state=startAbyss&&_meta.CampaignCompleted&&_services.Store.FullGameUnlocked?generator.CreateAbyssRun(seed,cls,unlocked):generator.CreateNewRun(seed,cls,unlocked);ApplyEntitlementLimit(state);Session=new GameSession(state,generator,Content);_account.TotalRuns++;_accounts.Save(_account);SaveCurrent();
            }
            _playStartedAt=Time.realtimeSinceStartup;_playSecondsBase=_meta.PlaySeconds;
            if(startedFresh){_replayRecorder=new ReplayRecorder(Session.State);_replayRepository=new ReplayRepository();}
            var audio=gameObject.AddComponent<GameEventAudioRouter>();audio.Initialize();_musicAndAmbience=gameObject.AddComponent<MusicAndAmbienceController>();_musicAndAmbience.Initialize(Content,Session.State);var ui=gameObject.AddComponent<RuntimeGameUI>();ui.Initialize(Session,Content,_heroId);ui.CommandExecuted+=OnCommandExecuted;ui.CommandResolved+=OnCommandResolved;ui.CommandResolved+=r=>audio.Present(r.Events);ui.StateChanged+=OnStateChanged;ui.ReturnToMenuRequested+=()=>SceneManager.LoadScene("Main");
            Debug.Log($"ClickDungeon slot={_slot} seed={Session.State.RootSeed} hero={_heroId} class={Session.State.HeroClass} floor={Session.State.Floor}");
        }

        private void ApplyEntitlementLimit(RunState state)
        {
            if(state==null)return;
            state.CampaignFloorLimit=_services.Store.FullGameUnlocked?Content.Balance.CampaignFloors:Math.Min(FreeIntroLastFloor,Content.Balance.CampaignFloors);
        }

        private void OnCommandExecuted(GameCommand command,CommandResult result)
        {
            if(_replayRecorder==null||_replayRepository==null)return;
            try
            {
                _replayRecorder.Record(command);
                _replayRepository.SaveLast(_replayRecorder.Finish(Session.State));
            }
            catch(Exception ex)
            {
                Debug.LogError($"Replay recording disabled after failure: {ex}");
                _replayRecorder=null;
            }
        }

        private void OnCommandResolved(CommandResult result)
        {
            int gained=0;
            foreach(var evt in result.Events)
            {
                if(evt.Type=="monster.defeated")gained+=Content.Progression.MonsterDefeatReward;
                else if(evt.Type=="boss.defeated")gained+=Content.Progression.BossRewardForFloor(Session.State.Floor);
                else if(evt.Type=="floor.entered.forbidden")gained+=Content.Progression.ForbiddenFloorMasteryBonus;
                else if(evt.Type=="campaign.completed"){gained+=Content.Progression.CampaignCompletionBonus;_account.TotalVictories++;}
                else if(evt.Type=="abyss.depth.entered"){gained+=Content.Progression.AbyssDepthReward;if(Content.Progression.AbyssMilestoneInterval>0&&evt.Amount%Content.Progression.AbyssMilestoneInterval==0)gained+=Content.Progression.AbyssDepthReward;}
                foreach(var achievementId in AchievementEvaluator.Evaluate(Content,Session.State,evt))if(!_account.AchievementIds.Contains(achievementId))_account.AchievementIds.Add(achievementId);
            }
            if(gained>0){_meta.ClassMastery+=gained;UnlockMasteryAbilities();}
            if(Session.State.GameOver&&!_gameOverRecorded){_gameOverRecorded=true;_meta.Deaths++;_account.TotalDeaths++;}
            _accounts.Save(_account);
        }

        private void UnlockMasteryAbilities()
        {
            foreach(var def in Content.AbilitiesForClass(Session.State.HeroClass))
            {
                if(_meta.ClassMastery<def.UnlockMastery||_meta.UnlockedAbilityIds.Contains(def.Id))continue;
                _meta.UnlockedAbilityIds.Add(def.Id);
            }
        }

        private SlotMetaState LoadMetaOrCreate(HeroClassId cls)
        {
            try{var doc=_saves.LoadSlot(_slot);if(doc?.payload?.Meta!=null)return doc.payload.Meta;}catch(Exception ex){Debug.LogWarning($"Existing slot metadata could not be reused: {ex.Message}");}
            string now=DateTimeOffset.UtcNow.ToString("O");return new SlotMetaState{HeroClassId=cls.ToString(),HeroId=HeroIdentityCatalog.StandardHeroId(cls),CreatedAt=now,LastPlayedAt=now};
        }

        private string[] UnlockedForMeta(HeroClassId cls,SlotMetaState meta)
        {
            var unlocked=Content.AbilitiesForClass(cls).Where(a=>a.UnlockMastery<=meta.ClassMastery).Select(a=>a.Id).ToList();
            if(unlocked.Count==0)unlocked.Add(Content.Hero(cls).AbilityIds[0]);
            foreach(var id in unlocked)if(!meta.UnlockedAbilityIds.Contains(id))meta.UnlockedAbilityIds.Add(id);
            return unlocked.ToArray();
        }

        private bool TryLoadExisting(out SlotSavePayload payload)
        {
            payload=null;
            try
            {
                var doc=_saves.LoadSlot(_slot);
                if(doc?.payload?.ActiveRun==null)return false;
                _revision=doc.revision_number;payload=doc.payload;return true;
            }
            catch(Exception ex)
            {
                if(_saves.SlotExists(_slot))throw new InvalidDataException($"Slot {_slot} exists but no valid save copy could be loaded.",ex);
                Debug.LogError($"Slot {_slot} could not be loaded: {ex}");
                return false;
            }
        }
        private void OnStateChanged(){UpdateMeta();SaveCurrent();_musicAndAmbience?.Refresh(Session.State);}
        private void UpdateMeta(){var s=Session.State;_meta.HeroClassId=s.HeroClass.ToString();_heroId=HeroIdentityCatalog.ResolveHeroId(s.HeroClass,_heroId??_meta.HeroId);_meta.HeroId=_heroId;_meta.BestFloor=Math.Max(_meta.BestFloor,s.Mode==RunMode.Campaign?s.Floor:_meta.BestFloor);_meta.BestAbyssDepth=Math.Max(_meta.BestAbyssDepth,s.AbyssDepth);_meta.CampaignCompleted|=s.CampaignCompleted;_meta.LastPlayedAt=DateTimeOffset.UtcNow.ToString("O");_meta.PlaySeconds=_playSecondsBase+Math.Max(0,(long)(Time.realtimeSinceStartup-_playStartedAt));foreach(var a in s.AbilityStates)if(!_meta.UnlockedAbilityIds.Contains(a.AbilityId))_meta.UnlockedAbilityIds.Add(a.AbilityId);}
        private void SaveCurrent(){UpdateMeta();_revision++;_saves.SaveSlot(_slot,new SlotSavePayload{Meta=_meta,ActiveRun=Session.State},_revision);}
        private static SlotMetaState NewMeta(RunState state){string now=DateTimeOffset.UtcNow.ToString("O");return new SlotMetaState{HeroClassId=state.HeroClass.ToString(),HeroId=HeroIdentityCatalog.StandardHeroId(state.HeroClass),BestFloor=state.Floor,CreatedAt=now,LastPlayedAt=now,UnlockedAbilityIds=state.AbilityStates.Select(a=>a.AbilityId).ToList()};}
        private static uint ParseSeed(string text,uint fallback)=>uint.TryParse(text,out var seed)?seed:fallback;

        private static GameContent LoadContent()
        {
            var generated=Resources.Load<GeneratedContentDatabase>("ClickDungeonGeneratedContent");
            if(generated!=null){try{return generated.CreateCatalog();}catch(Exception ex){Debug.LogError($"Generated content database failed validation: {ex}");}}
#if UNITY_EDITOR
            try{string path=Path.Combine(Application.dataPath,"ClickDungeon","Content","Json");if(Directory.Exists(path))return new JsonContentCatalogLoader().LoadFromDirectory(path);}catch(Exception ex){Debug.LogError($"Canonical content load failed. Development fallback will be used. {ex}");}
#endif
            Debug.LogError("Generated production content was not found. Falling back to development definitions; release validation must reject this state.");return GameContent.CreateDevelopmentFallback();
        }
    }
}
