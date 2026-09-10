using System.Linq;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;
using NUnit.Framework;

public sealed class SpecialDungeonTileTests
{
    [Test]
    public void HealingFountainHealsOnceAndCapsAtMaxHp()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(2101,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        state.Hp=8;state.MaxHp=12;ConfigureSpecial(state.Tiles[13],"special.fountain.heal",5);
        var session=new GameSession(state,g);

        var first=session.Apply(new InteractCommand(13));
        Assert.IsTrue(first.Accepted);
        Assert.AreEqual(12,state.Hp);
        Assert.AreEqual(TileResolution.Resolved,state.Tiles[13].Resolution);
        Assert.IsTrue(first.Events.Any(e=>e.Type=="fountain.healed"&&e.Amount==4));

        var second=session.Apply(new InteractCommand(13));
        Assert.IsFalse(second.Accepted);
        Assert.AreEqual(12,state.Hp);
    }

    [Test]
    public void TeleportUsesStoredDestinationInsteadOfRuntimeRandomness()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(2102,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        ConfigureSpecial(state.Tiles[13],"special.teleport",0);state.Tiles[13].TeleportDestinationIndex=18;
        ConfigureSpecial(state.Tiles[18],"special.teleport",0);state.Tiles[18].TeleportDestinationIndex=13;
        var result=new GameSession(state,g).Apply(new InteractCommand(13));

        Assert.IsTrue(result.Accepted);
        Assert.AreEqual(new GridPosition(3,3),state.PlayerPosition);
        Assert.AreEqual(OccupancyKind.Player,state.Tiles[18].Occupancy);
        Assert.AreEqual(OccupancyKind.None,state.Tiles[12].Occupancy);
        Assert.AreEqual(TileResolution.Available,state.Tiles[13].Resolution,"Teleport tiles are reusable; pairing is deterministic state, not a one-shot roll.");
        Assert.IsTrue(result.Events.Any(e=>e.Type=="teleport.used"&&e.Amount==18));
    }

    [Test]
    public void PressurePlateUnlocksOnlyItsStoredLinkedTargetAndResolvesOnce()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(2103,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        ConfigureSpecial(state.Tiles[13],"special.pressure_plate",0);state.Tiles[13].LinkedTileIndex=14;
        ConfigureDisabledExit(state.Tiles[14]);ConfigureDisabledExit(state.Tiles[19]);
        var result=new GameSession(state,g).Apply(new InteractCommand(13));

        Assert.IsTrue(result.Accepted);
        Assert.AreEqual(TileResolution.Resolved,state.Tiles[13].Resolution);
        Assert.AreEqual(TileResolution.Available,state.Tiles[14].Resolution);
        Assert.AreEqual(TileResolution.Disabled,state.Tiles[19].Resolution);
        Assert.IsTrue(result.Events.Any(e=>e.Type=="pressure_plate.activated"&&e.Amount==14));
    }

    [TestCase("trap.bomb",4)]
    [TestCase("trap.spike",2)]
    public void NewTrapTilesUseTheExistingDeterministicRevealDamagePipeline(string trapId,int damage)
    {
        var content=GameContent.CreateDevelopmentFallback();
        content.Add(new TrapDefinition{Id=trapId,Damage=damage,StatusId="",StatusDuration=0,MinFloor=1});
        var g=new FloorGenerator(content);var state=g.CreateNewRun(2104,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        ConfigureTrap(state.Tiles[13],trapId);int hpBefore=state.Hp;
        var result=new GameSession(state,g,content).Apply(new RevealTileCommand(13));

        Assert.IsTrue(result.Accepted);
        Assert.AreEqual(hpBefore-damage,state.Hp);
        Assert.AreEqual(TileResolution.Resolved,state.Tiles[13].Resolution);
        Assert.IsTrue(result.Events.Any(e=>e.Type=="trap.triggered"&&e.Id==trapId&&e.Amount==damage));
    }

    [Test]
    public void InvalidTeleportDestinationIsRejectedWithoutMovingPlayer()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(2105,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        ConfigureSpecial(state.Tiles[13],"special.teleport",0);state.Tiles[13].TeleportDestinationIndex=99;
        var result=new GameSession(state,g).Apply(new InteractCommand(13));
        Assert.IsFalse(result.Accepted);
        Assert.AreEqual("teleport.destination_invalid",result.RejectionReason);
        Assert.AreEqual(new GridPosition(2,2),state.PlayerPosition);
    }

    private static void ClearBoard(RunState state)
    {
        foreach(var tile in state.Tiles)
        {
            tile.Content=TileContentKind.Empty;tile.ContentId="tile.empty";tile.Visibility=TileVisibility.Revealed;
            tile.Resolution=TileResolution.Resolved;tile.Occupancy=OccupancyKind.None;tile.MonsterHp=0;tile.MonsterMaxHp=0;
            tile.ThreatPattern=ThreatPattern.None;tile.Terrain=TerrainKind.Normal;tile.TerrainTriggered=false;
            tile.LinkedTileIndex=-1;tile.TeleportDestinationIndex=-1;
        }
    }

    private static void PlacePlayer(RunState state,int index)
    {
        state.PlayerPosition=new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);
        state.Tiles[index].Occupancy=OccupancyKind.Player;state.Tiles[index].Visibility=TileVisibility.Revealed;
        state.Tiles[index].Resolution=TileResolution.Resolved;
    }

    private static void ConfigureSpecial(TileState tile,string id,int amount)
    {
        tile.Content=TileContentKind.SpecialEvent;tile.ContentId=id;tile.Amount=amount;tile.Visibility=TileVisibility.Revealed;
        tile.Resolution=TileResolution.Available;tile.Occupancy=OccupancyKind.Object;
    }

    private static void ConfigureDisabledExit(TileState tile)
    {
        tile.Content=TileContentKind.SafeExit;tile.ContentId="exit.safe";tile.Visibility=TileVisibility.Revealed;
        tile.Resolution=TileResolution.Disabled;tile.Occupancy=OccupancyKind.Object;
    }

    private static void ConfigureTrap(TileState tile,string id)
    {
        tile.Content=TileContentKind.Trap;tile.ContentId=id;tile.Visibility=TileVisibility.Hidden;
        tile.Resolution=TileResolution.Available;tile.Occupancy=OccupancyKind.Hazard;
    }
}
