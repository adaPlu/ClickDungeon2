using System;
using System.IO;
using ClickDungeon.Presentation.Assets;
using NUnit.Framework;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class HeroPresentationContractTests
    {
        private static readonly string[] HeroIds={"ironheart","clickington","dawnward","rageclaw","gearspark","windsong","lightbringer","emberwisp","shadowcut"};
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
