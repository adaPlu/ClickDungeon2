using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ClickDungeon.Application.Persistence;
using ClickDungeon.Application.Services;
using ClickDungeon.Application.State;
using ClickDungeon.Presentation.Menu;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class MainMenuUiContractTests
    {
        private string _tempRoot;
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            _tempRoot=Path.Combine(Path.GetTempPath(),"ClickDungeon-MainMenuUiContractTests",Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
            _host=new GameObject("MainMenuUiContractHost");
        }

        [TearDown]
        public void TearDown()
        {
            if(_host!=null)UnityEngine.Object.DestroyImmediate(_host);
            if(Directory.Exists(_tempRoot))Directory.Delete(_tempRoot,true);
        }

        [Test]
        public void UtilityNavigationActivatesNamedOverlaysAndCloseNavigationHidesThem()
        {
            BuildMenu(new LockedStore());

            AssertOverlayToggle("NavInventory","InventoryOverlay","InventoryOverlayClose");
            AssertOverlayToggle("NavTalents","TalentsOverlay","TalentsOverlayClose");
            AssertOverlayToggle("NavSettings","SettingsOverlay","SettingsOverlayClose");
            AssertOverlayToggle("NavShop","ShopOverlay","ShopOverlayClose");
        }

        [Test]
        public void InventoryAndTalentsProjectSelectedSlotState()
        {
            var run=new RunState
            {
                EquippedWeaponId="item.weapon.iron_sword",
                EquippedArmorId="item.armor.leather"
            };
            run.InventoryItemIds.Add("item.consumable.small_potion");
            run.InventoryItemIds.Add("item.tool.trap_disarm_kit");

            var saves=new LocalSaveRepository(Path.Combine(_tempRoot,"slots"));
            saves.SaveSlot(1,new SlotSavePayload
            {
                Meta=new SlotMetaState
                {
                    HeroClassId="Knight",
                    HeroId="clickington",
                    ClassMastery=7,
                    UnlockedAbilityIds={"ability.knight.shield_bash"},
                    BestFloor=9
                },
                ActiveRun=run
            },1);

            BuildMenu(new LockedStore(),saves:saves);

            RequireButton("NavInventory").onClick.Invoke();
            string inventory=RequireText("InventoryOverlaySummary").text;
            StringAssert.Contains("IRON SWORD",inventory.ToUpperInvariant());
            StringAssert.Contains("LEATHER",inventory.ToUpperInvariant());
            StringAssert.Contains("SMALL POTION",inventory.ToUpperInvariant());

            RequireButton("NavTalents").onClick.Invoke();
            string talents=RequireText("TalentsOverlaySummary").text;
            StringAssert.Contains("MASTERY 7",talents.ToUpperInvariant());
            StringAssert.Contains("SHIELD BASH",talents.ToUpperInvariant());
        }

        [Test]
        public void SettingsControlsPersistThroughAccountRepositoryRoundTrip()
        {
            var accountPath=Path.Combine(_tempRoot,"account.json");
            var accounts=new AccountRepository(accountPath);
            var account=new AccountState
            {
                MasterVolume=.8f,
                MusicVolume=.7f,
                SfxVolume=.6f,
                HapticsEnabled=true,
                ReducedMotion=false,
                TextScale=1f
            };
            accounts.Save(account);

            BuildMenu(new LockedStore(),accounts:accounts,account:account);
            RequireButton("NavSettings").onClick.Invoke();

            RequireSlider("SettingsMasterVolume").value=.42f;
            RequireSlider("SettingsMusicVolume").value=.35f;
            RequireSlider("SettingsSfxVolume").value=.67f;
            RequireToggle("SettingsHaptics").isOn=false;
            RequireToggle("SettingsReducedMotion").isOn=true;
            RequireSlider("SettingsTextScale").value=1.25f;

            AccountState reloaded=new AccountRepository(accountPath).Load();
            Assert.That(reloaded.MasterVolume,Is.EqualTo(.42f).Within(.001f));
            Assert.That(reloaded.MusicVolume,Is.EqualTo(.35f).Within(.001f));
            Assert.That(reloaded.SfxVolume,Is.EqualTo(.67f).Within(.001f));
            Assert.That(reloaded.HapticsEnabled,Is.False);
            Assert.That(reloaded.ReducedMotion,Is.True);
            Assert.That(reloaded.TextScale,Is.EqualTo(1.25f).Within(.001f));
        }

        [Test]
        public void ShopOverlayRetainsStorePurchasePath()
        {
            var store=new LockedStore();
            BuildMenu(store);

            RequireButton("NavShop").onClick.Invoke();
            RequireButton("ShopPurchaseButton").onClick.Invoke();

            Assert.That(store.PurchaseCalled,Is.True);
            StringAssert.Contains("UNLOCKED",RequireText("ShopOverlayStatus").text.ToUpperInvariant());
        }

        private void AssertOverlayToggle(string navButtonName,string overlayName,string closeButtonName)
        {
            GameObject overlay=RequireObject(overlayName);
            Assert.That(overlay.activeSelf,Is.False,$"{overlayName} must start hidden.");

            RequireButton(navButtonName).onClick.Invoke();
            Assert.That(overlay.activeSelf,Is.True,$"{navButtonName} must activate {overlayName}.");

            RequireButton(closeButtonName).onClick.Invoke();
            Assert.That(overlay.activeSelf,Is.False,$"{closeButtonName} must hide {overlayName}.");
        }

        private void BuildMenu(IStoreService store,LocalSaveRepository saves=null,AccountRepository accounts=null,AccountState account=null)
        {
            saves??=new LocalSaveRepository(Path.Combine(_tempRoot,"slots"));
            accounts??=new AccountRepository(Path.Combine(_tempRoot,"account.json"));
            account??=new AccountState();

            var ui=_host.AddComponent<MainMenuUI>();
            SetField(ui,"_saves",saves);
            SetField(ui,"_accounts",accounts);
            SetField(ui,"_account",account);
            SetField(ui,"_services",new ServiceRegistry(store:store));
            SetField(ui,"_content",GameContent.CreateDevelopmentFallback());
            SetField(ui,"_assets",null);
            Invoke(ui,"Build");
        }

        private Button RequireButton(string name)
        {
            var button=RequireObject(name).GetComponent<Button>();
            Assert.That(button,Is.Not.Null,$"{name} must be a Button.");
            return button;
        }

        private Slider RequireSlider(string name)
        {
            var slider=RequireObject(name).GetComponent<Slider>();
            Assert.That(slider,Is.Not.Null,$"{name} must be a Slider.");
            return slider;
        }

        private Toggle RequireToggle(string name)
        {
            var toggle=RequireObject(name).GetComponent<Toggle>();
            Assert.That(toggle,Is.Not.Null,$"{name} must be a Toggle.");
            return toggle;
        }

        private TMP_Text RequireText(string name)
        {
            var text=RequireObject(name).GetComponent<TMP_Text>();
            Assert.That(text,Is.Not.Null,$"{name} must contain TMP text.");
            return text;
        }

        private GameObject RequireObject(string name)
        {
            Transform found=FindDeep(_host.transform,name);
            Assert.That(found,Is.Not.Null,$"Expected runtime object {name}.");
            return found.gameObject;
        }

        private static Transform FindDeep(Transform root,string name)
        {
            if(root.name==name)return root;
            for(int i=0;i<root.childCount;i++)
            {
                Transform found=FindDeep(root.GetChild(i),name);
                if(found!=null)return found;
            }
            return null;
        }

        private static void SetField(object target,string name,object value)
        {
            FieldInfo field=target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(field,Is.Not.Null,$"Expected private field {name}.");
            field.SetValue(target,value);
        }

        private static void Invoke(object target,string name)
        {
            MethodInfo method=target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(method,Is.Not.Null,$"Expected private method {name}.");
            method.Invoke(target,null);
        }

        private sealed class LockedStore : IStoreService
        {
            public bool PurchaseCalled { get; private set; }
            public bool IsSupported=>true;
            public bool FullGameUnlocked { get; private set; }
            public void RefreshEntitlements(Action<bool> completed=null)=>completed?.Invoke(FullGameUnlocked);
            public void PurchaseFullGame(Action<bool,string> completed)
            {
                PurchaseCalled=true;
                FullGameUnlocked=true;
                completed?.Invoke(true,"test_unlock");
            }
        }
    }
}
