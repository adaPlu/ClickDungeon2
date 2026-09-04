using System;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using ClickDungeon.Application.State;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.ApplicationEditMode
{
    public sealed class HeroIdentityTests
    {
        private const string CatalogTypeName = "ClickDungeon.Application.Heroes.HeroIdentityCatalog";

        [Test]
        public void HeroIdentityStorageIsOptionalAndLegacyChecksumSafe()
        {
            var heroIdField = typeof(SlotMetaState).GetField("HeroId", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(heroIdField, "SlotMetaState must expose an optional HeroId without replacing HeroClassId.");

            var legacyCompatibleMeta = new SlotMetaState();
            string legacyCompatibleJson = JsonConvert.SerializeObject(legacyCompatibleMeta, Formatting.None);
            StringAssert.DoesNotContain("\"HeroId\"", legacyCompatibleJson,
                "An unset HeroId must be omitted so existing schema-2 save checksums remain valid.");

            heroIdField.SetValue(legacyCompatibleMeta, "clickington");
            string clickingtonJson = JsonConvert.SerializeObject(legacyCompatibleMeta, Formatting.None);
            StringAssert.Contains("\"HeroId\":\"clickington\"", clickingtonJson,
                "New saves must persist the selected hero identity when it differs from the legacy class-only model.");
        }

        [Test]
        public void IronheartAndClickingtonShareKnightMechanicsButNotCampaignIdentity()
        {
            Type catalog = typeof(SlotMetaState).Assembly.GetType(CatalogTypeName);
            Assert.NotNull(catalog, "HeroIdentityCatalog must live in the application layer, outside deterministic simulation state.");

            MethodInfo resolveHeroId = catalog.GetMethod("ResolveHeroId", BindingFlags.Public | BindingFlags.Static);
            MethodInfo classForHero = catalog.GetMethod("ClassForHero", BindingFlags.Public | BindingFlags.Static);
            MethodInfo displayNameForHero = catalog.GetMethod("DisplayNameForHero", BindingFlags.Public | BindingFlags.Static);
            MethodInfo campaignForHero = catalog.GetMethod("CampaignForHero", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(resolveHeroId);
            Assert.NotNull(classForHero);
            Assert.NotNull(displayNameForHero);
            Assert.NotNull(campaignForHero);

            Assert.AreEqual("ironheart", resolveHeroId.Invoke(null, new object[] { HeroClassId.Knight, null }));
            Assert.AreEqual("clickington", resolveHeroId.Invoke(null, new object[] { HeroClassId.Knight, "clickington" }));
            Assert.AreEqual("ironheart", resolveHeroId.Invoke(null, new object[] { HeroClassId.Knight, "windsong" }),
                "A hero identity that belongs to another class must not silently change Knight mechanics.");

            Assert.AreEqual(HeroClassId.Knight, (HeroClassId)classForHero.Invoke(null, new object[] { "ironheart" }));
            Assert.AreEqual(HeroClassId.Knight, (HeroClassId)classForHero.Invoke(null, new object[] { "clickington" }));
            Assert.AreEqual("Ironheart", displayNameForHero.Invoke(null, new object[] { "ironheart" }));
            Assert.AreEqual("Sir Clickington", displayNameForHero.Invoke(null, new object[] { "clickington" }));
            Assert.AreEqual(string.Empty, campaignForHero.Invoke(null, new object[] { "ironheart" }));
            Assert.AreEqual("clickington_campaign", campaignForHero.Invoke(null, new object[] { "clickington" }));
        }

        [Test]
        public void SelectionLabelsExposeHeroIdentityAndClickingtonStoryStatusWithoutCreatingANinthClass()
        {
            Type catalog = typeof(SlotMetaState).Assembly.GetType(CatalogTypeName);
            Assert.NotNull(catalog);
            MethodInfo selectionLabel = catalog.GetMethod("SelectionLabelForHero", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(selectionLabel, "Menu-facing hero labels must be generated from the identity catalog rather than UI special cases.");

            Assert.AreEqual("Ironheart — Knight", selectionLabel.Invoke(null, new object[] { "ironheart" }));
            Assert.AreEqual("Sir Clickington — Knight • Story Campaign", selectionLabel.Invoke(null, new object[] { "clickington" }));
            Assert.AreEqual("Windsong — Ranger", selectionLabel.Invoke(null, new object[] { "windsong" }));
            Assert.AreEqual("Shadowcut — Thief", selectionLabel.Invoke(null, new object[] { "shadowcut" }));
            Assert.AreEqual("Emberwisp — Wizard", selectionLabel.Invoke(null, new object[] { "emberwisp" }));
        }

        [Test]
        public void HeroVisualAssetKeysAreDeterministicAndRejectUnknownInputs()
        {
            Type catalog = typeof(SlotMetaState).Assembly.GetType(CatalogTypeName);
            Assert.NotNull(catalog);
            MethodInfo assetKey = catalog.GetMethod("VisualAssetKeyForHero", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(assetKey, "Hero-derived filenames and runtime lookups must share one tested asset-key contract.");

            Assert.AreEqual("hero.ironheart.portrait", assetKey.Invoke(null, new object[] { "ironheart", "portrait" }));
            Assert.AreEqual("hero.ironheart.attack", assetKey.Invoke(null, new object[] { "ironheart", "attack" }));
            Assert.AreEqual("hero.clickington.roster", assetKey.Invoke(null, new object[] { "clickington", "roster" }));
            Assert.AreEqual("hero.clickington.defeat", assetKey.Invoke(null, new object[] { "clickington", "defeat" }));

            var unknownHero = Assert.Throws<TargetInvocationException>(()=>assetKey.Invoke(null,new object[]{"not-a-hero","portrait"}));
            Assert.IsInstanceOf<ArgumentException>(unknownHero.InnerException);
            var unknownVariant = Assert.Throws<TargetInvocationException>(()=>assetKey.Invoke(null,new object[]{"ironheart","sparkle-wallpaper"}));
            Assert.IsInstanceOf<ArgumentException>(unknownVariant.InnerException);
        }
    }
}
