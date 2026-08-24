using NUnit.Framework;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;
using System.Linq;

public sealed class FloorGeneratorTests
{
    [Test] public void FloorHasExactly25TilesAndTwoExitOptions()
    {
        var state=new FloorGenerator().CreateNewRun(42,HeroClassId.Knight);
        Assert.AreEqual(25,state.Tiles.Count);
        Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.SafeExit));
        Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.ForbiddenExit));
    }
    [Test] public void SameSeedProducesSameContentOrder()
    {
        var g=new FloorGenerator(); var a=g.CreateNewRun(9876,HeroClassId.Knight); var b=g.CreateNewRun(9876,HeroClassId.Knight);
        CollectionAssert.AreEqual(a.Tiles.Select(t=>t.ContentId).ToArray(),b.Tiles.Select(t=>t.ContentId).ToArray());
    }
}
