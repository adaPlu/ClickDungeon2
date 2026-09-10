using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ClickDungeon.Simulation.Abilities;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class NewHeroClassAbilityTests
    {
        private sealed class Fixture
        {
            public GameContent Content;
            public RunState State;
            public AbilityResolver Resolver;
            public List<GameEvent> Events = new List<GameEvent>();
        }

        [Test]
        public void RadiantStrikeDealsNormalAdjacentDamageAndGrantsTwoShield()
        {
            var f=NewFixture(HeroClassId.Paladin);var monster=PutMonster(f,7,10);
            AssertUse(f,"ability.paladin.radiant_strike",7);
            Assert.AreEqual(8,monster.MonsterHp);Assert.AreEqual(2,f.State.ShieldPoints);AssertCharge(f,"ability.paladin.radiant_strike",2);
        }

        [Test]
        public void LayOnHandsHealsFiveWithoutExceedingMaxHp()
        {
            var f=NewFixture(HeroClassId.Paladin);f.State.Hp=10;f.State.MaxHp=17;
            AssertUse(f,"ability.paladin.lay_on_hands");
            Assert.AreEqual(15,f.State.Hp);AssertCharge(f,"ability.paladin.lay_on_hands",1);
        }

        [Test]
        public void ConsecrationDealsTwoToEachAdjacentRevealedMonsterAndGrantsShield()
        {
            var f=NewFixture(HeroClassId.Paladin);var a=PutMonster(f,7,6);var b=PutMonster(f,11,6);var far=PutMonster(f,0,6);
            AssertUse(f,"ability.paladin.consecration");
            Assert.AreEqual(4,a.MonsterHp);Assert.AreEqual(4,b.MonsterHp);Assert.AreEqual(6,far.MonsterHp);Assert.AreEqual(2,f.State.ShieldPoints);
        }

        [Test]
        public void AegisOfDawnGrantsSixShieldAndTwoResponseDefenseBuff()
        {
            var f=NewFixture(HeroClassId.Paladin);
            AssertUse(f,"ability.paladin.aegis_of_dawn");
            Assert.AreEqual(6,f.State.ShieldPoints);Assert.AreEqual(1,StateInt(f.State,"TemporaryDefenseBonus"));Assert.AreEqual(2,StateInt(f.State,"TemporaryDefenseResponsesRemaining"));
        }

        [Test]
        public void DivineBulwarkHealsSixAndGrantsEightShield()
        {
            var f=NewFixture(HeroClassId.Paladin);f.State.Hp=5;
            AssertUse(f,"ability.paladin.divine_bulwark");
            Assert.AreEqual(11,f.State.Hp);Assert.AreEqual(8,f.State.ShieldPoints);
        }

        [Test]
        public void CleavingBlowDealsNormalAdjacentDamagePlusTwo()
        {
            var f=NewFixture(HeroClassId.Berserker);var monster=PutMonster(f,7,12);
            AssertUse(f,"ability.berserker.cleaving_blow",7);
            Assert.AreEqual(6,monster.MonsterHp);AssertCharge(f,"ability.berserker.cleaving_blow",2);
        }

        [Test]
        public void BloodrushNeverReducesHpBelowOneAndRefreshesAttackBuff()
        {
            var f=NewFixture(HeroClassId.Berserker);f.State.Hp=2;
            AssertUse(f,"ability.berserker.bloodrush");
            Assert.AreEqual(1,f.State.Hp);Assert.AreEqual(2,StateInt(f.State,"TemporaryAttackBonus"));Assert.AreEqual(2,StateInt(f.State,"TemporaryAttackActionsRemaining"));
        }

        [Test]
        public void WarCryForcesBasicAttackAtReducedIntentPower()
        {
            var f=NewFixture(HeroClassId.Berserker);var monster=PutMonster(f,7,12,attack:4);monster.IntentKind=MonsterIntentKind.HeavyAttack;monster.IntentPower=7;
            AssertUse(f,"ability.berserker.war_cry",7);
            Assert.AreEqual(MonsterIntentKind.Attack,monster.IntentKind);Assert.AreEqual(3,monster.IntentPower);
        }

        [Test]
        public void FrenzyMakesTwoAdjacentNormalAttacksButStopsAfterLethalFirstHit()
        {
            var f=NewFixture(HeroClassId.Berserker);var monster=PutMonster(f,7,3);
            AssertUse(f,"ability.berserker.frenzy",7);
            Assert.AreEqual(0,monster.MonsterHp);Assert.AreEqual(1,f.Events.Count(e=>e.Type=="ability.damage"));
        }

        [Test]
        public void RagequakeHitsOnlyAdjacentRevealedMonstersAtNormalPowerPlusOne()
        {
            var f=NewFixture(HeroClassId.Berserker);var a=PutMonster(f,7,10);var b=PutMonster(f,11,10);var far=PutMonster(f,0,10);
            AssertUse(f,"ability.berserker.ragequake");
            Assert.AreEqual(5,a.MonsterHp);Assert.AreEqual(5,b.MonsterHp);Assert.AreEqual(10,far.MonsterHp);
        }

        [Test]
        public void ShockWrenchDamagesAdjacentTargetAndRootsSurvivorForOneResponse()
        {
            var f=NewFixture(HeroClassId.Engineer);var monster=PutMonster(f,7,10);
            AssertUse(f,"ability.engineer.shock_wrench",7);
            Assert.AreEqual(8,monster.MonsterHp);Assert.AreEqual(1,monster.MonsterRootActions);
        }

        [Test]
        public void BarrierDroneGrantsFiveShield()
        {
            var f=NewFixture(HeroClassId.Engineer);AssertUse(f,"ability.engineer.barrier_drone");Assert.AreEqual(5,f.State.ShieldPoints);
        }

        [Test]
        public void SnareMineRootsOneRevealedTargetForTwoResponses()
        {
            var f=NewFixture(HeroClassId.Engineer);var monster=PutMonster(f,0,10);
            AssertUse(f,"ability.engineer.snare_mine",0);Assert.AreEqual(2,monster.MonsterRootActions);
        }

        [Test]
        public void OverclockGrantsRefreshOnlyAttackAndDefenseForThreeEnemyResponses()
        {
            var f=NewFixture(HeroClassId.Engineer);AssertUse(f,"ability.engineer.overclock");
            Assert.AreEqual(1,StateInt(f.State,"TemporaryAttackBonus"));Assert.AreEqual(3,StateInt(f.State,"TemporaryAttackResponsesRemaining"));
            Assert.AreEqual(1,StateInt(f.State,"TemporaryDefenseBonus"));Assert.AreEqual(3,StateInt(f.State,"TemporaryDefenseResponsesRemaining"));
            AssertUse(f,"ability.engineer.overclock");
            Assert.AreEqual(1,StateInt(f.State,"TemporaryAttackBonus"));Assert.AreEqual(3,StateInt(f.State,"TemporaryAttackResponsesRemaining"));
        }

        [Test]
        public void ClockworkBarrageTargetsNearestThreeUsingStableBoardIndexTieBreak()
        {
            var f=NewFixture(HeroClassId.Engineer);var a=PutMonster(f,7,10);var b=PutMonster(f,11,10);var c=PutMonster(f,13,10);var far=PutMonster(f,0,10);
            AssertUse(f,"ability.engineer.clockwork_barrage");
            Assert.AreEqual(8,a.MonsterHp);Assert.AreEqual(8,b.MonsterHp);Assert.AreEqual(8,c.MonsterHp);Assert.AreEqual(10,far.MonsterHp);
        }

        [Test]
        public void SmiteHitsRevealedMonsterWithinTwoAtNormalPowerPlusOneAndRejectsFarTarget()
        {
            var f=NewFixture(HeroClassId.Cleric);var near=PutMonster(f,2,10);var far=PutMonster(f,0,10);
            AssertUse(f,"ability.cleric.smite",2);Assert.AreEqual(7,near.MonsterHp);
            Assert.IsFalse(f.Resolver.TryUse(f.State,"ability.cleric.smite",0,f.Events,out var rejection));Assert.AreEqual("ability.invalid_target",rejection);Assert.AreEqual(10,far.MonsterHp);
        }

        [Test]
        public void MendHealsFiveCappedAtMaxHp()
        {
            var f=NewFixture(HeroClassId.Cleric);f.State.Hp=12;AssertUse(f,"ability.cleric.mend");Assert.AreEqual(15,f.State.Hp);
        }

        [Test]
        public void SanctuaryGrantsFiveShieldAndTwoResponseDefenseBuff()
        {
            var f=NewFixture(HeroClassId.Cleric);AssertUse(f,"ability.cleric.sanctuary");
            Assert.AreEqual(5,f.State.ShieldPoints);Assert.AreEqual(1,StateInt(f.State,"TemporaryDefenseBonus"));Assert.AreEqual(2,StateInt(f.State,"TemporaryDefenseResponsesRemaining"));
        }

        [Test]
        public void BlessingGrantsRefreshOnlyAttackAndDefenseForThreeEnemyResponses()
        {
            var f=NewFixture(HeroClassId.Cleric);AssertUse(f,"ability.cleric.blessing");
            Assert.AreEqual(1,StateInt(f.State,"TemporaryAttackBonus"));Assert.AreEqual(3,StateInt(f.State,"TemporaryAttackResponsesRemaining"));
            Assert.AreEqual(1,StateInt(f.State,"TemporaryDefenseBonus"));Assert.AreEqual(3,StateInt(f.State,"TemporaryDefenseResponsesRemaining"));
        }

        [Test]
        public void RadiantRenewalHealsEightAndGrantsFiveShield()
        {
            var f=NewFixture(HeroClassId.Cleric);f.State.Hp=4;AssertUse(f,"ability.cleric.radiant_renewal");Assert.AreEqual(12,f.State.Hp);Assert.AreEqual(5,f.State.ShieldPoints);
        }

        private static Fixture NewFixture(HeroClassId cls)
        {
            var content=GameContent.CreateDevelopmentFallback();
            var generator=new FloorGenerator(content);
            var state=generator.CreateNewRun(7000u+(uint)cls,cls,content.Hero(cls).AbilityIds);
            state.Tiles=new List<TileState>(RunState.BoardSize*RunState.BoardSize);
            for(int i=0;i<RunState.BoardSize*RunState.BoardSize;i++)state.Tiles.Add(new TileState{Index=i,Content=TileContentKind.Empty,ContentId="tile.empty",Visibility=TileVisibility.Hidden,Resolution=TileResolution.Available,Occupancy=OccupancyKind.None});
            state.PlayerPosition=new GridPosition(2,2);
            state.Tiles[12].Visibility=TileVisibility.Revealed;state.Tiles[12].Resolution=TileResolution.Resolved;state.Tiles[12].Occupancy=OccupancyKind.Player;
            return new Fixture{Content=content,State=state,Resolver=new AbilityResolver(content)};
        }

        private static TileState PutMonster(Fixture f,int index,int hp,int attack=2,int defense=0)
        {
            var tile=f.State.Tiles[index];tile.Content=TileContentKind.Monster;tile.ContentId="monster.rat";tile.Visibility=TileVisibility.Revealed;tile.Resolution=TileResolution.Available;tile.Occupancy=OccupancyKind.Monster;tile.MonsterHp=hp;tile.MonsterMaxHp=hp;tile.MonsterAttack=attack;tile.MonsterDefense=defense;tile.IntentKind=MonsterIntentKind.Attack;tile.IntentPower=attack;return tile;
        }

        private static void AssertUse(Fixture f,string abilityId,int target=-1)
        {
            Assert.IsTrue(f.Resolver.TryUse(f.State,abilityId,target,f.Events,out var rejection),rejection);
        }

        private static void AssertCharge(Fixture f,string abilityId,int expected)
        {
            Assert.AreEqual(expected,f.State.AbilityStates.First(a=>a.AbilityId==abilityId).Charges);
        }

        private static int StateInt(RunState state,string fieldName)
        {
            FieldInfo field=typeof(RunState).GetField(fieldName,BindingFlags.Instance|BindingFlags.Public);
            Assert.NotNull(field,$"RunState must expose deterministic field {fieldName}.");
            return (int)field.GetValue(state);
        }
    }
}
