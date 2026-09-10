using System;
using Newtonsoft.Json;
using NUnit.Framework;
using ClickDungeon.Application.Services;
using ClickDungeon.Application.State;

namespace ClickDungeon.Tests.ApplicationEditMode
{
    public sealed class AccountMenuStateTests
    {
        [Test]
        public void NewAccountStartsWithRealZeroBalancesAndClaimableDayOneReward()
        {
            var account=new AccountState();
            var now=new DateTimeOffset(2026,9,10,12,0,0,TimeSpan.Zero);

            var status=DailyRewardService.GetStatus(account,now);

            Assert.AreEqual(0,account.GoldBalance);
            Assert.AreEqual(0,account.GemBalance);
            Assert.IsTrue(status.CanClaim);
            Assert.AreEqual(0,status.CurrentStreak);
            Assert.AreEqual(100,status.GoldReward);
            Assert.AreEqual(5,status.GemReward);
        }

        [Test]
        public void DailyRewardClaimIsIdempotentWithinSameUtcCalendarDay()
        {
            var account=new AccountState();
            var firstAt=new DateTimeOffset(2026,9,10,0,5,0,TimeSpan.Zero);
            var secondAt=new DateTimeOffset(2026,9,10,23,55,0,TimeSpan.Zero);

            var first=DailyRewardService.TryClaim(account,firstAt);
            var second=DailyRewardService.TryClaim(account,secondAt);

            Assert.IsTrue(first.Claimed);
            Assert.AreEqual(100,first.GoldGranted);
            Assert.AreEqual(5,first.GemsGranted);
            Assert.AreEqual(1,first.Streak);
            Assert.IsFalse(second.Claimed);
            Assert.AreEqual(100,account.GoldBalance);
            Assert.AreEqual(5,account.GemBalance);
            Assert.AreEqual(1,account.DailyRewardStreak);
            Assert.AreEqual(firstAt.ToString("O"),account.DailyRewardLastClaimUtc);
        }

        [Test]
        public void ConsecutiveUtcDayAdvancesRewardStreakAndProgression()
        {
            var account=new AccountState();
            var dayOne=new DateTimeOffset(2026,9,10,22,0,0,TimeSpan.Zero);
            var dayTwo=new DateTimeOffset(2026,9,11,1,0,0,TimeSpan.Zero);

            DailyRewardService.TryClaim(account,dayOne);
            var second=DailyRewardService.TryClaim(account,dayTwo);

            Assert.IsTrue(second.Claimed);
            Assert.AreEqual(2,second.Streak);
            Assert.AreEqual(125,second.GoldGranted);
            Assert.AreEqual(5,second.GemsGranted);
            Assert.AreEqual(225,account.GoldBalance);
            Assert.AreEqual(10,account.GemBalance);
            Assert.AreEqual(new DateTimeOffset(2026,9,12,0,0,0,TimeSpan.Zero),second.NextEligibleUtc);
        }

        [Test]
        public void AccountJsonRoundTripPreservesMenuBalancesAndRewardState()
        {
            var source=new AccountState
            {
                GoldBalance=725,
                GemBalance=31,
                DailyRewardLastClaimUtc="2026-09-10T04:00:00.0000000+00:00",
                DailyRewardStreak=4
            };

            var json=JsonConvert.SerializeObject(source);
            var loaded=JsonConvert.DeserializeObject<AccountState>(json);

            Assert.NotNull(loaded);
            Assert.AreEqual(725,loaded.GoldBalance);
            Assert.AreEqual(31,loaded.GemBalance);
            Assert.AreEqual(4,loaded.DailyRewardStreak);
            Assert.AreEqual(source.DailyRewardLastClaimUtc,loaded.DailyRewardLastClaimUtc);
        }
    }
}
