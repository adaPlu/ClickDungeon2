using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ClickDungeon.Application.Persistence;
using ClickDungeon.Application.Content;
using ClickDungeon.Application.State;
using ClickDungeon.Application.Heroes;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Application.Services;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Menu
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        private const string TitleText="ClickDungeon";
        private static readonly Color BackdropColor=new Color(.012f,.022f,.032f,1f);
        private static readonly Color PanelColor=new Color(.022f,.043f,.064f,.96f);
        private static readonly Color PanelBlue=new Color(.025f,.075f,.13f,.97f);
        private static readonly Color Gold=new Color(.95f,.62f,.16f,1f);
        private static readonly Color GoldSoft=new Color(1f,.84f,.48f,1f);
        private static readonly Color Cream=new Color(.96f,.91f,.78f,1f);
        private static readonly Color Muted=new Color(.72f,.75f,.80f,1f);
        private static readonly Color PlayGreen=new Color(.10f,.43f,.12f,.98f);
        private static readonly Color QuitRed=new Color(.40f,.06f,.055f,.98f);

        private LocalSaveRepository _saves;
        private RectTransform _root;
        private RectTransform _viewport;
        private Rect _lastSafeArea;
        private TMP_Text _status;
        private int _selectedSlot=1;
        private ServiceRegistry _services;
        private AccountRepository _accounts;
        private AccountState _account;
        private GameContent _content;
        private PresentationAssetDatabase _assets;

        private TMP_Text _selectedHeroName;
        private TMP_Text _selectedHeroMeta;
        private Image _selectedHeroPortrait;
        private Image _heroShowcase;
        private TMP_Text _continueFloor;
        private TMP_Text _continueDetail;
        private TMP_Text _continueActionLabel;
        private readonly Image[] _slotButtonBackgrounds=new Image[4];
        private GameObject _heroSelectOverlay;
        private RectTransform _heroSelectContent;
        private RectTransform _heroSelectCardHost;
        private TMP_Text _heroSelectClassLabel;
        private TMP_Text _heroSelectHeroLabel;
        private int _heroSelectClassIndex;
        private int _heroSelectHeroIndex;
        private GameObject _utilityDrawer;

        private void Start()
        {
            _saves=new LocalSaveRepository();
            _accounts=new AccountRepository();
            _account=_accounts.Load();
            _services=new ServiceRegistry();
            _services.Store.RefreshEntitlements();
            _content=LoadContent();
            _assets=Resources.Load<PresentationAssetDatabase>("ClickDungeonPresentationAssets");
            EnsureEventSystem();
            Build();
            ApplySafeArea();
            StartMenuMusic();
        }

        private static GameContent LoadContent()
        {
            var generated=Resources.Load<GeneratedContentDatabase>("ClickDungeonGeneratedContent");
            if(generated==null)
            {
                Debug.LogWarning("Generated content database is missing on the main menu. Development fallback definitions will be used; release validation must reject this state.");
                return GameContent.CreateDevelopmentFallback();
            }
            try{return generated.CreateCatalog();}
            catch(Exception ex)
            {
                Debug.LogError($"Generated content database failed validation on the main menu. Development fallback definitions will be used. {ex}");
                return GameContent.CreateDevelopmentFallback();
            }
        }

        private void Update(){if(Screen.safeArea!=_lastSafeArea)ApplySafeArea();}

        private void Build()
        {
            var canvasGo=new GameObject("MainMenuCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform,false);
            canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            scaler.matchWidthOrHeight=.5f;

            _viewport=CreateRect("SafeViewport",canvasGo.transform);
            Stretch(_viewport);
            var viewportImage=_viewport.gameObject.AddComponent<Image>();
            viewportImage.color=BackdropColor;
            viewportImage.raycastTarget=false;

            _root=CreateRect("TitleScreen",_viewport);
            Stretch(_root);

            BuildBackdrop();
            BuildTitleHeader();
            BuildSelectedHeroPanel();
            BuildTopUtilities();
            BuildContinuePanel();
            BuildHeroShowcase();
            BuildDailyRewardPanel();
            BuildBottomNavigation();
            BuildStatusBar();
            BuildUtilityDrawer();
            BuildHeroSelectOverlay();
            RefreshSelectedSlotPresentation();
        }

        private void BuildBackdrop()
        {
            Sprite crypt=FirstSprite("biome.crypt","dungeon.floor.stone");
            var background=CreateSprite("DungeonHallBackdrop",_root,crypt,new Vector2(0,0),new Vector2(1,1),false);
            background.color=crypt==null?BackdropColor:new Color(.36f,.38f,.42f,.56f);

            CreatePanel("TopShade",_root,new Vector2(0,.80f),new Vector2(1,1),new Color(.008f,.014f,.023f,.72f),Color.clear);
            CreatePanel("BottomShade",_root,new Vector2(0,0),new Vector2(1,.18f),new Color(.008f,.014f,.023f,.86f),Color.clear);
            CreatePanel("LeftStoneBay",_root,new Vector2(0,.14f),new Vector2(.19f,.83f),new Color(.012f,.025f,.038f,.78f),new Color(.35f,.22f,.07f,.55f));
            CreatePanel("RightStoneBay",_root,new Vector2(.81f,.14f),new Vector2(1,.83f),new Color(.012f,.025f,.038f,.78f),new Color(.35f,.22f,.07f,.55f));

            AddBanner("LeftBanner",new Vector2(.018f,.36f),new Vector2(.095f,.69f),"EXPLORE.\nSURVIVE.\nLOOT.\nREPEAT.");
            AddBanner("RightBanner",new Vector2(.905f,.37f),new Vector2(.982f,.70f),"EVERY\nTILE\nTELLS A\nSTORY.");

            AddDecorativeSprite("TorchLeft","dungeon.torch",new Vector2(.205f,.42f),new Vector2(.255f,.62f));
            AddDecorativeSprite("TorchRight","dungeon.torch",new Vector2(.745f,.42f),new Vector2(.795f,.62f));
        }

        private void BuildTitleHeader()
        {
            var title=CreateLabel("Title",_root,TitleText,96,new Vector2(.28f,.845f),new Vector2(.72f,.98f),TextAlignmentOptions.Center,GoldSoft,true);
            var outline=title.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(.18f,.055f,.01f,1f);
            outline.effectDistance=new Vector2(3,-3);
            var shadow=title.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(0,0,0,.85f);
            shadow.effectDistance=new Vector2(5,-6);

            var taglinePanel=CreatePanel("TaglineFrame",_root,new Vector2(.34f,.795f),new Vector2(.66f,.855f),new Color(.045f,.055f,.065f,.95f),new Color(.54f,.33f,.08f,.95f));
            CreateLabel("Tagline",taglinePanel,"EXPLORE. SURVIVE. LOOT. REPEAT.",27,new Vector2(.03f,.05f),new Vector2(.97f,.95f),TextAlignmentOptions.Center,GoldSoft,true);
        }

        private void BuildSelectedHeroPanel()
        {
            var panel=CreatePanel("SelectedHeroPanel",_root,new Vector2(.025f,.835f),new Vector2(.265f,.975f),PanelColor,Gold);
            _selectedHeroPortrait=CreateSprite("SelectedHeroPortrait",panel,null,new Vector2(.025f,.12f),new Vector2(.27f,.90f),true);
            _selectedHeroName=CreateLabel("SelectedHeroName",panel,"Sir Clickington",29,new Vector2(.30f,.48f),new Vector2(.96f,.88f),TextAlignmentOptions.Left,Cream,true);
            _selectedHeroMeta=CreateLabel("SelectedHeroMeta",panel,"KNIGHT • STORY CAMPAIGN",18,new Vector2(.30f,.20f),new Vector2(.96f,.50f),TextAlignmentOptions.Left,Muted,false);
            CreateLabel("SlotTag",panel,"ACTIVE HERO",14,new Vector2(.30f,.04f),new Vector2(.96f,.23f),TextAlignmentOptions.Left,GoldSoft,true);
        }

        private void BuildTopUtilities()
        {
            CreateButton("AchievementsButton",_root,"★",ShowAchievements,new Vector2(.79f,.89f),new Vector2(.835f,.97f),PanelBlue,Gold,34);
            CreateButton("MailButton",_root,"✉",()=>ShowStatus("No inbox service is connected to this build."),new Vector2(.842f,.89f),new Vector2(.887f,.97f),PanelBlue,Gold,31);
            CreateButton("SettingsTopButton",_root,"⚙",ShowSettings,new Vector2(.894f,.89f),new Vector2(.939f,.97f),PanelBlue,Gold,30);
            CreateButton("MenuButton",_root,"☰",ToggleUtilityDrawer,new Vector2(.946f,.89f),new Vector2(.99f,.97f),PanelBlue,Gold,30);
        }

        private void BuildContinuePanel()
        {
            var panel=CreatePanel("ContinuePanel",_root,new Vector2(.115f,.29f),new Vector2(.31f,.76f),PanelColor,Gold);
            CreateLabel("ContinueTitle",panel,"CONTINUE",34,new Vector2(.05f,.86f),new Vector2(.95f,.98f),TextAlignmentOptions.Center,Cream,true);

            var previewFrame=CreatePanel("DungeonPreviewFrame",panel,new Vector2(.08f,.43f),new Vector2(.92f,.84f),new Color(.01f,.018f,.025f,1f),new Color(.56f,.36f,.11f,1f));
            var preview=CreateSprite("DungeonPreview",previewFrame,FirstSprite("biome.crypt","dungeon.door.locked","dungeon.floor.stone"),new Vector2(.03f,.03f),new Vector2(.97f,.97f),true);
            preview.color=new Color(.88f,.88f,.88f,1f);

            _continueFloor=CreateLabel("ContinueFloor",panel,"FLOOR 1",30,new Vector2(.08f,.31f),new Vector2(.92f,.43f),TextAlignmentOptions.Center,Cream,true);
            _continueDetail=CreateLabel("ContinueDetail",panel,"SLOT 1 — EMPTY",17,new Vector2(.08f,.23f),new Vector2(.92f,.32f),TextAlignmentOptions.Center,Muted,false);

            for(int slot=1;slot<=4;slot++)
            {
                int captured=slot;
                float left=.08f+(slot-1)*.215f;
                Button b=CreateButton("Slot"+slot,panel,slot.ToString(),()=>SelectSlot(captured),new Vector2(left,.12f),new Vector2(left+.17f,.22f),PanelBlue,Gold,17);
                _slotButtonBackgrounds[slot-1]=b.targetGraphic as Image;
            }

            Button action=CreateButton("ContinueAction",panel,"PLAY",PrimaryPlay,new Vector2(.16f,.015f),new Vector2(.84f,.105f),PlayGreen,GoldSoft,23);
            _continueActionLabel=action.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void BuildHeroShowcase()
        {
            var stage=CreatePanel("HeroShowcaseStage",_root,new Vector2(.31f,.17f),new Vector2(.76f,.78f),new Color(.015f,.025f,.035f,.17f),Color.clear);
            CreatePanel("HeroPedestal",stage,new Vector2(.16f,.02f),new Vector2(.84f,.13f),new Color(.11f,.075f,.035f,.92f),new Color(.48f,.30f,.08f,.9f));
            _heroShowcase=CreateSprite("HeroShowcase",stage,null,new Vector2(.10f,.08f),new Vector2(.90f,.98f),true);
        }

        private void BuildDailyRewardPanel()
        {
            var panel=CreatePanel("DailyRewardPanel",_root,new Vector2(.765f,.29f),new Vector2(.91f,.76f),PanelColor,Gold);
            CreateLabel("DailyRewardTitle",panel,"DAILY REWARD",31,new Vector2(.05f,.86f),new Vector2(.95f,.98f),TextAlignmentOptions.Center,Cream,true);
            var chest=CreateSprite("DailyRewardChest",panel,FirstSprite("chest.standard","chest.open"),new Vector2(.14f,.39f),new Vector2(.86f,.82f),true);
            chest.color=Color.white;
            CreateLabel("DailyRewardCaption",panel,"Claim Your Reward!",19,new Vector2(.08f,.28f),new Vector2(.92f,.40f),TextAlignmentOptions.Center,Cream,true);
            CreateButton("ClaimDailyReward",panel,"CLAIM",ShowDailyRewardUnavailable,new Vector2(.16f,.12f),new Vector2(.84f,.27f),PlayGreen,GoldSoft,25);
            CreateLabel("DailyRewardNote",panel,"Reward service pending",12,new Vector2(.10f,.02f),new Vector2(.90f,.11f),TextAlignmentOptions.Center,Muted,false);
        }

        private void BuildBottomNavigation()
        {
            var bar=CreatePanel("BottomNavigation",_root,new Vector2(.035f,.018f),new Vector2(.965f,.145f),new Color(.012f,.027f,.045f,.98f),Gold);
            string[] labels={"PLAY","HERO SELECT","INVENTORY","TALENTS","SHOP","SETTINGS","QUIT"};
            UnityEngine.Events.UnityAction[] actions={PrimaryPlay,ShowHeroSelect,ShowInventory,ShowTalents,ShowShop,ShowSettings,QuitGame};
            for(int i=0;i<labels.Length;i++)
            {
                float left=.012f+i*.141f;
                float right=left+.128f;
                Color fill=i==0?PlayGreen:(i==labels.Length-1?QuitRed:PanelBlue);
                CreateButton("Nav"+labels[i].Replace(" ",string.Empty),bar,labels[i],actions[i],new Vector2(left,.08f),new Vector2(right,.92f),fill,Gold,i==1?17:19);
            }
        }

        private void BuildStatusBar()
        {
            _status=CreateLabel("Status",_root,"Choose PLAY to continue or begin a run.",17,new Vector2(.30f,.145f),new Vector2(.70f,.18f),TextAlignmentOptions.Center,Muted,false);
        }

        private void BuildUtilityDrawer()
        {
            var drawer=CreatePanel("UtilityDrawer",_root,new Vector2(.73f,.49f),new Vector2(.965f,.88f),new Color(.015f,.035f,.055f,.99f),Gold);
            _utilityDrawer=drawer.gameObject;
            CreateLabel("DrawerTitle",drawer,"MENU",26,new Vector2(.08f,.84f),new Vector2(.92f,.97f),TextAlignmentOptions.Center,Cream,true);
            CreateButton("DrawerAchievements",drawer,"ACHIEVEMENTS",ShowAchievements,new Vector2(.08f,.68f),new Vector2(.92f,.81f),PanelBlue,Gold,17);
            CreateButton("DrawerAbyss",drawer,"ENTER THE ABYSS",StartAbyss,new Vector2(.08f,.52f),new Vector2(.92f,.65f),PanelBlue,Gold,16);
            if(!_services.Store.FullGameUnlocked)CreateButton("DrawerUnlock",drawer,"UNLOCK FULL GAME",UnlockFullGame,new Vector2(.08f,.36f),new Vector2(.92f,.49f),PlayGreen,Gold,16);
            CreateButton("DrawerDelete",drawer,"DELETE SLOT",DeleteSelected,new Vector2(.08f,.20f),new Vector2(.92f,.33f),QuitRed,Gold,16);
            CreateButton("DrawerClose",drawer,"CLOSE",ToggleUtilityDrawer,new Vector2(.22f,.04f),new Vector2(.78f,.16f),PanelBlue,Gold,15);
            _utilityDrawer.SetActive(false);
        }

        private void BuildHeroSelectOverlay()
        {
            var overlay=CreatePanel("HeroSelectOverlay",_root,new Vector2(0,0),new Vector2(1,1),new Color(0,0,0,.86f),Color.clear);
            _heroSelectOverlay=overlay.gameObject;
            var panel=CreatePanel("HeroSelectPanel",overlay,new Vector2(.27f,.10f),new Vector2(.73f,.90f),new Color(.015f,.035f,.055f,.99f),Gold);
            _heroSelectContent=panel;
            CreateLabel("HeroSelectTitle",panel,"CHOOSE YOUR HERO",34,new Vector2(.08f,.89f),new Vector2(.92f,.98f),TextAlignmentOptions.Center,Cream,true);
            CreateLabel("HeroSelectHint",panel,"Choose a class, then choose its hero identity. Selecting the card starts a new run.",15,new Vector2(.08f,.83f),new Vector2(.92f,.89f),TextAlignmentOptions.Center,Muted,false);

            CreateButton("PreviousClass",panel,"◀",()=>CycleHeroClass(-1),new Vector2(.06f,.72f),new Vector2(.16f,.81f),PanelBlue,Gold,22);
            _heroSelectClassLabel=CreateLabel("HeroClassLabel",panel,string.Empty,25,new Vector2(.18f,.72f),new Vector2(.82f,.81f),TextAlignmentOptions.Center,GoldSoft,true);
            CreateButton("NextClass",panel,"▶",()=>CycleHeroClass(1),new Vector2(.84f,.72f),new Vector2(.94f,.81f),PanelBlue,Gold,22);

            CreateButton("PreviousHero",panel,"◀",()=>CycleHeroIdentity(-1),new Vector2(.06f,.62f),new Vector2(.16f,.70f),PanelBlue,Gold,20);
            _heroSelectHeroLabel=CreateLabel("HeroIdentityLabel",panel,string.Empty,21,new Vector2(.18f,.62f),new Vector2(.82f,.70f),TextAlignmentOptions.Center,Cream,true);
            CreateButton("NextHero",panel,"▶",()=>CycleHeroIdentity(1),new Vector2(.84f,.62f),new Vector2(.94f,.70f),PanelBlue,Gold,20);

            _heroSelectCardHost=CreateRect("HeroCardHost",panel);
            SetAnchors(_heroSelectCardHost,new Vector2(.06f,.13f),new Vector2(.94f,.59f));
            _heroSelectClassIndex=0;
            _heroSelectHeroIndex=0;
            RefreshHeroSelectCard();

            CreateButton("HeroSelectClose",panel,"CLOSE",HideHeroSelect,new Vector2(.34f,.02f),new Vector2(.66f,.085f),PanelBlue,Gold,16);
            _heroSelectOverlay.SetActive(false);
        }

        private void CycleHeroClass(int delta)
        {
            int count=HeroIdentityCatalog.ClassDisplayOrder.Count;
            if(count==0)return;
            _heroSelectClassIndex=WrapIndex(_heroSelectClassIndex+delta,count);
            _heroSelectHeroIndex=0;
            RefreshHeroSelectCard();
        }

        private void CycleHeroIdentity(int delta)
        {
            var heroes=HeroIdentityCatalog.ForClass(CurrentHeroSelectClass()).ToArray();
            if(heroes.Length<=1)
            {
                _heroSelectHeroIndex=0;
                RefreshHeroSelectCard();
                return;
            }
            _heroSelectHeroIndex=WrapIndex(_heroSelectHeroIndex+delta,heroes.Length);
            RefreshHeroSelectCard();
        }

        private HeroClassId CurrentHeroSelectClass()
        {
            int count=HeroIdentityCatalog.ClassDisplayOrder.Count;
            if(count==0)return HeroClassId.Knight;
            _heroSelectClassIndex=WrapIndex(_heroSelectClassIndex,count);
            return HeroIdentityCatalog.ClassDisplayOrder[_heroSelectClassIndex];
        }

        private void RefreshHeroSelectCard()
        {
            if(_heroSelectCardHost==null)return;
            HeroClassId selectedClass=CurrentHeroSelectClass();
            var heroes=HeroIdentityCatalog.ForClass(selectedClass).ToArray();
            if(_heroSelectClassLabel!=null)_heroSelectClassLabel.text=selectedClass.ToString().ToUpperInvariant();

            for(int i=_heroSelectCardHost.childCount-1;i>=0;i--)
                Destroy(_heroSelectCardHost.GetChild(i).gameObject);

            if(heroes.Length==0)
            {
                _heroSelectHeroIndex=0;
                if(_heroSelectHeroLabel!=null)_heroSelectHeroLabel.text="NO HERO REGISTERED";
                return;
            }

            _heroSelectHeroIndex=WrapIndex(_heroSelectHeroIndex,heroes.Length);
            HeroIdentityDefinition hero=heroes[_heroSelectHeroIndex];
            if(_heroSelectHeroLabel!=null)
                _heroSelectHeroLabel.text=heroes.Length>1?$"{hero.DisplayName}  {_heroSelectHeroIndex+1}/{heroes.Length}":hero.DisplayName;
            AddHeroCard(hero);
        }

        private void AddHeroCard(HeroIdentityDefinition hero)
        {
            HeroCardDescriptor card=HeroCardPresentation.Describe(hero);
            var rt=CreatePanel("HeroCard_"+card.HeroId,_heroSelectCardHost,new Vector2(0,0),new Vector2(1,1),new Color(.035f,.055f,.075f,.98f),new Color(.55f,.42f,.18f,.85f));
            var background=rt.GetComponent<Image>();
            var button=rt.gameObject.AddComponent<Button>();
            button.targetGraphic=background;
            button.onClick.AddListener(()=>StartNew(card.HeroId));

            var portraitFrame=CreatePanel("PortraitFrame",rt,new Vector2(.04f,.10f),new Vector2(.34f,.90f),new Color(.012f,.018f,.028f,1f),new Color(.45f,.31f,.10f,.9f));
            var portrait=CreateSprite("Portrait",portraitFrame,ResolveHeroCardSprite(card),new Vector2(.06f,.06f),new Vector2(.94f,.94f),true);
            portrait.raycastTarget=false;

            CreateLabel("Name",rt,card.DisplayName,30,new Vector2(.39f,.53f),new Vector2(.94f,.88f),TextAlignmentOptions.Left,Cream,true);
            CreateLabel("Class",rt,card.ClassLabel,19,new Vector2(.39f,.31f),new Vector2(.82f,.54f),TextAlignmentOptions.Left,Muted,false);
            if(!string.IsNullOrEmpty(card.Badge))CreateLabel("Badge",rt,card.Badge,14,new Vector2(.39f,.16f),new Vector2(.94f,.32f),TextAlignmentOptions.Left,GoldSoft,true);
            CreateLabel("SelectHint",rt,"SELECT",15,new Vector2(.74f,.04f),new Vector2(.94f,.16f),TextAlignmentOptions.Right,GoldSoft,true);
        }

        private static int WrapIndex(int value,int count)
        {
            if(count<=0)return 0;
            int wrapped=value%count;
            return wrapped<0?wrapped+count:wrapped;
        }

        private void AddBanner(string name,Vector2 min,Vector2 max,string text)
        {
            var banner=CreatePanel(name,_root,min,max,new Color(.025f,.08f,.16f,.94f),new Color(.57f,.36f,.09f,.9f));
            CreateLabel("BannerText",banner,text,19,new Vector2(.10f,.08f),new Vector2(.90f,.92f),TextAlignmentOptions.Center,GoldSoft,true);
        }

        private void AddDecorativeSprite(string name,string id,Vector2 min,Vector2 max)
        {
            Sprite sprite=_assets?.SpriteFor(id);
            if(sprite==null)return;
            var image=CreateSprite(name,_root,sprite,min,max,true);
            image.color=new Color(1f,.90f,.72f,.92f);
        }

        private void RefreshSelectedSlotPresentation()
        {
            HeroCardDescriptor card=SelectedHeroCard();
            SlotMetaState meta=LoadSelectedMeta();
            if(card!=null)
            {
                if(_selectedHeroName!=null)_selectedHeroName.text=card.DisplayName;
                if(_selectedHeroMeta!=null)_selectedHeroMeta.text=meta==null?$"{card.ClassLabel} • {(string.IsNullOrEmpty(card.Badge)?"NEW ADVENTURE":card.Badge)}":$"{card.ClassLabel} • MASTERY {meta.ClassMastery}";
                Sprite portrait=ResolveHeroCardSprite(card);
                if(_selectedHeroPortrait!=null){_selectedHeroPortrait.sprite=portrait;_selectedHeroPortrait.enabled=portrait!=null;}
                Sprite showcase=ResolveHeroShowcaseSprite(card);
                if(_heroShowcase!=null){_heroShowcase.sprite=showcase;_heroShowcase.enabled=showcase!=null;}
            }

            bool exists=_saves.SlotExists(_selectedSlot);
            if(_continueFloor!=null)_continueFloor.text=exists?$"FLOOR {Math.Max(1,meta?.BestFloor??1)}":"NEW ADVENTURE";
            if(_continueDetail!=null)_continueDetail.text=exists?$"SLOT {_selectedSlot} • {(card?.DisplayName??"HERO")}":$"SLOT {_selectedSlot} — EMPTY";
            if(_continueActionLabel!=null)_continueActionLabel.text=exists?"CONTINUE":"PLAY";
            for(int i=0;i<_slotButtonBackgrounds.Length;i++)if(_slotButtonBackgrounds[i]!=null)_slotButtonBackgrounds[i].color=i==_selectedSlot-1?new Color(.18f,.27f,.40f,1f):PanelBlue;
        }

        private HeroCardDescriptor SelectedHeroCard()
        {
            string heroId="clickington";
            SlotMetaState meta=LoadSelectedMeta();
            if(meta!=null)
            {
                HeroClassId cls;
                if(!Enum.TryParse(meta.HeroClassId,true,out cls))cls=HeroClassId.Knight;
                heroId=HeroIdentityCatalog.ResolveHeroId(cls,meta.HeroId);
            }

            HeroIdentityDefinition first=null;
            foreach(var hero in HeroIdentityCatalog.All)
            {
                if(first==null)first=hero;
                if(string.Equals(hero.HeroId,heroId,StringComparison.OrdinalIgnoreCase))return HeroCardPresentation.Describe(hero);
            }
            return first==null?null:HeroCardPresentation.Describe(first);
        }

        private SlotMetaState LoadSelectedMeta()
        {
            try{return _saves.LoadSlot(_selectedSlot)?.payload?.Meta;}
            catch(Exception ex){Debug.LogWarning($"Slot {_selectedSlot} preview failed: {ex.Message}");return null;}
        }

        private Sprite ResolveHeroCardSprite(HeroCardDescriptor card)
        {
            if(_assets==null||card==null)return null;
            foreach(string key in card.SpriteKeys)
            {
                var sprite=_assets.SpriteFor(key);
                if(sprite!=null)return sprite;
            }
            return null;
        }

        private Sprite ResolveHeroShowcaseSprite(HeroCardDescriptor card)
        {
            if(_assets==null||card==null)return null;
            string prefix="hero."+card.HeroId.ToLowerInvariant()+".";
            string[] keys={prefix+"select",prefix+"roster",prefix+"gameplay",prefix+"portrait"};
            foreach(string key in keys)
            {
                Sprite sprite=_assets.SpriteFor(key);
                if(sprite!=null)return sprite;
            }
            return ResolveHeroCardSprite(card);
        }

        private Sprite FirstSprite(params string[] ids)
        {
            if(_assets==null||ids==null)return null;
            foreach(string id in ids)
            {
                Sprite sprite=_assets.SpriteFor(id);
                if(sprite!=null)return sprite;
            }
            return null;
        }

        private void ApplySafeArea()
        {
            if(_viewport==null)return;
            Rect safe=Screen.safeArea;
            _lastSafeArea=safe;
            Vector2 min=safe.position;
            Vector2 max=safe.position+safe.size;
            min.x/=Screen.width;min.y/=Screen.height;max.x/=Screen.width;max.y/=Screen.height;
            _viewport.anchorMin=min;_viewport.anchorMax=max;_viewport.offsetMin=Vector2.zero;_viewport.offsetMax=Vector2.zero;
        }

        private void StartMenuMusic()
        {
            var clip=_assets?.AudioFor("music.menu");
            if(clip==null)return;
            var source=gameObject.AddComponent<AudioSource>();
            source.clip=clip;source.loop=true;source.volume=.5f;source.spatialBlend=0f;source.Play();
        }

        private string SlotLabel(int slot)
        {
            try
            {
                var doc=_saves.LoadSlot(slot);
                if(doc?.payload==null)return $"Slot {slot} — Empty";
                var meta=doc.payload.Meta;
                HeroClassId cls;
                if(!Enum.TryParse(meta.HeroClassId,true,out cls))cls=HeroClassId.Knight;
                string heroId=HeroIdentityCatalog.ResolveHeroId(cls,meta.HeroId);
                string heroName=HeroIdentityCatalog.DisplayNameForHero(heroId);
                string complete=meta.CampaignCompleted?" ✓":"";
                return $"Slot {slot} — {heroName} ({cls}){complete} — Mastery {meta.ClassMastery} — Floor {meta.BestFloor} — Abyss {meta.BestAbyssDepth}";
            }
            catch(Exception ex){Debug.LogWarning($"Slot {slot} preview failed: {ex.Message}");return $"Slot {slot} — Recovery Required";}
        }

        private void SelectSlot(int slot)
        {
            _selectedSlot=slot;
            RefreshSelectedSlotPresentation();
            ShowStatus(SlotLabel(slot));
        }

        private void PrimaryPlay()
        {
            if(_saves.SlotExists(_selectedSlot))Continue();
            else ShowHeroSelect();
        }

        private void ShowHeroSelect(){RefreshHeroSelectCard();if(_heroSelectOverlay!=null)_heroSelectOverlay.SetActive(true);}
        private void HideHeroSelect(){if(_heroSelectOverlay!=null)_heroSelectOverlay.SetActive(false);}
        private void ToggleUtilityDrawer(){if(_utilityDrawer!=null)_utilityDrawer.SetActive(!_utilityDrawer.activeSelf);}
        private void ShowInventory(){ShowStatus("Inventory is managed inside an active dungeon run.");}
        private void ShowTalents(){ShowStatus("Talents are managed inside an active dungeon run.");}
        private void ShowShop(){ShowStatus(_services.Store.FullGameUnlocked?"Full game unlocked. Run-based shop features remain available during play.":"Full-game upgrade is available from the menu button in the upper-right.");}
        private void ShowSettings(){ShowStatus($"Settings — music {Mathf.RoundToInt(_account.MusicVolume*100)}%, SFX {Mathf.RoundToInt(_account.SfxVolume*100)}%, haptics {(_account.HapticsEnabled?"on":"off")}. In-run settings remain unchanged.");}
        private void ShowDailyRewardUnavailable(){ShowStatus("Daily Reward is presentation-only for now; no reward service exists yet, so nothing was granted.");}
        private void QuitGame(){ShowStatus("Quit requested.");Application.Quit();}
        private void ShowStatus(string message){if(_status!=null)_status.text=message??string.Empty;}

        private void StartNew(string heroId)
        {
            HeroClassId cls=HeroIdentityCatalog.ClassForHero(heroId);
            PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",0);PlayerPrefs.SetInt("cd2.abyss",0);PlayerPrefs.SetInt("cd2.class",(int)cls);PlayerPrefs.SetString("cd2.hero",heroId);PlayerPrefs.SetString("cd2.seed",unchecked((uint)DateTime.UtcNow.Ticks).ToString());PlayerPrefs.Save();SceneManager.LoadScene("Game");
        }

        private void Continue()
        {
            if(!_saves.SlotExists(_selectedSlot)){ShowStatus("That slot is empty.");return;}
            PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",1);PlayerPrefs.SetInt("cd2.abyss",0);PlayerPrefs.Save();SceneManager.LoadScene("Game");
        }

        private void StartAbyss()
        {
            if(!_services.Store.FullGameUnlocked){ShowStatus("The Abyss is part of the full game.");return;}
            try
            {
                var doc=_saves.LoadSlot(_selectedSlot);var meta=doc?.payload?.Meta;
                if(meta==null||!meta.CampaignCompleted){ShowStatus("Complete Floor 50 on this slot to unlock the Abyss.");return;}
                HeroClassId cls;if(!Enum.TryParse(meta.HeroClassId,true,out cls))cls=HeroClassId.Knight;
                string heroId=HeroIdentityCatalog.ResolveHeroId(cls,meta.HeroId);
                PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",0);PlayerPrefs.SetInt("cd2.abyss",1);PlayerPrefs.SetInt("cd2.class",(int)cls);PlayerPrefs.SetString("cd2.hero",heroId);PlayerPrefs.SetString("cd2.seed",unchecked((uint)DateTime.UtcNow.Ticks).ToString());PlayerPrefs.Save();SceneManager.LoadScene("Game");
            }
            catch(Exception ex){ShowStatus($"Abyss unavailable: {ex.Message}");}
        }

        private void ShowAchievements()
        {
            var unlocked=new System.Collections.Generic.HashSet<string>(_account.AchievementIds,StringComparer.Ordinal);
            var lines=new System.Collections.Generic.List<string>();
            foreach(var achievement in _content.Achievements)lines.Add((unlocked.Contains(achievement.Id)?"✓ ":"□ ")+achievement.DisplayName);
            ShowStatus(lines.Count==0?"No achievement definitions loaded.":string.Join("  •  ",lines));
        }

        private void UnlockFullGame()
        {
            _services.Store.PurchaseFullGame((ok,message)=>ShowStatus(ok?"Full game unlocked. Existing demo runs can now descend past Floor 5.":$"Unlock failed: {message}"));
        }

        private void DeleteSelected()
        {
            _saves.DeleteSlot(_selectedSlot);
            RefreshSelectedSlotPresentation();
            ShowStatus($"Deleted slot {_selectedSlot}.");
        }

        private RectTransform CreatePanel(string name,Transform parent,Vector2 min,Vector2 max,Color fill,Color outlineColor)
        {
            var rt=CreateRect(name,parent);SetAnchors(rt,min,max);
            var image=rt.gameObject.AddComponent<Image>();image.color=fill;
            if(outlineColor.a>0f){var outline=rt.gameObject.AddComponent<Outline>();outline.effectColor=outlineColor;outline.effectDistance=new Vector2(2,-2);}
            return rt;
        }

        private TMP_Text CreateLabel(string name,Transform parent,string value,float size,Vector2 min,Vector2 max,TextAlignmentOptions alignment,Color color,bool bold)
        {
            var rt=CreateRect(name,parent);SetAnchors(rt,min,max);
            var text=rt.gameObject.AddComponent<TextMeshProUGUI>();
            text.text=value;text.fontSize=size;text.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;text.alignment=alignment;text.color=color;text.raycastTarget=false;
            text.enableAutoSizing=true;text.fontSizeMax=size;text.fontSizeMin=Mathf.Max(10f,size*.58f);text.overflowMode=TextOverflowModes.Ellipsis;
            return text;
        }

        private Button CreateButton(string name,Transform parent,string label,UnityEngine.Events.UnityAction action,Vector2 min,Vector2 max,Color fill,Color outlineColor,float fontSize)
        {
            var rt=CreatePanel(name,parent,min,max,fill,outlineColor);
            var image=rt.GetComponent<Image>();
            var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;if(action!=null)button.onClick.AddListener(action);
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1f,.92f,.74f,1f);colors.pressedColor=new Color(.82f,.68f,.48f,1f);colors.selectedColor=colors.highlightedColor;button.colors=colors;
            CreateLabel("Label",rt,label,fontSize,new Vector2(.04f,.05f),new Vector2(.96f,.95f),TextAlignmentOptions.Center,Cream,true);
            return button;
        }

        private Image CreateSprite(string name,Transform parent,Sprite sprite,Vector2 min,Vector2 max,bool preserveAspect)
        {
            var rt=CreateRect(name,parent);SetAnchors(rt,min,max);
            var image=rt.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=preserveAspect;image.raycastTarget=false;image.enabled=sprite!=null;return image;
        }

        private static RectTransform CreateRect(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();
        }

        private static void SetAnchors(RectTransform rt,Vector2 min,Vector2 max)
        {
            rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        }
    }
}
