using NUnit.Framework;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Loot;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class LootResolverTests
    {
        [Test]
        public void SameSeedAndSourceProduceSameLoot()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var generator=new FloorGenerator(content);
            var a=generator.CreateNewRun(1234,HeroClassId.Knight);
            var b=generator.CreateNewRun(1234,HeroClassId.Knight);
            var resolver=new LootResolver(content);
            var x=resolver.Roll(a,"loot.chest.standard",4);
            var y=resolver.Roll(b,"loot.chest.standard",4);
            Assert.AreEqual(x.Gold,y.Gold);
            Assert.AreEqual(x.Item?.BaseItemId,y.Item?.BaseItemId);
            Assert.AreEqual(x.Item?.AffixId,y.Item?.AffixId);
        }

        [Test]
        public void VaultConsumesDeterministicTableWithEquipmentOrGold()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var state=new FloorGenerator(content).CreateNewRun(9,HeroClassId.Knight);
            var reward=new LootResolver(content).Roll(state,"loot.vault.sealed",7);
            Assert.IsTrue(reward.Gold>0 || reward.Item!=null);
        }
    }
}
