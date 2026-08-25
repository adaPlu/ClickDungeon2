using System.Collections.Generic;
using NUnit.Framework;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Status;
using ClickDungeon.Simulation.Combat;
using ClickDungeon.Simulation.Progression;

public sealed class CanonicalContentBehaviorTests
{
    [Test] public void PoisonUsesCanonicalStackAndDuration()
    {
        var content=GameContent.CreateDevelopmentFallback();var state=new RunState{Hp=10,MaxHp=10};var events=new List<GameEvent>();
        StatusResolver.AddOrRefresh(state,content,"status.poison");StatusResolver.AddOrRefresh(state,content,"status.poison");
        Assert.AreEqual(2,state.Statuses[0].Stacks);Assert.AreEqual(3,state.Statuses[0].RemainingActions);
        StatusResolver.AdvanceMeaningfulAction(state,content,events);Assert.AreEqual(8,state.Hp);Assert.AreEqual(2,state.Statuses[0].RemainingActions);
    }

    [Test] public void VulnerableIsConsumedByIncomingHit()
    {
        var content=GameContent.CreateDevelopmentFallback();var state=new RunState{Hp=10,MaxHp=10,Defense=0};
        StatusResolver.AddOrRefresh(state,content,"status.vulnerable",3);int applied=DamageResolver.ApplyIncoming(state,2,content);
        Assert.AreEqual(3,applied);Assert.IsFalse(state.Statuses.Exists(s=>s.StatusId=="status.vulnerable"));
    }

    [Test] public void CurseDoesNotAdvanceAsMeaningfulAction()
    {
        var content=GameContent.CreateDevelopmentFallback();var state=new RunState();state.AbilityStates.Add(new AbilityChargeState{AbilityId="ability.knight.shield_wall",Charges=0,RechargeProgress=3});var events=new List<GameEvent>();
        StatusResolver.AddOrRefresh(state,content,"status.curse",2);StatusResolver.AdvanceMeaningfulAction(state,content,events);
        Assert.AreEqual(3,state.AbilityStates[0].RechargeProgress);
        Assert.AreEqual(2,state.Statuses[0].RemainingActions);
    }

    [Test] public void CurseDrainsRechargeProgressOnFloorAction()
    {
        var content=GameContent.CreateDevelopmentFallback();var state=new RunState();state.AbilityStates.Add(new AbilityChargeState{AbilityId="ability.knight.shield_wall",Charges=0,RechargeProgress=3});var events=new List<GameEvent>();
        StatusResolver.AddOrRefresh(state,content,"status.curse",2);StatusResolver.AdvanceFloorAction(state,content,events);
        Assert.AreEqual(2,state.AbilityStates[0].RechargeProgress);
        Assert.AreEqual(1,state.Statuses[0].RemainingActions);
    }

    [Test] public void FallbackStatusTimingMatchesCanonicalTiming()
    {
        var content=GameContent.CreateDevelopmentFallback();
        Assert.AreEqual("enemy_response",content.Status("status.root").TickTiming);
        Assert.AreEqual("floor_action",content.Status("status.curse").TickTiming);
    }

    [Test] public void Depth99AchievementUsesCanonicalThreshold()
    {
        var content=GameContent.CreateDevelopmentFallback();var state=new RunState{Mode=RunMode.Abyss,AbyssDepth=99};var evt=new GameEvent("abyss.depth.entered",-1,"",99);
        CollectionAssert.Contains(new List<string>(AchievementEvaluator.Evaluate(content,state,evt)),"achievement.depth_99");
    }
}
