using System.Linq;
using NUnit.Framework;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class GenerationFuzzTests
    {
        [Test]
        public void FiveHundredSeedsAcrossCampaignRespectStructuralInvariants()
        {
            var content=GameContent.CreateDevelopmentFallback();var gen=new FloorGenerator(content);
            for(uint seed=1;seed<=500;seed++)
            {
                var state=gen.CreateNewRun(seed,HeroClassId.Knight);
                foreach(int floor in new[]{1,5,10,20,30,40,50})
                {
                    gen.GenerateFloor(state,floor,seed%3==0?RouteModifier.Forbidden:RouteModifier.Standard);
                    Assert.AreEqual(25,state.Tiles.Count,$"seed={seed} floor={floor}");
                    Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.SafeExit),$"safe exit seed={seed} floor={floor}");
                    Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.ForbiddenExit),$"forbidden exit seed={seed} floor={floor}");
                    Assert.AreEqual(1,state.Tiles.Count(t=>t.Occupancy==OccupancyKind.Player),$"player occupancy seed={seed} floor={floor}");
                    if(state.BossRequired)Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.Boss),$"boss seed={seed} floor={floor}");
                }
            }
        }
    }
}
