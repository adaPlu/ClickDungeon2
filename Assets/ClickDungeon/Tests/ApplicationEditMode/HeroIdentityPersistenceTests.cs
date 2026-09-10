using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ClickDungeon.Application.Persistence;
using ClickDungeon.Application.State;
using ClickDungeon.Application.Versioning;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.ApplicationEditMode
{
    public sealed class HeroIdentityPersistenceTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir=Path.Combine(Path.GetTempPath(),"ClickDungeon2HeroIdentityTests",Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            if(Directory.Exists(_dir))Directory.Delete(_dir,true);
        }

        [Test]
        public void ConvenienceSaveAssignsStandardHeroIdentityForKnight()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var run=new FloorGenerator(content).CreateNewRun(314u,HeroClassId.Knight);
            var repo=new LocalSaveRepository(_dir);

            repo.Save(1,run,1);
            var loaded=repo.LoadSlot(1);

            Assert.AreEqual("ironheart",loaded.payload.Meta.HeroId);
            Assert.AreEqual(HeroClassId.Knight,loaded.payload.ActiveRun.HeroClass);
        }

        [Test]
        public void ExplicitClickingtonIdentityRoundTripsWithoutChangingKnightSimulationClass()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var run=new FloorGenerator(content).CreateNewRun(315u,HeroClassId.Knight);
            var repo=new LocalSaveRepository(_dir);
            var payload=new SlotSavePayload
            {
                Meta=new SlotMetaState{HeroClassId="Knight",HeroId="clickington"},
                ActiveRun=run
            };

            repo.SaveSlot(1,payload,1);
            var loaded=repo.LoadSlot(1);

            Assert.AreEqual("clickington",loaded.payload.Meta.HeroId);
            Assert.AreEqual(HeroClassId.Knight,loaded.payload.ActiveRun.HeroClass);
        }

        [TestCase(HeroClassId.Paladin,"dawnward")]
        [TestCase(HeroClassId.Berserker,"rageclaw")]
        [TestCase(HeroClassId.Engineer,"gearspark")]
        [TestCase(HeroClassId.Cleric,"lightbringer")]
        public void MissingHeroIdentityResolvesToClassDefaultOnLoad(HeroClassId heroClass,string expectedHeroId)
        {
            var content=GameContent.CreateDevelopmentFallback();
            var run=new FloorGenerator(content).CreateNewRun((uint)(400+(int)heroClass),heroClass);
            var repo=new LocalSaveRepository(_dir);
            repo.SaveSlot(1,new SlotSavePayload
            {
                Meta=new SlotMetaState{HeroClassId=heroClass.ToString(),HeroId=null},
                ActiveRun=run
            },1);

            var loaded=repo.LoadSlot(1);

            Assert.AreEqual(heroClass,loaded.payload.ActiveRun.HeroClass);
            Assert.AreEqual(expectedHeroId,loaded.payload.Meta.HeroId);
        }

        [Test]
        public void MismatchedHeroIdentityNormalizesToSavedClassWithoutChangingMechanics()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var run=new FloorGenerator(content).CreateNewRun(500u,HeroClassId.Paladin);
            var repo=new LocalSaveRepository(_dir);
            repo.SaveSlot(1,new SlotSavePayload
            {
                Meta=new SlotMetaState{HeroClassId="Paladin",HeroId="clickington"},
                ActiveRun=run
            },1);

            var loaded=repo.LoadSlot(1);

            Assert.AreEqual(HeroClassId.Paladin,loaded.payload.ActiveRun.HeroClass);
            Assert.AreEqual("Paladin",loaded.payload.Meta.HeroClassId);
            Assert.AreEqual("dawnward",loaded.payload.Meta.HeroId);
        }

        [Test]
        public void CurrentSchemaValidChecksumIsVerifiedBeforeInMemoryHeroNormalization()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var run=new FloorGenerator(content).CreateNewRun(501u,HeroClassId.Paladin);
            var payload=new SlotSavePayload
            {
                Meta=new SlotMetaState{HeroClassId="Paladin",HeroId="clickington"},
                ActiveRun=run
            };
            var doc=new SaveDocument
            {
                revision_number=3,
                updated_at="2026-09-09T12:00:00.0000000+00:00",
                payload=payload
            };
            doc.checksum=ChecksumUtility.Sha256(JsonConvert.SerializeObject(payload,Formatting.None));
            File.WriteAllText(Path.Combine(_dir,"slot_1.json"),JsonConvert.SerializeObject(doc,Formatting.Indented));

            var loaded=new LocalSaveRepository(_dir).LoadSlot(1);

            Assert.AreEqual(HeroClassId.Paladin,loaded.payload.ActiveRun.HeroClass);
            Assert.AreEqual("Paladin",loaded.payload.Meta.HeroClassId);
            Assert.AreEqual("dawnward",loaded.payload.Meta.HeroId,
                "A valid stored checksum must be accepted before identity normalization changes the in-memory payload.");
        }

        [Test]
        public void CurrentSchemaSaveWithoutNewRunStateFieldsRemainsChecksumValid()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var run=new FloorGenerator(content).CreateNewRun(502u,HeroClassId.Knight);
            var repo=new LocalSaveRepository(_dir);
            repo.SaveSlot(1,new SlotSavePayload
            {
                Meta=new SlotMetaState{HeroClassId="Knight",HeroId="ironheart"},
                ActiveRun=run
            },4);

            string path=Path.Combine(_dir,"slot_1.json");
            var root=JObject.Parse(File.ReadAllText(path));
            var storedRun=(JObject)root["payload"]["ActiveRun"];
            string[] fields=
            {
                "TemporaryAttackBonus","TemporaryAttackActionsRemaining","TemporaryAttackResponsesRemaining",
                "TemporaryDefenseBonus","TemporaryDefenseResponsesRemaining",
                "PaladinHalfHpPassiveTriggered","ClericShrinePassiveTriggered"
            };
            foreach(string field in fields)storedRun.Remove(field);
            root["checksum"]=ChecksumUtility.Sha256(root["payload"].ToString(Formatting.None));
            File.WriteAllText(path,root.ToString(Formatting.Indented));

            var loaded=repo.LoadSlot(1);

            Assert.NotNull(loaded,"A valid current-schema save from before the remaster must not fail merely because new default RunState fields were added later.");
            Assert.AreEqual(HeroClassId.Knight,loaded.payload.ActiveRun.HeroClass);
            Assert.AreEqual("ironheart",loaded.payload.Meta.HeroId);
        }

        [TestCase(0,"ironheart")]
        [TestCase(1,"windsong")]
        [TestCase(2,"shadowcut")]
        [TestCase(3,"emberwisp")]
        public void SchemaOneMigrationPreservesLegacyNumericClassAndAssignsCanonicalIdentity(int legacyClassValue,string expectedHeroId)
        {
            var content=GameContent.CreateDevelopmentFallback();
            var heroClass=(HeroClassId)legacyClassValue;
            var run=new FloorGenerator(content).CreateNewRun((uint)(600+legacyClassValue),heroClass);
            string legacyJson=JsonConvert.SerializeObject(new
            {
                schema_version=1,
                game_version="0.1.0",
                simulation_version=1,
                content_revision=1,
                revision_number=7,
                updated_at="2026-01-01T00:00:00.0000000+00:00",
                payload=run
            },Formatting.None);

            var migrated=SaveMigrator.DeserializeAndMigrate(legacyJson);

            Assert.AreEqual(expectedHeroId,migrated.payload.Meta.HeroId);
            Assert.AreEqual(heroClass,migrated.payload.ActiveRun.HeroClass);
            Assert.AreEqual(heroClass.ToString(),migrated.payload.Meta.HeroClassId);
            Assert.AreEqual(GameVersionInfo.SaveSchemaVersion,migrated.schema_version);
        }
    }
}
