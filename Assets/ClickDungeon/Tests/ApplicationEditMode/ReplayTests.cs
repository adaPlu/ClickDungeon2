using System.IO;
using NUnit.Framework;
using ClickDungeon.Application.Replay;
using ClickDungeon.Application.Versioning;
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
                Commands={"reveal:1","move:1","ability:ability.thief.trap_scan"}
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
