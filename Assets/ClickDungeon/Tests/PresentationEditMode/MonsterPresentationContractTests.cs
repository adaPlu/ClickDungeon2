using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation.Model;
using NUnit.Framework;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class MonsterPresentationContractTests
    {
        [TestCase("monster.skeleton",TileContentKind.Monster,"monster.skeleton")]
        [TestCase("boss.lich_sovereign",TileContentKind.Boss,"boss.lich_sovereign")]
        public void RevealedActiveActorsUseCanonicalContentAssetIds(string contentId,TileContentKind kind,string expected)
        {
            var tile=new TileState
            {
                Visibility=TileVisibility.Revealed,
                Resolution=TileResolution.Available,
                Content=kind,
                ContentId=contentId,
                MonsterHp=5,
                MonsterMaxHp=5
            };

            Assert.That(TilePresentationAssetResolver.PrimaryAssetId(tile),Is.EqualTo(expected));
        }

        [TestCase("skeleton",TileContentKind.Monster)]
        [TestCase("boss.skeleton",TileContentKind.Monster)]
        [TestCase("monster.skeleton",TileContentKind.Boss)]
        [TestCase("",TileContentKind.Monster)]
        public void ActorResolverRejectsNonCanonicalOrMismatchedContentIds(string contentId,TileContentKind kind)
        {
            var tile=new TileState
            {
                Visibility=TileVisibility.Revealed,
                Resolution=TileResolution.Available,
                Content=kind,
                ContentId=contentId,
                MonsterHp=5,
                MonsterMaxHp=5
            };

            Assert.That(TilePresentationAssetResolver.PrimaryAssetId(tile),Is.EqualTo(string.Empty));
        }

        [Test]
        public void DefeatedActorLeavesTheActiveContentLayer()
        {
            var tile=new TileState
            {
                Visibility=TileVisibility.Revealed,
                Resolution=TileResolution.Resolved,
                Content=TileContentKind.Monster,
                ContentId="monster.skeleton",
                MonsterHp=0,
                MonsterMaxHp=5
            };

            Assert.That(TilePresentationAssetResolver.PrimaryAssetId(tile),Is.EqualTo(string.Empty));
        }
    }
}
