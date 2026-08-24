using System;
using System.IO;
using NUnit.Framework;
using ClickDungeon.Application.Persistence;
using ClickDungeon.Application.State;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.ApplicationEditMode
{
    public sealed class PersistenceTests
    {
        private string _dir;
        [SetUp] public void SetUp(){_dir=Path.Combine(Path.GetTempPath(),"ClickDungeon2Tests",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(_dir);}
        [TearDown] public void TearDown(){if(Directory.Exists(_dir))Directory.Delete(_dir,true);}

        [Test]
        public void SlotRoundTripPreservesFutureAffectingState()
        {
            var content=GameContent.CreateDevelopmentFallback();var run=new FloorGenerator(content).CreateNewRun(17,HeroClassId.Wizard);run.Gold=33;run.BigKeys=1;run.ShieldPoints=2;
            var repo=new LocalSaveRepository(_dir);repo.SaveSlot(1,new SlotSavePayload{Meta=new SlotMetaState{HeroClassId="Wizard"},ActiveRun=run},1);
            var loaded=repo.LoadSlot(1);Assert.AreEqual(33,loaded.payload.ActiveRun.Gold);Assert.AreEqual(1,loaded.payload.ActiveRun.BigKeys);Assert.AreEqual(2,loaded.payload.ActiveRun.ShieldPoints);Assert.AreEqual(run.FloorRngState,loaded.payload.ActiveRun.FloorRngState);
        }

        [Test]
        public void CorruptPrimaryFallsBackToPreviousKnownGoodCopy()
        {
            var content=GameContent.CreateDevelopmentFallback();var run=new FloorGenerator(content).CreateNewRun(1,HeroClassId.Knight);var repo=new LocalSaveRepository(_dir);
            repo.SaveSlot(1,new SlotSavePayload{Meta=new SlotMetaState(),ActiveRun=run},1);run.Gold=5;repo.SaveSlot(1,new SlotSavePayload{Meta=new SlotMetaState(),ActiveRun=run},2);
            File.WriteAllText(Path.Combine(_dir,"slot_1.json"),"{broken");var recovered=repo.LoadSlot(1);Assert.AreEqual(0,recovered.payload.ActiveRun.Gold);
        }
    }
}
