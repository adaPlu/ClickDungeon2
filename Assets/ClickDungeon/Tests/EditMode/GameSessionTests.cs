using NUnit.Framework;
using System.Linq;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

public sealed class GameSessionTests
{
    [Test] public void NonAdjacentRevealIsRejected()
    {
        var g=new FloorGenerator(); var state=g.CreateNewRun(1,HeroClassId.Knight); var session=new GameSession(state,g);
        Assert.IsFalse(session.Apply(new RevealTileCommand(0)).Accepted);
    }
    [Test] public void InvalidNoProgressActionDoesNotAdvanceCommandNumber()
    {
        var g=new FloorGenerator(); var state=g.CreateNewRun(2,HeroClassId.Knight); var session=new GameSession(state,g); long before=state.CommandNumber;
        session.Apply(new MoveCommand(0));
        Assert.AreEqual(before,state.CommandNumber);
    }

    [Test] public void NewlyAppliedPoisonDoesNotTickOnTheApplyingCommand()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(3,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        ConfigureMonster(state.Tiles[13],"monster.slime",10,MonsterIntentKind.ApplyPoison,3);
        int hpBefore=state.Hp;var session=new GameSession(state,g);
        var result=session.Apply(new AttackCommand(13));
        Assert.IsTrue(result.Accepted);
        Assert.IsTrue(state.Statuses.Exists(s=>s.StatusId=="status.poison"&&s.RemainingActions==3));
        Assert.IsFalse(result.Events.Any(e=>e.Type=="status.damage.tick"));
        Assert.AreEqual(hpBefore-1,state.Hp);
    }

    [Test] public void RefreshedPoisonTicksOnlyThePreCommandStack()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(9,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        ConfigureMonster(state.Tiles[13],"monster.slime",10,MonsterIntentKind.ApplyPoison,3);
        state.Statuses.Add(new StatusInstance{StatusId="status.poison",RemainingActions=1,Stacks=1});
        int hpBefore=state.Hp;var result=new GameSession(state,g).Apply(new AttackCommand(13));
        Assert.IsTrue(result.Accepted);
        Assert.IsTrue(result.Events.Any(e=>e.Type=="status.damage.tick"&&e.Amount==1));
        Assert.AreEqual(hpBefore-2,state.Hp);
        Assert.IsTrue(state.Statuses.Exists(s=>s.StatusId=="status.poison"&&s.RemainingActions==3&&s.Stacks==2));
    }

    [Test] public void CamouflageIsConsumedWhenMovementBypassesThreat()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(4,HeroClassId.Ranger);ClearBoard(state);PlacePlayer(state,12);
        ConfigureEmpty(state.Tiles[13]);ConfigureEmpty(state.Tiles[18]);
        ConfigureMonster(state.Tiles[14],"monster.rat",3,MonsterIntentKind.Attack,2);
        ConfigureMonster(state.Tiles[23],"monster.rat",3,MonsterIntentKind.Attack,2);
        state.CamouflageActions=1;var session=new GameSession(state,g);
        Assert.IsTrue(session.Apply(new MoveCommand(13)).Accepted);
        Assert.AreEqual(0,state.CamouflageActions);
        var second=session.Apply(new MoveCommand(18));
        Assert.IsFalse(second.Accepted);
        Assert.AreEqual("tile.threatened",second.RejectionReason);
    }

    [Test] public void RootedMovementConsumesAnAcceptedBlockedAction()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(5,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);ConfigureEmpty(state.Tiles[13]);
        state.RootedActions=1;state.Statuses.Add(new StatusInstance{StatusId="status.root",RemainingActions=1,Stacks=1});long before=state.CommandNumber;
        var session=new GameSession(state,g);var result=session.Apply(new MoveCommand(13));
        Assert.IsTrue(result.Accepted);
        Assert.IsTrue(result.Events.Any(e=>e.Type=="player.rooted"));
        Assert.AreEqual(new GridPosition(2,2),state.PlayerPosition);
        Assert.AreEqual(before+1,state.CommandNumber);
        Assert.AreEqual(0,state.RootedActions);
        Assert.IsFalse(state.Statuses.Exists(s=>s.StatusId=="status.root"));
    }

    [Test] public void InvalidRootedMovementDoesNotConsumeRoot()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(8,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);ConfigureEmpty(state.Tiles[0]);
        state.RootedActions=1;state.Statuses.Add(new StatusInstance{StatusId="status.root",RemainingActions=1,Stacks=1});long before=state.CommandNumber;
        var result=new GameSession(state,g).Apply(new MoveCommand(0));
        Assert.IsFalse(result.Accepted);
        Assert.AreEqual("tile.not_adjacent",result.RejectionReason);
        Assert.AreEqual(1,state.RootedActions);
        Assert.IsTrue(state.Statuses.Exists(s=>s.StatusId=="status.root"));
        Assert.AreEqual(before,state.CommandNumber);
    }

    [Test] public void ThreatenedRootedMovementIsRejectedBeforeRootIsConsumed()
    {
        var g=new FloorGenerator();var state=g.CreateNewRun(10,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);ConfigureEmpty(state.Tiles[13]);
        ConfigureMonster(state.Tiles[14],"monster.rat",3,MonsterIntentKind.Attack,2);
        state.RootedActions=1;state.Statuses.Add(new StatusInstance{StatusId="status.root",RemainingActions=1,Stacks=1});long before=state.CommandNumber;
        var result=new GameSession(state,g).Apply(new MoveCommand(13));
        Assert.IsFalse(result.Accepted);
        Assert.AreEqual("tile.threatened",result.RejectionReason);
        Assert.AreEqual(1,state.RootedActions);
        Assert.IsTrue(state.Statuses.Exists(s=>s.StatusId=="status.root"));
        Assert.AreEqual(before,state.CommandNumber);
    }

    [Test] public void SelfTargetedUtilityAbilitiesDoNotSpendCharges()
    {
        var content=GameContent.CreateDevelopmentFallback();
        var rangerGenerator=new FloorGenerator(content);var ranger=rangerGenerator.CreateNewRun(6,HeroClassId.Ranger,new[]{"ability.ranger.eagle_eye"});ClearBoard(ranger);PlacePlayer(ranger,12);
        int rangerCharges=ranger.AbilityStates[0].Charges;var rangerResult=new GameSession(ranger,rangerGenerator).Apply(new UseAbilityCommand("ability.ranger.eagle_eye",12));
        Assert.IsFalse(rangerResult.Accepted);
        Assert.AreEqual("ability.invalid_target",rangerResult.RejectionReason);
        Assert.AreEqual(rangerCharges,ranger.AbilityStates[0].Charges);

        var thiefGenerator=new FloorGenerator(content);var thief=thiefGenerator.CreateNewRun(7,HeroClassId.Thief,new[]{"ability.thief.shadowstep"});ClearBoard(thief);PlacePlayer(thief,12);
        int thiefCharges=thief.AbilityStates[0].Charges;var thiefResult=new GameSession(thief,thiefGenerator).Apply(new UseAbilityCommand("ability.thief.shadowstep",12));
        Assert.IsFalse(thiefResult.Accepted);
        Assert.AreEqual("ability.invalid_target",thiefResult.RejectionReason);
        Assert.AreEqual(thiefCharges,thief.AbilityStates[0].Charges);
    }

    [Test] public void NewlyAppliedCurseDoesNotDrainUntilNextFloorAction()
    {
        var content=GameContent.CreateDevelopmentFallback();var g=new FloorGenerator(content);var state=g.CreateNewRun(11,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);
        state.AbilityStates.Clear();state.AbilityStates.Add(new AbilityChargeState{AbilityId="ability.knight.shield_wall",Charges=0,RechargeProgress=3});
        ConfigureEmpty(state.Tiles[13]);state.Tiles[13].Terrain=TerrainKind.Grave;
        ConfigureEmpty(state.Tiles[18]);
        var session=new GameSession(state,g,content);
        var cursedMove=session.Apply(new MoveCommand(13));
        Assert.IsTrue(cursedMove.Accepted);
        Assert.AreEqual(3,state.AbilityStates[0].RechargeProgress);
        Assert.IsTrue(state.Statuses.Exists(s=>s.StatusId=="status.curse"&&s.RemainingActions==3));
        var nextMove=session.Apply(new MoveCommand(18));
        Assert.IsTrue(nextMove.Accepted);
        Assert.AreEqual(2,state.AbilityStates[0].RechargeProgress);
        Assert.IsTrue(state.Statuses.Exists(s=>s.StatusId=="status.curse"&&s.RemainingActions==2));
    }

    private static void ClearBoard(RunState state){foreach(var tile in state.Tiles)ConfigureEmpty(tile);}
    private static void PlacePlayer(RunState state,int index){state.PlayerPosition=new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);state.Tiles[index].Occupancy=OccupancyKind.Player;state.Tiles[index].Visibility=TileVisibility.Revealed;state.Tiles[index].Resolution=TileResolution.Resolved;}
    private static void ConfigureEmpty(TileState tile){tile.Content=TileContentKind.Empty;tile.ContentId="tile.empty";tile.Visibility=TileVisibility.Revealed;tile.Resolution=TileResolution.Resolved;tile.Occupancy=OccupancyKind.None;tile.MonsterHp=0;tile.MonsterMaxHp=0;tile.ThreatPattern=ThreatPattern.None;tile.Terrain=TerrainKind.Normal;tile.TerrainTriggered=false;}
    private static void ConfigureMonster(TileState tile,string id,int hp,MonsterIntentKind intent,int power)
    {
        tile.Content=TileContentKind.Monster;tile.ContentId=id;tile.Visibility=TileVisibility.Revealed;tile.Resolution=TileResolution.Available;tile.Occupancy=OccupancyKind.Monster;tile.MonsterHp=hp;tile.MonsterMaxHp=hp;tile.MonsterAttack=power;tile.MonsterDefense=0;tile.ThreatPattern=ThreatPattern.Adjacent;tile.IntentKind=intent;tile.IntentPower=power;
    }
}
