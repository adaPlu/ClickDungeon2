using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ClickDungeon.Application.Replay;
using ClickDungeon.Application.Versioning;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.ApplicationEditMode
{
    public sealed class ReplayTests
    {
        [Test]
        public void ReplayCodecRoundTripPreservesDeterministicInputs()
        {
            var replay=new ReplayEnvelope
            {
                SimulationVersion=GameVersionInfo.SimulationVersion,
                ContentRevision=GameVersionInfo.ContentRevision,
                RootSeed=0xDEADBEEFu,
                HeroClassId="Thief",
                UnlockedAbilityIds={"ability.thief.camouflage","ability.thief.trap_scan"},
                Commands={"reveal|1","move|1","ability|ability.thief.trap_scan|-1"}
            };

            string encoded=ReplayCodec.Encode(replay);
            Assert.IsFalse(encoded.Contains("+"));
            Assert.IsFalse(encoded.Contains("/"));
            Assert.IsFalse(encoded.Contains("="));

            var decoded=ReplayCodec.Decode(encoded);
            Assert.AreEqual(replay.SimulationVersion,decoded.SimulationVersion);
            Assert.AreEqual(replay.ContentRevision,decoded.ContentRevision);
            Assert.AreEqual(replay.RootSeed,decoded.RootSeed);
            Assert.AreEqual(replay.HeroClassId,decoded.HeroClassId);
            CollectionAssert.AreEqual(replay.UnlockedAbilityIds,decoded.UnlockedAbilityIds);
            CollectionAssert.AreEqual(replay.Commands,decoded.Commands);
        }

        [Test]
        public void ReplayEnvelopeDefaultsToCurrentVersions()
        {
            var replay=new ReplayEnvelope();
            Assert.AreEqual(GameVersionInfo.SimulationVersion,replay.SimulationVersion);
            Assert.AreEqual(GameVersionInfo.ContentRevision,replay.ContentRevision);
        }

        [Test]
        public void ReplayCodecRejectsUnsupportedVersions()
        {
            var oldSimulation=new ReplayEnvelope{SimulationVersion=GameVersionInfo.SimulationVersion-1,ContentRevision=GameVersionInfo.ContentRevision};
            var futureContent=new ReplayEnvelope{SimulationVersion=GameVersionInfo.SimulationVersion,ContentRevision=GameVersionInfo.ContentRevision+1};
            Assert.Throws<InvalidDataException>(()=>ReplayCodec.Encode(oldSimulation));
            Assert.Throws<InvalidDataException>(()=>ReplayCodec.Encode(futureContent));
        }

        [Test]
        public void EveryCurrentCommandRoundTripsThroughCanonicalCodec()
        {
            var commands=new GameCommand[]
            {
                new RevealTileCommand(1),new MoveCommand(2),new InteractCommand(3),new AttackCommand(4),new DefendCommand(),
                new UseAbilityCommand("ability.wizard.fireball",9),new UseItemCommand("item.healing_potion"),
                new ChooseShrineCommand(5,ShrineChoice.Defense),new BuyItemCommand(6,"item.rusty_sword"),
                new EquipItemCommand("item.rusty_sword","shop-1-2-3"),new TakeSafeExitCommand(7),
                new TakeForbiddenExitCommand(8),new UnlockVaultCommand(10)
            };
            foreach(var command in commands)
            {
                string encoded=ReplayCommandCodec.Encode(command);
                var decoded=ReplayCommandCodec.Decode(encoded);
                Assert.AreEqual(encoded,ReplayCommandCodec.Encode(decoded),command.GetType().Name);
            }
            Assert.Throws<FormatException>(()=>ReplayCommandCodec.Decode("unknown|1"));
        }

        [Test]
        public void RecordedRunReplaysToExactFinalStateHash()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var generator=new FloorGenerator(content);
            var state=generator.CreateNewRun(424242u,HeroClassId.Knight);
            state.CampaignFloorLimit=5;
            var session=new GameSession(state,generator,content);
            var recorder=new ReplayRecorder(state);

            int adjacent=7;
            var command=new RevealTileCommand(adjacent);
            recorder.Record(command);
            var result=session.Apply(command);
            Assert.IsTrue(result.Accepted);

            var replay=recorder.Finish(state);
            var playback=ReplayRunner.Play(ReplayCodec.Decode(ReplayCodec.Encode(replay)),content);
            Assert.AreEqual(1,playback.CommandsApplied);
            Assert.AreEqual(0,playback.RejectedCommands);
            Assert.AreEqual(StateHasher.Hash(state),playback.FinalStateHash);
            Assert.AreEqual(replay.FinalStateHash,playback.FinalStateHash);
        }

        [Test]
        public void ReplayRepositoryRecoversPreviousKnownGoodCopy()
        {
            string dir=Path.Combine(Path.GetTempPath(),"ClickDungeon2ReplayTests",Guid.NewGuid().ToString("N"));
            try
            {
                var repository=new ReplayRepository(dir);
                var first=new ReplayEnvelope{RootSeed=1,FinalStateHash="first"};
                var second=new ReplayEnvelope{RootSeed=2,FinalStateHash="second"};
                repository.SaveLast(first);
                repository.SaveLast(second);
                File.WriteAllText(Path.Combine(dir,"last.replay"),"broken");
                var recovered=repository.LoadLast();
                Assert.AreEqual(1u,recovered.RootSeed);
                Assert.AreEqual("first",recovered.FinalStateHash);
            }
            finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }

        [Test]
        public void StateHashIsStableForEquivalentDeterministicRuns()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var generator=new FloorGenerator(content);
            var first=generator.CreateNewRun(123456u,HeroClassId.Wizard);
            var second=generator.CreateNewRun(123456u,HeroClassId.Wizard);

            Assert.AreEqual(StateHasher.Hash(first),StateHasher.Hash(second));
        }

        [Test]
        public void StateHashChangesWhenFutureAffectingStateChanges()
        {
            var content=GameContent.CreateDevelopmentFallback();
            var generator=new FloorGenerator(content);
            var first=generator.CreateNewRun(77u,HeroClassId.Knight);
            var second=generator.CreateNewRun(77u,HeroClassId.Knight);
            second.Gold++;

            Assert.AreNotEqual(StateHasher.Hash(first),StateHasher.Hash(second));
        }
    }
}
