using NUnit.Framework;
using System.Linq;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

public sealed class BossGateTests
{
    [Test] public void FloorTenContainsBossAndRequiresBossDefeat()
    {
        var g=new FloorGenerator(); var state=g.CreateNewRun(10,HeroClassId.Knight); g.GenerateFloor(state,10,RouteModifier.Standard);
        Assert.IsTrue(state.BossRequired);
        Assert.IsFalse(state.BossDefeated);
        Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.Boss));
    }
}
