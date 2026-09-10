using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Presentation.Menu;
using NUnit.Framework;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class HeroPresentationContractTests
    {
        private static readonly string[] HeroIds={"ironheart","clickington","dawnward","rageclaw","gearspark","windsong","lightbringer","emberwisp","shadowcut"};
        private static readonly string[] SelectionOrder={"ironheart","clickington","windsong","shadowcut","emberwisp","lightbringer","rageclaw","gearspark","dawnward"};
        private static readonly string[] Variants={"master","portrait","roster","gameplay","idle","attack","hit","victory","defeat"};

        [TestCase("ironheart")]
        [TestCase("clickington")]
        [TestCase("dawnward")]
        [TestCase("rageclaw")]
        [TestCase("gearspark")]
        [TestCase("windsong")]
        [TestCase("lightbringer")]
        [TestCase("emberwisp")]
        [TestCase("shadowcut")]
        public void EveryHeroUsesIdentityScopedPresentationKeys(string heroId)
        {
            Assert.That(HeroPresentationAssetResolver.MasterAssetId(heroId),Is.EqualTo($"hero.{heroId}.master"));
            Assert.That(HeroPresentationAssetResolver.PortraitAssetId(heroId),Is.EqualTo($"hero.{heroId}.portrait"));
            Assert.That(HeroPresentationAssetResolver.RosterAssetId(heroId),Is.EqualTo($"hero.{heroId}.roster"));
            Assert.That(HeroPresentationAssetResolver.GameplayAssetId(heroId),Is.EqualTo($"hero.{heroId}.gameplay"));
            Assert.That(HeroPresentationAssetResolver.IdleAssetId(heroId),Is.EqualTo($"hero.{heroId}.idle"));
            Assert.That(HeroPresentationAssetResolver.AttackAssetId(heroId),Is.EqualTo($"hero.{heroId}.attack"));
            Assert.That(HeroPresentationAssetResolver.HitAssetId(heroId),Is.EqualTo($"hero.{heroId}.hit"));
            Assert.That(HeroPresentationAssetResolver.VictoryAssetId(heroId),Is.EqualTo($"hero.{heroId}.victory"));
            Assert.That(HeroPresentationAssetResolver.DefeatAssetId(heroId),Is.EqualTo($"hero.{heroId}.defeat"));
        }

        [Test]
        public void RuntimeFilenameMapperSupportsAllNineRequiredVariantsForAllNineHeroes()
        {
            foreach(string heroId in HeroIds)
                foreach(string variant in Variants)
                    Assert.That(PresentationAssetIdMapper.SpriteId($"hero_{heroId}_{variant}"),Is.EqualTo($"hero.{heroId}.{variant}"));
        }

        [Test]
        public void HeroRuntimePresentationDoesNotFallBackToMechanicalClassArt()
        {
            string source=SourceFile("Presentation","Assets","HeroPresentationAssets.cs");
            Assert.That(source,Does.Not.Contain("LegacyClassBaseId"));
            Assert.That(source,Does.Not.Contain("fallbackClass"),"Identity-specific runtime art must never silently borrow another hero's class art.");
        }

        [Test]
        public void HeroSelectionUsesApprovedNineHeroOrder()
        {
            PropertyInfo property=typeof(HeroCardPresentation).GetProperty("SelectionOrder",BindingFlags.Public|BindingFlags.Static);
            Assert.That(property,Is.Not.Null,"HeroCardPresentation must expose the approved selection order.");
            var enumerable=property?.GetValue(null) as IEnumerable;
            Assert.That(enumerable,Is.Not.Null);
            string[] actual=enumerable.Cast<object>().Select(value=>value?.ToString()??string.Empty).ToArray();
            Assert.That(actual,Is.EqualTo(SelectionOrder));
        }

        [Test]
        public void HeroSelectionDescriptorExposesCanonicalGameplayStats()
        {
            string[] required={"BaseHp","BaseAttack","BaseDefense","GameplayIdentity","BoardPassive","AbilityIds"};
            foreach(string propertyName in required)
                Assert.That(typeof(HeroCardDescriptor).GetProperty(propertyName,BindingFlags.Public|BindingFlags.Instance),Is.Not.Null,$"Missing canonical selector field {propertyName}.");
        }

        [Test]
        public void MainMenuUsesSingleHeroPageWithExplicitSelectAndNextActions()
        {
            string source=SourceFile("Presentation","Menu","MainMenuUI.cs");
            Assert.That(source,Does.Contain("HeroSelectSelect"));
            Assert.That(source,Does.Contain("HeroSelectNext"));
            Assert.That(source,Does.Contain("CORE STATS"));
            Assert.That(source,Does.Contain("CLASS KIT"));
            Assert.That(source,Does.Contain("GAMEPLAY IDENTITY"));
            Assert.That(source,Does.Not.Contain("CreateButton(\"PreviousClass\""));
            Assert.That(source,Does.Not.Contain("CreateButton(\"NextClass\""));
            Assert.That(source,Does.Not.Contain("CreateButton(\"PreviousHero\""));
            Assert.That(source,Does.Not.Contain("CreateButton(\"NextHero\""));
            Assert.That(source,Does.Not.Contain("button.onClick.AddListener(()=>StartNew(card.HeroId))"),"Hero page itself must not start a run; SELECT is the explicit commit action.");
        }

        private static string SourceFile(params string[] relative)
        {
            string root=Directory.GetCurrentDirectory();
            while(!File.Exists(Path.Combine(root,"ProjectSettings","ProjectVersion.txt"))&&Directory.GetParent(root)!=null)root=Directory.GetParent(root).FullName;
            string path=Path.Combine(root,"Assets","ClickDungeon",Path.Combine(relative));
            Assert.That(File.Exists(path),Is.True);
            return File.ReadAllText(path);
        }
    }
}
