using System.Linq;
using NUnit.Framework;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

public sealed class ChestProgressionTests
{
    [Test]
    public void StandardChestRequiresThreeInteractionsBeforeConsumingKeyAndAwardingLoot()
    {
        var generator=new FloorGenerator();
        var state=generator.CreateNewRun(1201,HeroClassId.Knight);
        ClearBoard(state);
        PlacePlayer(state,12);
        ConfigureChest(state.Tiles[13]);
        state.SmallKeys=1;
        var session=new GameSession(state,generator);

        var first=session.Apply(new InteractCommand(13));
        Assert.IsTrue(first.Accepted);
        Assert.AreEqual(1,state.SmallKeys);
        Assert.AreEqual(TileResolution.Available,state.Tiles[13].Resolution);
        Assert.IsTrue(first.Events.Any(e=>e.Type=="chest.opening.progress"&&e.Amount==1));
        Assert.IsFalse(first.Events.Any(e=>e.Type=="chest.opened"));

        var second=session.Apply(new InteractCommand(13));
        Assert.IsTrue(second.Accepted);
        Assert.AreEqual(1,state.SmallKeys);
        Assert.AreEqual(TileResolution.Available,state.Tiles[13].Resolution);
        Assert.IsTrue(second.Events.Any(e=>e.Type=="chest.opening.progress"&&e.Amount==2));
        Assert.IsFalse(second.Events.Any(e=>e.Type=="chest.opened"));

        var third=session.Apply(new InteractCommand(13));
        Assert.IsTrue(third.Accepted);
        Assert.AreEqual(0,state.SmallKeys);
        Assert.AreEqual(TileResolution.Resolved,state.Tiles[13].Resolution);
        Assert.IsTrue(third.Events.Any(e=>e.Type=="chest.opening.progress"&&e.Amount==3));
        Assert.IsTrue(third.Events.Any(e=>e.Type=="chest.opened"));
    }

    private static void ClearBoard(RunState state)
    {
        foreach(var tile in state.Tiles)
        {
            tile.Content=TileContentKind.Empty;
            tile.ContentId="tile.empty";
            tile.Visibility=TileVisibility.Revealed;
            tile.Resolution=TileResolution.Resolved;
            tile.Occupancy=OccupancyKind.None;
            tile.MonsterHp=0;
            tile.MonsterMaxHp=0;
            tile.ThreatPattern=ThreatPattern.None;
            tile.Terrain=TerrainKind.Normal;
            tile.TerrainTriggered=false;
        }
    }

    private static void PlacePlayer(RunState state,int index)
    {
        state.PlayerPosition=new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);
        state.Tiles[index].Occupancy=OccupancyKind.Player;
        state.Tiles[index].Visibility=TileVisibility.Revealed;
        state.Tiles[index].Resolution=TileResolution.Resolved;
    }

    private static void ConfigureChest(TileState tile)
    {
        tile.Content=TileContentKind.Chest;
        tile.ContentId="chest.standard";
        tile.Visibility=TileVisibility.Revealed;
        tile.Resolution=TileResolution.Available;
        tile.Occupancy=OccupancyKind.None;
    }
}
