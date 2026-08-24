using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ClickDungeon.Application.Persistence;
using ClickDungeon.Application.Content;
using ClickDungeon.Application.State;
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

        private void Start()
        {
            _saves=new LocalSaveRepository();_accounts=new AccountRepository();_account=_accounts.Load();_services=new ServiceRegistry();_services.Store.RefreshEntitlements();var generated=Resources.Load<GeneratedContentDatabase>("ClickDungeonGeneratedContent");_content=generated!=null?generated.CreateCatalog():GameContent.CreateDevelopmentFallback();EnsureEventSystem();Build();ApplySafeArea();StartMenuMusic();
        }

        private void Update(){if(Screen.safeArea!=_lastSafeArea)ApplySafeArea();}

        private void Build()
        {
            var canvasGo=new GameObject("MainMenuCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasGo.transform.SetParent(transform,false);canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
            _viewport=CreateRect("SafeViewport",canvasGo.transform);Stretch(_viewport);var viewportImage=_viewport.gameObject.AddComponent<Image>();viewportImage.color=new Color(.035f,.03f,.055f,1f);var mask=_viewport.gameObject.AddComponent<RectMask2D>();
            _root=CreateRect("Content",_viewport);_root.anchorMin=new Vector2(0,1);_root.anchorMax=new Vector2(1,1);_root.pivot=new Vector2(.5f,1);_root.offsetMin=Vector2.zero;_root.offsetMax=Vector2.zero;var layout=_root.gameObject.AddComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(80,80,80,100);layout.spacing=22;layout.childAlignment=TextAnchor.UpperCenter;layout.childControlHeight=true;layout.childForceExpandHeight=false;var fitter=_root.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;var scroll=_viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=_viewport;scroll.content=_root;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=38f;
            AddText("CLICKDUNGEON",54,110);AddText("Read the dungeon. Reveal the danger. Risk the deeper path.",24,100);
            for(int slot=1;slot<=4;slot++){int captured=slot;string label=SlotLabel(slot);AddButton(label,()=>SelectSlot(captured),92);}
            _status=AddText($"Select a slot.  Achievements {_account.AchievementIds.Count}/{new System.Collections.Generic.List<AchievementDefinition>(_content.Achievements).Count}",22,120);
            AddText("NEW RUN",30,64);foreach(HeroClassId cls in Enum.GetValues(typeof(HeroClassId))){HeroClassId captured=cls;AddButton(cls.ToString(),()=>StartNew(captured),74);}
            AddButton("Continue Selected Slot",Continue,82);AddButton("Achievements",ShowAchievements,74);AddButton("Enter the Abyss",StartAbyss,82);if(!_services.Store.FullGameUnlocked)AddButton("Unlock Full Game",UnlockFullGame,82);AddButton("Delete Selected Slot",DeleteSelected,70);
        }


        private void ApplySafeArea()
        {
            if(_viewport==null)return;Rect safe=Screen.safeArea;_lastSafeArea=safe;Vector2 min=safe.position;Vector2 max=safe.position+safe.size;min.x/=Screen.width;min.y/=Screen.height;max.x/=Screen.width;max.y/=Screen.height;_viewport.anchorMin=min;_viewport.anchorMax=max;_viewport.offsetMin=Vector2.zero;_viewport.offsetMax=Vector2.zero;
        }

        private void StartMenuMusic()
        {
            var assets=Resources.Load<PresentationAssetDatabase>("ClickDungeonPresentationAssets");var clip=assets?.AudioFor("music.menu");if(clip==null)return;var source=gameObject.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=.5f;source.spatialBlend=0f;source.Play();
        }
        private string SlotLabel(int slot)
        {
            try
            {
                var doc=_saves.LoadSlot(slot);if(doc?.payload==null)return $"Slot {slot} — Empty";var meta=doc.payload.Meta;string complete=meta.CampaignCompleted?" ✓":"";return $"Slot {slot} — {meta.HeroClassId}{complete} — Mastery {meta.ClassMastery} — Floor {meta.BestFloor} — Abyss {meta.BestAbyssDepth}";
            }
            catch(Exception ex){Debug.LogWarning($"Slot {slot} preview failed: {ex.Message}");return $"Slot {slot} — Recovery Required";}
        }
        private void SelectSlot(int slot){_selectedSlot=slot;_status.text=$"Selected slot {slot}.";}
        private void StartNew(HeroClassId cls)
        {
            PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",0);PlayerPrefs.SetInt("cd2.abyss",0);PlayerPrefs.SetInt("cd2.class",(int)cls);PlayerPrefs.SetString("cd2.seed",unchecked((uint)DateTime.UtcNow.Ticks).ToString());PlayerPrefs.Save();SceneManager.LoadScene("Game");
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
                HeroClassId cls;if(!Enum.TryParse(meta.HeroClassId,out cls))cls=HeroClassId.Knight;PlayerPrefs.SetInt("cd2.slot",_selectedSlot);PlayerPrefs.SetInt("cd2.continue",0);PlayerPrefs.SetInt("cd2.abyss",1);PlayerPrefs.SetInt("cd2.class",(int)cls);PlayerPrefs.SetString("cd2.seed",unchecked((uint)DateTime.UtcNow.Ticks).ToString());PlayerPrefs.Save();SceneManager.LoadScene("Game");
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
