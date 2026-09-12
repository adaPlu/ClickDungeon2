using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Abilities;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Combat;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class NewHeroClassPassiveTests
    {
        [Test]
        public void PaladinHalfHpPassiveFiresOnlyOnThresholdCrossingOncePerFloorAndResetsNextFloor()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);var state=generator.CreateNewRun(8101u,HeroClassId.Paladin);
            state.MaxHp=17;state.Hp=10;state.ShieldPoints=0;
            Assert.AreEqual(2,DamageResolver.ApplyIncoming(state,4,content));
            Assert.AreEqual(8,state.Hp);Assert.AreEqual(3,state.ShieldPoints);Assert.IsTrue(state.PaladinHalfHpPassiveTriggered);
            state.ShieldPoints=0;
            Assert.AreEqual(2,DamageResolver.ApplyIncoming(state,4,content));
            Assert.AreEqual(6,state.Hp);Assert.AreEqual(0,state.ShieldPoints,"The once-per-floor passive must not fire again while already below half HP.");
            generator.GenerateFloor(state,2,RouteModifier.Standard);
            Assert.IsFalse(state.PaladinHalfHpPassiveTriggered);
        }

        [Test]
        public void BerserkerLowHpAttackBonusIsDerivedAndDoesNotStack()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);var state=generator.CreateNewRun(8102u,HeroClassId.Berserker);
            var monster=new TileState{MonsterHp=20,MonsterMaxHp=20,MonsterDefense=0};
            state.Hp=9;Assert.AreEqual(4,DamageResolver.PlayerAttackDamage(state,monster,content));
            state.Hp=8;Assert.AreEqual(5,DamageResolver.PlayerAttackDamage(state,monster,content));
            Assert.AreEqual(5,DamageResolver.PlayerAttackDamage(state,monster,content),"Repeated damage calculations must not recursively stack the low-HP bonus.");
        }

        [Test]
        public void BloodrushAttackBuffExpiresAfterExactlyTwoOffensiveActions()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);var state=generator.CreateNewRun(8103u,HeroClassId.Berserker,content.Hero(HeroClassId.Berserker).AbilityIds);
            ResetBoard(state);var monster=PutMonster(state,7,60,1);monster.MonsterRootActions=10;var session=new GameSession(state,generator,content);
            Assert.IsTrue(session.Apply(new UseAbilityCommand("ability.berserker.bloodrush")).Accepted);Assert.AreEqual(2,state.TemporaryAttackActionsRemaining);
            int before=monster.MonsterHp;Assert.IsTrue(session.Apply(new AttackCommand(7)).Accepted);Assert.AreEqual(6,before-monster.MonsterHp);Assert.AreEqual(1,state.TemporaryAttackActionsRemaining);
            before=monster.MonsterHp;Assert.IsTrue(session.Apply(new AttackCommand(7)).Accepted);Assert.AreEqual(6,before-monster.MonsterHp);Assert.AreEqual(0,state.TemporaryAttackActionsRemaining);Assert.AreEqual(0,state.TemporaryAttackBonus);
            before=monster.MonsterHp;Assert.IsTrue(session.Apply(new AttackCommand(7)).Accepted);Assert.AreEqual(4,before-monster.MonsterHp);
        }

        [Test]
        public void ResponseBuffsExpireAfterExactlyThreeEnemyResponsesIncludingGuardResponses()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);var state=generator.CreateNewRun(8104u,HeroClassId.Engineer,content.Hero(HeroClassId.Engineer).AbilityIds);
            ResetBoard(state);var monster=PutMonster(state,7,20,2);monster.IntentKind=MonsterIntentKind.Guard;monster.IntentPower=2;
            var resolver=new AbilityResolver(content);var events=new List<GameEvent>();Assert.IsTrue(resolver.TryUse(state,"ability.engineer.overclock",-1,events,out var rejection),rejection);
            Assert.AreEqual(3,state.TemporaryAttackResponsesRemaining);Assert.AreEqual(3,state.TemporaryDefenseResponsesRemaining);
            MonsterIntentResolver.Resolve(state,monster,content,events);Assert.AreEqual(2,state.TemporaryAttackResponsesRemaining);Assert.AreEqual(2,state.TemporaryDefenseResponsesRemaining);
            MonsterIntentResolver.Resolve(state,monster,content,events);Assert.AreEqual(1,state.TemporaryAttackResponsesRemaining);Assert.AreEqual(1,state.TemporaryDefenseResponsesRemaining);
            MonsterIntentResolver.Resolve(state,monster,content,events);Assert.AreEqual(0,state.TemporaryAttackResponsesRemaining);Assert.AreEqual(0,state.TemporaryDefenseResponsesRemaining);Assert.AreEqual(0,state.TemporaryAttackBonus);Assert.AreEqual(0,state.TemporaryDefenseBonus);
        }

        [Test]
        public void TemporaryDefenseAppliesToEnemyDamageBeforeResponseExpiry()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);var state=generator.CreateNewRun(8105u,HeroClassId.Engineer,content.Hero(HeroClassId.Engineer).AbilityIds);
            ResetBoard(state);var monster=PutMonster(state,7,20,3);monster.IntentKind=MonsterIntentKind.Attack;monster.IntentPower=3;
            var resolver=new AbilityResolver(content);var events=new List<GameEvent>();Assert.IsTrue(resolver.TryUse(state,"ability.engineer.overclock",-1,events,out var rejection),rejection);
            int before=state.Hp;MonsterIntentResolver.Resolve(state,monster,content,events);
            Assert.AreEqual(1,before-state.Hp,"Engineer base DEF 1 plus Overclock DEF 1 should reduce a 3-power attack to 1.");Assert.AreEqual(2,state.TemporaryDefenseResponsesRemaining);
        }

        [Test]
        public void EngineerIdentifiesNearestHiddenThreatWithStableIndexTieBreakAndNeverIdentifiesTraps()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);
            for(uint seed=1;seed<=500;seed++)
            {
                var state=generator.CreateNewRun(seed,HeroClassId.Engineer);
                var monsters=state.Tiles.Where(t=>t.Content==TileContentKind.Monster).ToArray();
                if(monsters.Length<2)continue;
                int minDistance=monsters.Min(t=>Manhattan(state.PlayerPosition,Position(t.Index)));
                var tied=monsters.Where(t=>Manhattan(state.PlayerPosition,Position(t.Index))==minDistance).OrderBy(t=>t.Index).ToArray();
                if(tied.Length<2)continue;
                Assert.AreEqual(TileVisibility.Identified,tied[0].Visibility,$"Seed {seed} must use lower board index as the deterministic tie-break.");
                Assert.AreEqual(1,monsters.Count(t=>t.Visibility==TileVisibility.Identified));
                Assert.AreEqual(0,state.Tiles.Count(t=>t.Content==TileContentKind.Trap&&t.Visibility==TileVisibility.Identified));
                return;
            }
            Assert.Fail("Expected at least one deterministic seed with a nearest-monster distance tie.");
        }

        [Test]
        public void ClericFirstCompletedShrineHealsThreeOnlyOncePerFloorAndResetsNextFloor()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);var state=generator.CreateNewRun(8106u,HeroClassId.Cleric);
            ResetBoard(state);PutShrine(state,7);PutShrine(state,11);state.Hp=5;var session=new GameSession(state,generator,content);
            Assert.IsTrue(session.Apply(new ChooseShrineCommand(7,ShrineChoice.Attack)).Accepted);Assert.AreEqual(8,state.Hp);Assert.IsTrue(state.ClericShrinePassiveTriggered);
            Assert.IsTrue(session.Apply(new ChooseShrineCommand(11,ShrineChoice.Defense)).Accepted);Assert.AreEqual(8,state.Hp,"Only the first completed shrine each floor should trigger the Cleric heal.");
            generator.GenerateFloor(state,2,RouteModifier.Standard);Assert.IsFalse(state.ClericShrinePassiveTriggered);
        }

        [Test]
        public void WarCryActuallyResolvesTheForcedBasicAttackAtReducedPower()
        {
            var content=GameContent.CreateDevelopmentFallback();var generator=new FloorGenerator(content);var state=generator.CreateNewRun(8107u,HeroClassId.Berserker,content.Hero(HeroClassId.Berserker).AbilityIds);
            ResetBoard(state);var monster=PutMonster(state,7,20,4);monster.IntentKind=MonsterIntentKind.HeavyAttack;monster.IntentPower=7;var session=new GameSession(state,generator,content);
            int before=state.Hp;var result=session.Apply(new UseAbilityCommand("ability.berserker.war_cry",7));
            Assert.IsTrue(result.Accepted);Assert.AreEqual(3,before-state.Hp,"War Cry should force the next attack to use reduced intent power 3 rather than the monster's base attack 4.");
        }

        private static void ResetBoard(RunState state)
        {
            state.Tiles=new List<TileState>(25);for(int i=0;i<25;i++)state.Tiles.Add(new TileState{Index=i,Content=TileContentKind.Empty,ContentId="tile.empty",Visibility=TileVisibility.Hidden,Resolution=TileResolution.Available,Occupancy=OccupancyKind.None});
            state.PlayerPosition=new GridPosition(2,2);state.Tiles[12].Visibility=TileVisibility.Revealed;state.Tiles[12].Resolution=TileResolution.Resolved;state.Tiles[12].Occupancy=OccupancyKind.Player;
        }

        private static TileState PutMonster(RunState state,int index,int hp,int attack)
        {
            var tile=state.Tiles[index];tile.Content=TileContentKind.Monster;tile.ContentId="monster.rat";tile.Visibility=TileVisibility.Revealed;tile.Resolution=TileResolution.Available;tile.Occupancy=OccupancyKind.Monster;tile.MonsterHp=hp;tile.MonsterMaxHp=hp;tile.MonsterAttack=attack;tile.MonsterDefense=0;tile.IntentKind=MonsterIntentKind.Attack;tile.IntentPower=attack;return tile;
        }

        private static void PutShrine(RunState state,int index)
        {
            var tile=state.Tiles[index];tile.Content=TileContentKind.Shrine;tile.ContentId="shrine.choice";tile.Visibility=TileVisibility.Revealed;tile.Resolution=TileResolution.Available;tile.Occupancy=OccupancyKind.None;
        }

        private static GridPosition Position(int index)=>new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);
        private static int Manhattan(GridPosition a,GridPosition b)=>Math.Abs(a.Row-b.Row)+Math.Abs(a.Col-b.Col);
    }
}
