using NUnit.Framework;
using System.Collections.Generic;
using ClickDungeon.Simulation.Biome;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

public sealed class TerrainTests
{
    [Test] public void ThornTerrainDamagesOnlyOnFirstEntry()
    {
        var generator=new FloorGenerator();var state=generator.CreateNewRun(88,HeroClassId.Knight);var tile=state.Tiles[13];tile.Terrain=TerrainKind.Thorn;int hp=state.Hp;var events=new List<GameEvent>();TerrainResolver.ResolveEntry(state,12,13,generator.Content,events);TerrainResolver.ResolveEntry(state,12,13,generator.Content,events);Assert.AreEqual(hp-1,state.Hp);
    }
}
