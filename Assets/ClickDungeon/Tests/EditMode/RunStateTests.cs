using NUnit.Framework;
using System.Linq;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

public sealed class RunStateTests
{
    [Test] public void NewRunStartsWithOnlySignatureAbility()
    {
        var state=new FloorGenerator().CreateNewRun(55,HeroClassId.Knight);
        Assert.AreEqual(1,state.AbilityStates.Count);
        Assert.AreEqual("ability.knight.shield_wall",state.AbilityStates[0].AbilityId);
    }

    [Test] public void CenterIsAlwaysSafeResolvedPlayerTile()
    {
        var state=new FloorGenerator().CreateNewRun(56,HeroClassId.Thief);
        var tile=state.Tiles[12];
        Assert.AreEqual(TileContentKind.Empty,tile.Content);
        Assert.AreEqual(TileVisibility.Revealed,tile.Visibility);
        Assert.AreEqual(TileResolution.Resolved,tile.Resolution);
        Assert.AreEqual(OccupancyKind.Player,tile.Occupancy);
    }
}
