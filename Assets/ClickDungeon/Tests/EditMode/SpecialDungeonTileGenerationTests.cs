using System.Collections.Generic;
using System.Linq;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;
using NUnit.Framework;

public sealed class SpecialDungeonTileGenerationTests
{
    [Test]
    public void SameSeedProducesSameSpecialTilePairing()
    {
        var a=new FloorGenerator().CreateNewRun(12345,HeroClassId.Knight);
        var b=new FloorGenerator().CreateNewRun(12345,HeroClassId.Knight);
        Assert.AreEqual(SnapshotSpecialTiles(a),SnapshotSpecialTiles(b));
        Assert.Greater(a.Tiles.Count(t=>t.Content==TileContentKind.SpecialEvent),0,"Each generated floor should expose one deterministic special feature.");
    }

    [Test]
    public void FixedSeedSweepExposesAllThreeApprovedSpecialFeatureFamilies()
    {
        var seen=new HashSet<string>();
        for(uint seed=1;seed<=96;seed++)
        {
            var state=new FloorGenerator().CreateNewRun(seed,HeroClassId.Knight);
            foreach(var tile in state.Tiles.Where(t=>t.Content==TileContentKind.SpecialEvent))seen.Add(tile.ContentId);
        }
        Assert.That(seen,Does.Contain("special.fountain.heal"));
        Assert.That(seen,Does.Contain("special.teleport"));
        Assert.That(seen,Does.Contain("special.pressure_plate"));
    }

    [Test]
    public void GeneratedSpecialTilesPreserveMandatoryStructureAndValidLinks()
    {
        for(uint seed=1;seed<=128;seed++)
        {
            var state=new FloorGenerator().CreateNewRun(seed,HeroClassId.Knight);
            Assert.AreEqual(25,state.Tiles.Count);
            Assert.AreEqual(TileContentKind.Empty,state.Tiles[12].Content);
            Assert.AreEqual(OccupancyKind.Player,state.Tiles[12].Occupancy);
            Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.SafeExit));
            Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.ForbiddenExit));

            foreach(var tile in state.Tiles.Where(t=>t.Content==TileContentKind.SpecialEvent))
            {
                Assert.AreNotEqual(12,tile.Index);
                if(tile.ContentId=="special.teleport")
                {
                    Assert.That(tile.TeleportDestinationIndex,Is.InRange(0,24));
                    Assert.AreNotEqual(tile.Index,tile.TeleportDestinationIndex);
                    var peer=state.Tiles[tile.TeleportDestinationIndex];
                    Assert.AreEqual("special.teleport",peer.ContentId);
                    Assert.AreEqual(tile.Index,peer.TeleportDestinationIndex,"Teleport pairs must be symmetric stored state.");
                }
                else if(tile.ContentId=="special.pressure_plate")
                {
                    Assert.That(tile.LinkedTileIndex,Is.InRange(0,24));
                    var target=state.Tiles[tile.LinkedTileIndex];
                    Assert.That(target.Content==TileContentKind.SafeExit||target.Content==TileContentKind.ForbiddenExit,Is.True);
                    Assert.AreEqual(TileResolution.Disabled,target.Resolution,"Generated linked route must actually begin locked.");
                }
                else if(tile.ContentId=="special.fountain.heal")
                {
                    Assert.Greater(tile.Amount,0);
                }
                else Assert.Fail("Unexpected generated special event: "+tile.ContentId);
            }
        }
    }

    [Test]
    public void BossFloorsKeepBossAndSpecialFeatureSeparate()
    {
        var generator=new FloorGenerator();
        var state=generator.CreateNewRun(8080,HeroClassId.Knight);
        generator.GenerateFloor(state,10,RouteModifier.Standard);
        Assert.AreEqual(1,state.Tiles.Count(t=>t.Content==TileContentKind.Boss));
        Assert.Greater(state.Tiles.Count(t=>t.Content==TileContentKind.SpecialEvent),0);
        Assert.IsFalse(state.Tiles.Any(t=>t.Index==12&&t.Content==TileContentKind.SpecialEvent));
    }

    private static string SnapshotSpecialTiles(RunState state)
    {
        return string.Join("|",state.Tiles
            .Where(t=>t.Content==TileContentKind.SpecialEvent)
            .OrderBy(t=>t.Index)
            .Select(t=>$"{t.Index}:{t.ContentId}:{t.LinkedTileIndex}:{t.TeleportDestinationIndex}:{t.Amount}:{t.Resolution}"));
    }
}
