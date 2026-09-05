using System;
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
        private LocalSaveRepository _saves;
        private RectTransform _root;
        private RectTransform _viewport;
        private Rect _lastSafeArea;
        private TMP_Text _status;
        private int _selectedSlot = 1;
        private ServiceRegistry _services;
        private AccountRepository _accounts;
        private AccountState _account;
        private GameContent _content;
        private PresentationAssetDatabase _assets;

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
            var canvasGo=new GameObject("MainMenuCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasGo.transform.SetParent(transform,false);canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
            _viewport=CreateRect("SafeViewport",canvasGo.transform);Stretch(_viewport);var viewportImage=_viewport.gameObject.AddComponent<Image>();viewportImage.color=new Color(.035f,.03f,.055f,1f);_viewport.gameObject.AddComponent<RectMask2D>();
            _root=CreateRect("Content",_viewport);_root.anchorMin=new Vector2(0,1);_root.anchorMax=new Vector2(1,1);_root.pivot=new Vector2(.5f,1);_root.offsetMin=Vector2.zero;_root.offsetMax=Vector2.zero;var layout=_root.gameObject.AddComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(80,80,80,100);layout.spacing=22;layout.childAlignment=TextAnchor.UpperCenter;layout.childControlHeight=true;layout.childForceExpandHeight=false;var fitter=_root.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;var scroll=_viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=_viewport;scroll.content=_root;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=38f;
            AddText("CLICKDUNGEON",54,110);AddText("Read the dungeon. Reveal the danger. Risk the deeper path.",24,100);
            for(int slot=1;slot<=4;slot++){int captured=slot;string label=SlotLabel(slot);AddButton(label,()=>SelectSlot(captured),92);}
            _status=AddText($"Select a slot.  Achievements {_account.AchievementIds.Count}/{new System.Collections.Generic.List<AchievementDefinition>(_content.Achievements).Count}",22,120);
            AddText("NEW RUN — CHOOSE YOUR HERO",30,64);
            foreach(var hero in HeroIdentityCatalog.All)AddHeroCard(hero);
            AddButton("Continue Selected Slot",Continue,82);AddButton("Achievements",ShowAchievements,74);AddButton("Enter the Abyss",StartAbyss,82);if(!_services.Store.FullGameUnlocked)AddButton("Unlock Full Game",UnlockFullGame,82);AddButton("Delete Selected Slot",DeleteSelected,70);
        }

        private void AddHeroCard(HeroIdentityDefinition hero)
        {
            HeroCardDescriptor card=HeroCardPresentation.Describe(hero);
            var rt=CreateRect("HeroCard_"+card.HeroId,_root);
            var background=rt.gameObject.AddComponent<Image>();background.color=new Color(.075f,.08f,.12f,.98f);
            var outline=rt.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.55f,.42f,.18f,.85f);outline.effectDistance=new Vector2(2,-2);
            var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=background;button.onClick.AddListener(()=>StartNew(card.HeroId));
            var element=rt.gameObject.AddComponent<LayoutElement>();element.preferredHeight=150;

            var portraitFrame=CreateRect("PortraitFrame",rt);portraitFrame.anchorMin=new Vector2(.015f,.08f);portraitFrame.anchorMax=new Vector2(.22f,.92f);portraitFrame.offsetMin=Vector2.zero;portraitFrame.offsetMax=Vector2.zero;
            var portraitBack=portraitFrame.gameObject.AddComponent<Image>();portraitBack.color=new Color(.025f,.025f,.04f,1f);
            var portraitRt=CreateRect("Portrait",portraitFrame);portraitRt.anchorMin=new Vector2(.05f,.05f);portraitRt.anchorMax=new Vector2(.95f,.95f);portraitRt.offsetMin=Vector2.zero;portraitRt.offsetMax=Vector2.zero;
            var portrait=portraitRt.gameObject.AddComponent<Image>();portrait.preserveAspect=true;portrait.raycastTarget=false;portrait.sprite=ResolveHeroCardSprite(card);portrait.enabled=portrait.sprite!=null;

            var nameRt=CreateRect("Name",rt);nameRt.anchorMin=new Vector2(.25f,.47f);nameRt.anchorMax=new Vector2(.97f,.91f);nameRt.offsetMin=Vector2.zero;nameRt.offsetMax=Vector2.zero;
            var name=nameRt.gameObject.AddComponent<TextMeshProUGUI>();name.text=card.DisplayName;name.fontSize=30;name.fontStyle=FontStyles.Bold;name.alignment=TextAlignmentOptions.Left;name.color=new Color(.96f,.91f,.77f,1f);name.raycastTarget=false;

            var classRt=CreateRect("Class",rt);classRt.anchorMin=new Vector2(.25f,.10f);classRt.anchorMax=new Vector2(.65f,.50f);classRt.offsetMin=Vector2.zero;classRt.offsetMax=Vector2.zero;
            var classText=classRt.gameObject.AddComponent<TextMeshProUGUI>();classText.text=card.ClassLabel;classText.fontSize=20;classText.alignment=TextAlignmentOptions.Left;classText.color=new Color(.74f,.76f,.84f,1f);classText.raycastTarget=false;

            if(!string.IsNullOrEmpty(card.Badge))
            {
                var badgeRt=CreateRect("Badge",rt);badgeRt.anchorMin=new Vector2(.64f,.10f);badgeRt.anchorMax=new Vector2(.97f,.45f);badgeRt.offsetMin=Vector2.zero;badgeRt.offsetMax=Vector2.zero;
                var badgeBack=badgeRt.gameObject.AddComponent<Image>();badgeBack.color=new Color(.30f,.20f,.42f,.96f);badgeBack.raycastTarget=false;
                var badgeLabel=CreateRect("Label",badgeRt).gameObject.AddComponent<TextMeshProUGUI>();badgeLabel.text=card.Badge;badgeLabel.fontSize=15;badgeLabel.fontStyle=FontStyles.Bold;badgeLabel.alignment=TextAlignmentOptions.Center;badgeLabel.color=Color.white;badgeLabel.raycastTarget=false;Stretch(badgeLabel.rectTransform);
            }
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

        private void ApplySafeArea()
        {
            if(_viewport==null)return;Rect safe=Screen.safeArea;_lastSafeArea=safe;Vector2 min=safe.position;Vector2 max=safe.position+safe.size;min.x/=Screen.width;min.y/=Screen.height;max.x/=Screen.width;max.y/=Screen.height;_viewport.anchorMin=min;_viewport.anchorMax=max;_viewport.offsetMin=Vector2.zero;_viewport.offsetMax=Vector2.zero;
        }

        private void StartMenuMusic()
        {
            var clip=_assets?.AudioFor("music.menu");if(clip==null)return;var source=gameObject.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=.5f;source.spatialBlend=0f;source.Play();
        }
        private string SlotLabel(int slot)
        {
            try
            {
                var doc=_saves.LoadSlot(slot);if(doc?.payload==null)return $"Slot {slot} — Empty";var meta=doc.payload.Meta;HeroClassId cls;if(!Enum.TryParse(meta.HeroClassId,true,out cls))cls=HeroClassId.Knight;string heroId=HeroIdentityCatalog.ResolveHeroId(cls,meta.HeroId);string heroName=HeroIdentityCatalog.DisplayNameForHero(heroId);string complete=meta.CampaignCompleted?" ✓":"";return $"Slot {slot} — {heroName} ({cls}){complete} — Mastery {meta.ClassMastery} — Floor {meta.BestFloor} — Abyss {meta.BestAbyssDepth}";
            }
            catch(Exception ex){Debug.LogWarning($"Slot {slot} preview failed: {ex.Message}");return $"Slot {slot} — Recovery Required";}
        }
        private void SelectSlot(int slot){_selectedSlot=slot;_status.text=$"Selected slot {slot}.";}
        private void StartNew(string heroId)
        {
            HeroClassId cls=HeroIdentityCatalog.ClassForHero(heroId);PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",0);PlayerPrefs.SetInt("cd2.abyss",0);PlayerPrefs.SetInt("cd2.class",(int)cls);PlayerPrefs.SetString("cd2.hero",heroId);PlayerPrefs.SetString("cd2.seed",unchecked((uint)DateTime.UtcNow.Ticks).ToString());PlayerPrefs.Save();SceneManager.LoadScene("Game");
        }
        private void Continue()
        {
            if(!_saves.SlotExists(_selectedSlot)){_status.text="That slot is empty.";return;}PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",1);PlayerPrefs.SetInt("cd2.abyss",0);PlayerPrefs.Save();SceneManager.LoadScene("Game");
        }
        private void StartAbyss()
        {
            if(!_services.Store.FullGameUnlocked){_status.text="The Abyss is part of the full game.";return;}
            try
            {
                var doc=_saves.LoadSlot(_selectedSlot);var meta=doc?.payload?.Meta;if(meta==null||!meta.CampaignCompleted){_status.text="Complete Floor 50 on this slot to unlock the Abyss.";return;}
                HeroClassId cls;if(!Enum.TryParse(meta.HeroClassId,true,out cls))cls=HeroClassId.Knight;string heroId=HeroIdentityCatalog.ResolveHeroId(cls,meta.HeroId);PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",0);PlayerPrefs.SetInt("cd2.abyss",1);PlayerPrefs.SetInt("cd2.class",(int)cls);PlayerPrefs.SetString("cd2.hero",heroId);PlayerPrefs.SetString("cd2.seed",unchecked((uint)DateTime.UtcNow.Ticks).ToString());PlayerPrefs.Save();SceneManager.LoadScene("Game");
            }
            catch(Exception ex){_status.text=$"Abyss unavailable: {ex.Message}";}
        }

        private void ShowAchievements()
        {
            var unlocked=new System.Collections.Generic.HashSet<string>(_account.AchievementIds,StringComparer.Ordinal);var lines=new System.Collections.Generic.List<string>();
            foreach(var achievement in _content.Achievements)lines.Add((unlocked.Contains(achievement.Id)?"✓ ":"□ ")+achievement.DisplayName);
            _status.text=lines.Count==0?"No achievement definitions loaded.":string.Join("  •  ",lines);
        }

        private void UnlockFullGame()
        {
            _services.Store.PurchaseFullGame((ok,message)=>{_status.text=ok?"Full game unlocked. Existing demo runs can now descend past Floor 5.":$"Unlock failed: {message}";});
        }
        private void DeleteSelected(){_saves.DeleteSlot(_selectedSlot);_status.text=$"Deleted slot {_selectedSlot}. Return to Main to refresh labels.";}

        private TMP_Text AddText(string value,float size,float height){var rt=CreateRect("Text",_root);var t=rt.gameObject.AddComponent<TextMeshProUGUI>();t.text=value;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=Color.white;var e=rt.gameObject.AddComponent<LayoutElement>();e.preferredHeight=height;return t;}
        private void AddButton(string value,UnityEngine.Events.UnityAction action,float height){var rt=CreateRect(value,_root);var image=rt.gameObject.AddComponent<Image>();image.color=new Color(.13f,.14f,.18f,.95f);var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);var label=CreateRect("Label",rt).gameObject.AddComponent<TextMeshProUGUI>();label.text=value;label.fontSize=24;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;Stretch(label.rectTransform);var e=rt.gameObject.AddComponent<LayoutElement>();e.preferredHeight=height;}
        private static RectTransform CreateRect(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();}
        private static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;}
        private static void EnsureEventSystem(){if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));}
    }
}
