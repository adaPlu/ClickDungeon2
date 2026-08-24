using NUnit.Framework;
using System.Collections.Generic;
using ClickDungeon.Simulation.Combat;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

public sealed class MonsterBehaviorTests
{
    [Test] public void TrollRegeneratesAfterItsIntent()
    {
        var content=GameContent.CreateDevelopmentFallback();var state=new RunState{Hp=20,MaxHp=20};var troll=new TileState{Index=1,Content=TileContentKind.Monster,ContentId="monster.troll",Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,MonsterHp=4,MonsterMaxHp=7,MonsterAttack=3,IntentKind=MonsterIntentKind.Attack,IntentPower=3};state.Tiles.Add(troll);var events=new List<GameEvent>();MonsterIntentResolver.Resolve(state,troll,content,events);Assert.AreEqual(5,troll.MonsterHp);
    }
}
