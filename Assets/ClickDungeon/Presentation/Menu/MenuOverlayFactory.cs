using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ClickDungeon.Application.Persistence;
using ClickDungeon.Application.Services;
using ClickDungeon.Application.State;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Menu
{
    public sealed class MenuOverlayFactory
    {
        private static readonly Color Backdrop=new Color(0f,0f,0f,.82f);
        private static readonly Color Panel=new Color(.018f,.040f,.060f,.995f);
        private static readonly Color Control=new Color(.035f,.080f,.115f,1f);
        private static readonly Color Gold=new Color(.95f,.62f,.16f,1f);
        private static readonly Color Cream=new Color(.96f,.91f,.78f,1f);
        private static readonly Color Muted=new Color(.72f,.75f,.80f,1f);
        private static readonly Color Green=new Color(.10f,.43f,.12f,.98f);

        private readonly AccountState _account;
        private readonly AccountRepository _accounts;
        private readonly IStoreService _store;
        private readonly GameObject _inventoryOverlay;
        private readonly GameObject _talentsOverlay;
        private readonly GameObject _settingsOverlay;
        private readonly GameObject _shopOverlay;
        private readonly TMP_Text _inventorySummary;
        private readonly TMP_Text _talentsSummary;
        private readonly TMP_Text _shopStatus;
        private readonly Button _shopPurchase;

        public MenuOverlayFactory(Transform root,AccountState account,AccountRepository accounts,IStoreService store)
        {
            if(root==null)throw new ArgumentNullException(nameof(root));
            _account=account??throw new ArgumentNullException(nameof(account));
            _accounts=accounts??throw new ArgumentNullException(nameof(accounts));
            _store=store??throw new ArgumentNullException(nameof(store));

            _inventoryOverlay=CreateSummaryOverlay(root,"InventoryOverlay","INVENTORY","InventoryOverlaySummary",out _inventorySummary);
            _talentsOverlay=CreateSummaryOverlay(root,"TalentsOverlay","TALENTS","TalentsOverlaySummary",out _talentsSummary);
            _settingsOverlay=CreateSettingsOverlay(root);
            _shopOverlay=CreateShopOverlay(root,out _shopStatus,out _shopPurchase);
        }

        public void ShowInventory(SlotSavePayload payload)
        {
            _inventorySummary.text=DescribeInventory(payload?.ActiveRun);
            Open(_inventoryOverlay);
        }

        public void ShowTalents(SlotSavePayload payload)
        {
            _talentsSummary.text=DescribeTalents(payload?.Meta);
            Open(_talentsOverlay);
        }

        public void ShowSettings()
        {
            Open(_settingsOverlay);
        }

        public void ShowShop()
        {
            RefreshShopState();
            Open(_shopOverlay);
        }

        private GameObject CreateSummaryOverlay(Transform root,string overlayName,string title,string summaryName,out TMP_Text summary)
        {
            GameObject overlay=CreateOverlayRoot(root,overlayName);
            RectTransform panel=CreatePanel(overlay.transform,overlayName+"Panel",new Vector2(.18f,.16f),new Vector2(.82f,.84f));
            CreateLabel(panel,title,34,new Vector2(.06f,.84f),new Vector2(.74f,.96f),TextAlignmentOptions.Left,true);
            CreateButton(panel,overlayName+"Close","CLOSE",()=>overlay.SetActive(false),new Vector2(.78f,.86f),new Vector2(.94f,.95f),Control);
            summary=CreateLabel(panel,summaryName,"",20,new Vector2(.07f,.10f),new Vector2(.93f,.79f),TextAlignmentOptions.TopLeft,false);
            overlay.SetActive(false);
            return overlay;
        }

        private GameObject CreateSettingsOverlay(Transform root)
        {
            GameObject overlay=CreateOverlayRoot(root,"SettingsOverlay");
            RectTransform panel=CreatePanel(overlay.transform,"SettingsOverlayPanel",new Vector2(.18f,.10f),new Vector2(.82f,.90f));
            CreateLabel(panel,"SETTINGS",34,new Vector2(.06f,.87f),new Vector2(.74f,.97f),TextAlignmentOptions.Left,true);
            CreateButton(panel,"SettingsOverlayClose","CLOSE",()=>overlay.SetActive(false),new Vector2(.78f,.88f),new Vector2(.94f,.96f),Control);

            CreateSettingLabel(panel,"MASTER VOLUME",.77f,.84f);
            CreateSlider(panel,"SettingsMasterVolume",_account.MasterVolume,0f,1f,v=>Persist(()=>_account.MasterVolume=v),.69f,.77f);
            CreateSettingLabel(panel,"MUSIC VOLUME",.61f,.68f);
            CreateSlider(panel,"SettingsMusicVolume",_account.MusicVolume,0f,1f,v=>Persist(()=>_account.MusicVolume=v),.53f,.61f);
            CreateSettingLabel(panel,"SFX VOLUME",.45f,.52f);
            CreateSlider(panel,"SettingsSfxVolume",_account.SfxVolume,0f,1f,v=>Persist(()=>_account.SfxVolume=v),.37f,.45f);
            CreateToggle(panel,"SettingsHaptics","HAPTICS",_account.HapticsEnabled,v=>Persist(()=>_account.HapticsEnabled=v),.27f,.35f);
            CreateToggle(panel,"SettingsReducedMotion","REDUCED MOTION",_account.ReducedMotion,v=>Persist(()=>_account.ReducedMotion=v),.17f,.25f);
            CreateSettingLabel(panel,"TEXT SCALE",.09f,.16f);
            CreateSlider(panel,"SettingsTextScale",_account.TextScale,.75f,1.5f,v=>Persist(()=>_account.TextScale=v),.02f,.10f);

            overlay.SetActive(false);
            return overlay;
        }

        private GameObject CreateShopOverlay(Transform root,out TMP_Text status,out Button purchase)
        {
            GameObject overlay=CreateOverlayRoot(root,"ShopOverlay");
            RectTransform panel=CreatePanel(overlay.transform,"ShopOverlayPanel",new Vector2(.24f,.20f),new Vector2(.76f,.80f));
            CreateLabel(panel,"SHOP",34,new Vector2(.07f,.81f),new Vector2(.67f,.95f),TextAlignmentOptions.Left,true);
            CreateButton(panel,"ShopOverlayClose","CLOSE",()=>overlay.SetActive(false),new Vector2(.73f,.83f),new Vector2(.93f,.94f),Control);
            status=CreateLabel(panel,"ShopOverlayStatus","",21,new Vector2(.08f,.47f),new Vector2(.92f,.75f),TextAlignmentOptions.Center,false);
            purchase=CreateButton(panel,"ShopPurchaseButton","UNLOCK FULL GAME",PurchaseFullGame,new Vector2(.22f,.20f),new Vector2(.78f,.39f),Green);
            RefreshShopState(status,purchase);
            overlay.SetActive(false);
            return overlay;
        }

        private void PurchaseFullGame()
        {
            _store.PurchaseFullGame((ok,message)=>
            {
                _shopStatus.text=ok?"FULL GAME UNLOCKED.":$"UNLOCK FAILED: {message}";
                _shopPurchase.interactable=!ok;
                SetButtonLabel(_shopPurchase,ok?"UNLOCKED":"TRY AGAIN");
            });
        }

        private void RefreshShopState()
        {
            RefreshShopState(_shopStatus,_shopPurchase);
        }

        private void RefreshShopState(TMP_Text status,Button purchase)
        {
            bool unlocked=_store.FullGameUnlocked;
            status.text=unlocked?"FULL GAME UNLOCKED.":"Unlock the full game to access the complete campaign and Abyss progression.";
            purchase.interactable=!unlocked;
            SetButtonLabel(purchase,unlocked?"UNLOCKED":"UNLOCK FULL GAME");
        }

        private void Persist(Action mutation)
        {
            mutation();
            _accounts.Save(_account);
        }

        private void Open(GameObject target)
        {
            _inventoryOverlay.SetActive(false);
            _talentsOverlay.SetActive(false);
            _settingsOverlay.SetActive(false);
            _shopOverlay.SetActive(false);
            target.SetActive(true);
        }

        private static string DescribeInventory(RunState run)
        {
            if(run==null)return "No active-run inventory exists for this slot.";
            var lines=new List<string>
            {
                "EQUIPPED WEAPON: "+Humanize(run.EquippedWeaponId),
                "EQUIPPED ARMOR: "+Humanize(run.EquippedArmorId),
                $"RUN GOLD: {run.Gold}",
                $"SMALL KEYS: {run.SmallKeys}    BIG KEYS: {run.BigKeys}",
                "",
                "INVENTORY"
            };
            if(run.InventoryItemIds!=null)foreach(string id in run.InventoryItemIds)lines.Add("• "+Humanize(id));
            if(run.ItemInstances!=null)foreach(ItemInstanceState item in run.ItemInstances)if(item!=null)lines.Add("• "+Humanize(item.BaseItemId));
            if(lines.Count==6)lines.Add("• EMPTY");
            return string.Join("\n",lines);
        }

        private static string DescribeTalents(SlotMetaState meta)
        {
            if(meta==null)return "No mastery data exists for this slot.";
            var lines=new List<string>
            {
                "CLASS: "+Humanize(meta.HeroClassId),
                $"MASTERY {meta.ClassMastery}",
                $"BEST FLOOR {meta.BestFloor}",
                "",
                "UNLOCKED ABILITIES"
            };
            if(meta.UnlockedAbilityIds!=null)foreach(string id in meta.UnlockedAbilityIds)lines.Add("• "+Humanize(id));
            if(lines.Count==5)lines.Add("• NONE YET");
            return string.Join("\n",lines);
        }

        private static string Humanize(string id)
        {
            if(string.IsNullOrWhiteSpace(id))return "NONE";
            int dot=id.LastIndexOf('.');
            string token=dot>=0&&dot<id.Length-1?id.Substring(dot+1):id;
            return token.Replace('_',' ').Replace('-',' ').ToUpperInvariant();
        }

        private static GameObject CreateOverlayRoot(Transform root,string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(root,false);
            RectTransform rt=go.GetComponent<RectTransform>();
            rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
            go.GetComponent<Image>().color=Backdrop;
            return go;
        }

        private static RectTransform CreatePanel(Transform parent,string name,Vector2 min,Vector2 max)
        {
            RectTransform rt=CreateRect(parent,name,min,max);
            var image=rt.gameObject.AddComponent<Image>();image.color=Panel;
            var outline=rt.gameObject.AddComponent<Outline>();outline.effectColor=Gold;outline.effectDistance=new Vector2(2,-2);
            return rt;
        }

        private static TMP_Text CreateLabel(Transform parent,string value,float size,Vector2 min,Vector2 max,TextAlignmentOptions alignment,bool bold)
        {
            return CreateLabel(parent,"Label",value,size,min,max,alignment,bold);
        }

        private static TMP_Text CreateLabel(Transform parent,string name,string value,float size,Vector2 min,Vector2 max,TextAlignmentOptions alignment,bool bold)
        {
            RectTransform rt=CreateRect(parent,name,min,max);
            var text=rt.gameObject.AddComponent<TextMeshProUGUI>();
            text.text=value;text.fontSize=size;text.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;text.alignment=alignment;text.color=bold?Cream:Muted;text.raycastTarget=false;
            text.enableAutoSizing=true;text.fontSizeMax=size;text.fontSizeMin=Mathf.Max(10f,size*.6f);text.overflowMode=TextOverflowModes.Ellipsis;
            return text;
        }

        private static Button CreateButton(Transform parent,string name,string label,UnityEngine.Events.UnityAction action,Vector2 min,Vector2 max,Color fill)
        {
            RectTransform rt=CreateRect(parent,name,min,max);
            var image=rt.gameObject.AddComponent<Image>();image.color=fill;
            var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;if(action!=null)button.onClick.AddListener(action);
            CreateLabel(rt,"Label",label,18,new Vector2(.04f,.08f),new Vector2(.96f,.92f),TextAlignmentOptions.Center,true);
            return button;
        }

        private static void SetButtonLabel(Button button,string value)
        {
            TMP_Text label=button==null?null:button.GetComponentInChildren<TMP_Text>();
            if(label!=null)label.text=value;
        }

        private static void CreateSettingLabel(Transform parent,string label,float minY,float maxY)
        {
            CreateLabel(parent,label,16,new Vector2(.08f,minY),new Vector2(.92f,maxY),TextAlignmentOptions.Left,true);
        }

        private static Slider CreateSlider(Transform parent,string name,float value,float minValue,float maxValue,UnityEngine.Events.UnityAction<float> changed,float minY,float maxY)
        {
            RectTransform rt=CreateRect(parent,name,new Vector2(.08f,minY),new Vector2(.92f,maxY));
            var bg=rt.gameObject.AddComponent<Image>();bg.color=Control;
            var slider=rt.gameObject.AddComponent<Slider>();
            slider.minValue=minValue;slider.maxValue=maxValue;slider.direction=Slider.Direction.LeftToRight;

            RectTransform fill=CreateRect(rt,"Fill",new Vector2(.02f,.25f),new Vector2(.98f,.75f));
            var fillImage=fill.gameObject.AddComponent<Image>();fillImage.color=Gold;
            slider.fillRect=fill;

            RectTransform handle=CreateRect(rt,"Handle",new Vector2(.02f,.08f),new Vector2(.08f,.92f));
            var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=Cream;
            slider.handleRect=handle;slider.targetGraphic=handleImage;
            slider.SetValueWithoutNotify(value);
            if(changed!=null)slider.onValueChanged.AddListener(changed);
            return slider;
        }

        private static Toggle CreateToggle(Transform parent,string name,string label,bool value,UnityEngine.Events.UnityAction<bool> changed,float minY,float maxY)
        {
            RectTransform rt=CreateRect(parent,name,new Vector2(.08f,minY),new Vector2(.92f,maxY));
            var bg=rt.gameObject.AddComponent<Image>();bg.color=Control;
            var toggle=rt.gameObject.AddComponent<Toggle>();toggle.targetGraphic=bg;
            RectTransform mark=CreateRect(rt,"Checkmark",new Vector2(.02f,.17f),new Vector2(.08f,.83f));
            var markImage=mark.gameObject.AddComponent<Image>();markImage.color=Gold;toggle.graphic=markImage;
            CreateLabel(rt,"Label",label,17,new Vector2(.12f,.08f),new Vector2(.96f,.92f),TextAlignmentOptions.Left,true);
            toggle.SetIsOnWithoutNotify(value);
            if(changed!=null)toggle.onValueChanged.AddListener(changed);
            return toggle;
        }

        private static RectTransform CreateRect(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
            RectTransform rt=go.GetComponent<RectTransform>();rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
            return rt;
        }
    }
}
