using NUnit.Framework;
using System.Collections.Generic;
using ClickDungeon.Simulation.Boss;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

public sealed class BossPhaseTests
{
    [Test] public void LichChangesBoardAtHalfHealth()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(777,HeroClassId.Knight);g.GenerateFloor(state,10,RouteModifier.Standard);var boss=state.Tiles.Find(t=>t.Content==TileContentKind.Boss);boss.Visibility=TileVisibility.Revealed;boss.MonsterHp=boss.MonsterMaxHp/2;var events=new List<GameEvent>();BossResolver.AfterDamage(state,boss,events);Assert.AreEqual(2,boss.BossPhase);Assert.IsTrue(events.Exists(e=>e.Type=="boss.phase_changed"));
    }
}
