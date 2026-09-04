using System;
using System.IO;
using Newtonsoft.Json;
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
            var run=new FloorGenerator(content).CreateNewRun(314,HeroClassId.Knight);
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
            var run=new FloorGenerator(content).CreateNewRun(315,HeroClassId.Knight);
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

        [Test]
        public void SchemaOneMigrationAssignsStandardHeroIdentityWithoutChangingClass()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var run=new FloorGenerator(content).CreateNewRun(316,HeroClassId.Knight);
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

            Assert.AreEqual("ironheart",migrated.payload.Meta.HeroId);
            Assert.AreEqual(HeroClassId.Knight,migrated.payload.ActiveRun.HeroClass);
            Assert.AreEqual(GameVersionInfo.SaveSchemaVersion,migrated.schema_version);
        }
    }
}
