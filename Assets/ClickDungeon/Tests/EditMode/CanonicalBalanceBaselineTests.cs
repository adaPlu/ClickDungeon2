using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ClickDungeon.Application.Content;
using ClickDungeon.Simulation.Balance;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class CanonicalBalanceBaselineTests
    {
        private const int RunsPerCohort = 25;
        private const int MaxCommandsPerRun = 1200;
        private const uint SeedBase = 20260825u;

        [Test]
        public void CanonicalProductionBalanceBaselineIsReportedDeterministically()
        {
            string contentPath=Path.Combine(UnityEngine.Application.dataPath,"ClickDungeon","Content","Json");
            var content=new JsonContentCatalogLoader().LoadFromDirectory(contentPath);
            Assert.AreEqual(50,content.Balance.CampaignFloors,"Baseline must use the canonical 50-floor production campaign.");

            var first=new BalanceEvaluator(content).Evaluate(RunsPerCohort,MaxCommandsPerRun,SeedBase);
            var second=new BalanceEvaluator(content).Evaluate(RunsPerCohort,MaxCommandsPerRun,SeedBase);
            Assert.AreEqual(16,first.Cohorts.Count);
            Assert.AreEqual(16,second.Cohorts.Count);

            for(int i=0;i<first.Cohorts.Count;i++)
            {
                var a=first.Cohorts[i];var b=second.Cohorts[i];
                Assert.AreEqual(a.HeroClass,b.HeroClass);Assert.AreEqual(a.Policy,b.Policy);Assert.AreEqual(a.Runs,b.Runs);
                Assert.AreEqual(a.Deaths,b.Deaths);Assert.AreEqual(a.CampaignCompletions,b.CampaignCompletions);Assert.AreEqual(a.StalledRuns,b.StalledRuns);
                Assert.AreEqual(a.TotalCommands,b.TotalCommands);Assert.AreEqual(a.TotalHighestFloor,b.TotalHighestFloor);Assert.AreEqual(a.ForbiddenExits,b.ForbiddenExits);Assert.AreEqual(a.TotalEndingGold,b.TotalEndingGold);
                Debug.Log($"[CD2_BALANCE] hero={a.HeroClass} policy={a.Policy} runs={a.Runs} completion={a.CompletionRate:F3} death={a.DeathRate:F3} stalls={a.StalledRuns} avgFloor={a.AverageHighestFloor:F2} avgCommands={a.AverageCommands:F2} forbidden={a.ForbiddenExits} avgGold={a.AverageEndingGold:F2}");
            }

            Assert.IsTrue(first.Cohorts.All(c=>c.Runs==RunsPerCohort));
        }
    }
}
