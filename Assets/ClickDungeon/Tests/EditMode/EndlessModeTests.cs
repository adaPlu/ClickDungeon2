using System.Linq;
using NUnit.Framework;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class EndlessModeTests
    {
        [Test]
        public void AbyssBeginsAfterCampaignAndCyclesBiome()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var state=new FloorGenerator(content).CreateAbyssRun(42,HeroClassId.Knight);
            Assert.AreEqual(RunMode.Abyss,state.Mode);
            Assert.AreEqual(1,state.AbyssDepth);
            Assert.AreEqual(51,state.Floor);
            Assert.AreEqual("biome.cavern",state.BiomeId);
        }

        [Test]
        public void AbyssDepthTenContainsBoss()
        {
            var content=GameContent.CreateDevelopmentFallback();var gen=new FloorGenerator(content);
            var state=gen.CreateAbyssRun(42,HeroClassId.Knight);state.AbyssDepth=10;gen.GenerateFloor(state,60,RouteModifier.Standard);
            Assert.IsTrue(state.BossRequired);
            Assert.IsTrue(state.Tiles.Any(t=>t.Content==TileContentKind.Boss));
        }

        [Test]
        public void AbyssExitAdvancesDepthInsteadOfCompletingCampaign()
        {
            var content=GameContent.CreateDevelopmentFallback();var gen=new FloorGenerator(content);var state=gen.CreateAbyssRun(7,HeroClassId.Knight);
            var exit=state.Tiles.First(t=>t.Content==TileContentKind.SafeExit);exit.Visibility=TileVisibility.Revealed;
            var exitPosition=new GridPosition(exit.Index/RunState.BoardSize,exit.Index%RunState.BoardSize);
            if(exitPosition.Col>0)state.PlayerPosition=new GridPosition(exitPosition.Row,exitPosition.Col-1);
            else state.PlayerPosition=new GridPosition(exitPosition.Row,exitPosition.Col+1);
            var session=new GameSession(state,gen,content);var result=session.Apply(new TakeSafeExitCommand(exit.Index));
            Assert.IsTrue(result.Accepted);Assert.AreEqual(2,state.AbyssDepth);Assert.IsFalse(state.CampaignCompleted);
        }
    }
}
