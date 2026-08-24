using NUnit.Framework;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class VariantTests
    {
        [Test]
        public void RepeatedGenerationWithSameSeedPreservesVariantAssignments()
        {
            var content=GameContent.CreateDevelopmentFallback();
            content.Add(new MonsterVariantDefinition{Id="variant.frost",HpMultiplierBasisPoints=11000,AttackMultiplierBasisPoints=10000,BehaviorAdd="intent_delay"});
            var generator=new FloorGenerator(content);
            var a=generator.CreateNewRun(991,HeroClassId.Knight); generator.GenerateFloor(a,26,RouteModifier.Standard);
            var b=generator.CreateNewRun(991,HeroClassId.Knight); generator.GenerateFloor(b,26,RouteModifier.Standard);
            for(int i=0;i<a.Tiles.Count;i++) Assert.AreEqual(a.Tiles[i].VariantId,b.Tiles[i].VariantId);
        }
    }
}
