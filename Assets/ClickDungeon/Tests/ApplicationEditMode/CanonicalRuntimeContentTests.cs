using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ClickDungeon.Application.Content;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.ApplicationEditMode
{
    public sealed class CanonicalRuntimeContentTests
    {
        [Test]
        public void CanonicalJsonLoadsAndGeneratesEntireCampaign()
        {
            string contentDir=FindContentDirectory();
            GameContent content=new JsonContentCatalogLoader().LoadFromDirectory(contentDir);
            Assert.AreEqual(50,content.Balance.CampaignFloors);
            Assert.AreEqual(RunState.BoardSize,content.Balance.BoardSize);
            AssertRange(content.Balance.TargetFloorSeconds,45,90,"floor seconds");
            AssertRange(content.Balance.TargetCampaignMinutes,60,90,"campaign minutes");
            AssertRange(content.Balance.NormalEncounterDecisions,2,4,"normal encounter decisions");
            AssertRange(content.Balance.EliteEncounterDecisions,3,6,"elite encounter decisions");
            AssertRange(content.Balance.BossEncounterDecisions,6,12,"boss encounter decisions");
            Assert.AreEqual(16000,content.Balance.ForbiddenRareRewardMultiplierBasisPoints,"forbidden rare reward multiplier");
            Assert.AreEqual(6,content.Balance.PowerEnvelopes.Length,"canonical power-envelope count");
            AssertPowerEnvelope(content,1,18,2,2,0);
            AssertPowerEnvelope(content,10,23,5,4,1);
            AssertPowerEnvelope(content,50,44,13,10,5);

            foreach(HeroClassId cls in Enum.GetValues(typeof(HeroClassId)))
            {
                var hero=content.Hero(cls);
                Assert.AreEqual(5,hero.AbilityIds.Length,$"{cls} canonical ability count");
                Assert.AreEqual(5,content.AbilitiesForClass(cls).Count(),$"{cls} loaded ability count");
            }

            AssertBoss(content,"boss.lich_sovereign",ThreatPattern.AuraTwo,MonsterIntentKind.Summon,6);
            AssertBoss(content,"boss.rootbound_leviathan",ThreatPattern.CrossTwo,MonsterIntentKind.Hazard,7);
            AssertBoss(content,"boss.frostbog_colossus",ThreatPattern.CrossTwo,MonsterIntentKind.Hazard,8);
            AssertBoss(content,"boss.archdemon_overlord",ThreatPattern.CrossTwo,MonsterIntentKind.Hazard,9);
            AssertBoss(content,"boss.primal_ancient_wyrm",ThreatPattern.OrthogonalLine,MonsterIntentKind.HeavyAttack,10);

            var generator=new FloorGenerator(content);
            var state=generator.CreateNewRun(0xC1C1D00Du,HeroClassId.Knight);
            for(int floor=1;floor<=content.Balance.CampaignFloors;floor++)
            {
                generator.GenerateFloor(state,floor,RouteModifier.Standard);
                Assert.AreEqual(25,state.Tiles.Count,$"Floor {floor} tile count");
                Assert.IsFalse(string.IsNullOrEmpty(state.BiomeId),$"Floor {floor} biome");
                Assert.IsNotEmpty(content.MonsterIdsForBiome(state.BiomeId),$"Floor {floor} monster pool");
                string bossId=content.BossForFloor(floor);
                if(!string.IsNullOrEmpty(bossId))
                {
                    Assert.DoesNotThrow(()=>content.Monster(bossId),$"Boss definition {bossId}");
                    Assert.IsTrue(state.Tiles.Any(t=>t.Content==TileContentKind.Boss&&t.ContentId==bossId),$"Floor {floor} boss tile");
                }
            }
        }

        private static void AssertRange(BalanceRangeDefinition range,int min,int max,string label)
        {
            Assert.NotNull(range,label);Assert.AreEqual(min,range.Min,label+" min");Assert.AreEqual(max,range.Max,label+" max");
        }

        private static void AssertPowerEnvelope(GameContent content,int floor,int hp,int attack,int defense,int itemTier)
        {
            var envelope=content.Balance.PowerEnvelopes.Single(e=>e.Floor==floor);
            Assert.AreEqual(hp,envelope.Hp,$"floor {floor} target hp");
            Assert.AreEqual(attack,envelope.Attack,$"floor {floor} target attack");
            Assert.AreEqual(defense,envelope.Defense,$"floor {floor} target defense");
            Assert.AreEqual(itemTier,envelope.ItemTier,$"floor {floor} target item tier");
        }

        private static void AssertBoss(GameContent content,string id,ThreatPattern threat,MonsterIntentKind intent,int intentPower)
        {
            var boss=content.Monster(id);Assert.AreEqual(threat,boss.ThreatPattern,id+" threat");Assert.AreEqual(intent,boss.PrimaryIntent,id+" intent");Assert.AreEqual(intentPower,boss.IntentPower,id+" intent power");Assert.IsFalse(string.IsNullOrWhiteSpace(boss.Decision),id+" decision rationale");
        }

        private static string FindContentDirectory()
        {
            string[] starts={Directory.GetCurrentDirectory(),TestContext.CurrentContext.TestDirectory};
            foreach(string start in starts)
            {
                var dir=new DirectoryInfo(start);
                for(int depth=0;depth<12&&dir!=null;depth++,dir=dir.Parent)
                {
                    string candidate=Path.Combine(dir.FullName,"Assets","ClickDungeon","Content","Json");
                    if(Directory.Exists(candidate))return candidate;
                }
            }
            throw new DirectoryNotFoundException("Could not locate Assets/ClickDungeon/Content/Json from test working directory.");
        }
    }
}
