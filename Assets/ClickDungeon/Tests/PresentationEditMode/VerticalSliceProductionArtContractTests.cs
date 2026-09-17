using System.Linq;
using NUnit.Framework;
using ClickDungeon.Presentation.Assets;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class VerticalSliceProductionArtContractTests
    {
        [Test]
        public void ActorIds_AreExactlyApprovedCoreSlice()
        {
            CollectionAssert.AreEquivalent(new[]
            {
                "hero.clickington",
                "monster.goblin_raider",
                "monster.slime",
                "monster.mimic_chest",
                "boss.lord_blobert"
            }, VerticalSliceProductionArtContract.ActorIds);
        }

        [Test]
        public void RequiredSequences_AreExactHybridMinimum()
        {
            var pairs = VerticalSliceProductionArtContract.RequiredSequences
                .Select(x => $"{x.Id}:{x.FrameCount}:{x.FramesPerSecond}").ToArray();

            CollectionAssert.AreEqual(new[]
            {
                "hero.clickington.attack:5:12",
                "monster.mimic_chest.reveal:6:12",
                "monster.mimic_chest.bite:5:12",
                "boss.lord_blobert.attack:6:12"
            }, pairs);
        }

        [TestCase("monster.fire_imp")]
        [TestCase("monster.cave_spider")]
        [TestCase("monster.crowned_slime")]
        [TestCase("boss.goblin_brute_king")]
        public void Contract_DoesNotReintroduceLaterScope(string id)
        {
            Assert.That(VerticalSliceProductionArtContract.ActorIds, Does.Not.Contain(id));
        }
    }
}
