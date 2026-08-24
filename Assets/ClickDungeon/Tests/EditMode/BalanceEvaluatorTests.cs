using NUnit.Framework;
using ClickDungeon.Simulation.Balance;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class BalanceEvaluatorTests
    {
        [Test]
        public void EvaluatorProducesAllClassPolicyCohortsDeterministically()
        {
            var evaluator=new BalanceEvaluator(GameContent.CreateDevelopmentFallback());
            var first=evaluator.Evaluate(2,120,900u);var second=evaluator.Evaluate(2,120,900u);
            Assert.AreEqual(16,first.Cohorts.Count);Assert.AreEqual(16,second.Cohorts.Count);
            for(int i=0;i<first.Cohorts.Count;i++)
            {
                Assert.AreEqual(first.Cohorts[i].HeroClass,second.Cohorts[i].HeroClass);
                Assert.AreEqual(first.Cohorts[i].Policy,second.Cohorts[i].Policy);
                Assert.AreEqual(first.Cohorts[i].Deaths,second.Cohorts[i].Deaths);
                Assert.AreEqual(first.Cohorts[i].CampaignCompletions,second.Cohorts[i].CampaignCompletions);
                Assert.AreEqual(first.Cohorts[i].StalledRuns,second.Cohorts[i].StalledRuns);
                Assert.AreEqual(first.Cohorts[i].TotalCommands,second.Cohorts[i].TotalCommands);
                Assert.AreEqual(first.Cohorts[i].TotalHighestFloor,second.Cohorts[i].TotalHighestFloor);
                Assert.AreEqual(first.Cohorts[i].ForbiddenExits,second.Cohorts[i].ForbiddenExits);
                Assert.AreEqual(first.Cohorts[i].TotalEndingGold,second.Cohorts[i].TotalEndingGold);
            }
        }

        [Test]
        public void HardRouteCohortExistsForEveryClass()
        {
            var result=new BalanceEvaluator(GameContent.CreateDevelopmentFallback()).Evaluate(1,60,100u);
            Assert.AreEqual(1,result.Find(HeroClassId.Knight,BalancePolicy.HardRoute).Runs);
            Assert.AreEqual(1,result.Find(HeroClassId.Ranger,BalancePolicy.HardRoute).Runs);
            Assert.AreEqual(1,result.Find(HeroClassId.Thief,BalancePolicy.HardRoute).Runs);
            Assert.AreEqual(1,result.Find(HeroClassId.Wizard,BalancePolicy.HardRoute).Runs);
        }
    }
}
