using System;
using ClickDungeon.Application.State;

namespace ClickDungeon.Application.Services
{
    public sealed class DailyRewardStatus
    {
        public bool CanClaim { get; }
        public int CurrentStreak { get; }
        public int GoldReward { get; }
        public int GemReward { get; }
        public DateTimeOffset NextEligibleUtc { get; }

        internal DailyRewardStatus(bool canClaim,int currentStreak,int goldReward,int gemReward,DateTimeOffset nextEligibleUtc)
        {
            CanClaim=canClaim;
            CurrentStreak=currentStreak;
            GoldReward=goldReward;
            GemReward=gemReward;
            NextEligibleUtc=nextEligibleUtc;
        }
    }

    public sealed class DailyRewardClaimResult
    {
        public bool Claimed { get; }
        public int GoldGranted { get; }
        public int GemsGranted { get; }
        public int Streak { get; }
        public DateTimeOffset NextEligibleUtc { get; }

        internal DailyRewardClaimResult(bool claimed,int goldGranted,int gemsGranted,int streak,DateTimeOffset nextEligibleUtc)
        {
            Claimed=claimed;
            GoldGranted=goldGranted;
            GemsGranted=gemsGranted;
            Streak=streak;
            NextEligibleUtc=nextEligibleUtc;
        }
    }

    public static class DailyRewardService
    {
        private static readonly int[] GoldRewards={100,125,150,175,200,250,400};
        private static readonly int[] GemRewards={5,5,8,8,10,12,25};

        public static DailyRewardStatus GetStatus(AccountState account,DateTimeOffset now)
        {
            if(account==null)throw new ArgumentNullException(nameof(account));
            DateTimeOffset utc=now.ToUniversalTime();
            DateTimeOffset last;
            bool hasLast=TryReadLastClaim(account,out last);
            int dayGap=hasLast?(utc.UtcDateTime.Date-last.UtcDateTime.Date).Days:int.MaxValue;
            bool canClaim=!hasLast||dayGap>0;
            int nextStreak=NextClaimStreak(account.DailyRewardStreak,hasLast,dayGap);
            DateTimeOffset nextEligible=canClaim?utc:StartOfNextUtcDay(utc);
            return new DailyRewardStatus(canClaim,Math.Max(0,account.DailyRewardStreak),RewardAt(GoldRewards,nextStreak),RewardAt(GemRewards,nextStreak),nextEligible);
        }

        public static DailyRewardClaimResult TryClaim(AccountState account,DateTimeOffset now)
        {
            if(account==null)throw new ArgumentNullException(nameof(account));
            DateTimeOffset utc=now.ToUniversalTime();
            DateTimeOffset last;
            bool hasLast=TryReadLastClaim(account,out last);
            int dayGap=hasLast?(utc.UtcDateTime.Date-last.UtcDateTime.Date).Days:int.MaxValue;
            if(hasLast&&dayGap<=0)
                return new DailyRewardClaimResult(false,0,0,Math.Max(0,account.DailyRewardStreak),StartOfNextUtcDay(utc));

            int streak=NextClaimStreak(account.DailyRewardStreak,hasLast,dayGap);
            int gold=RewardAt(GoldRewards,streak);
            int gems=RewardAt(GemRewards,streak);
            account.GoldBalance+=gold;
            account.GemBalance+=gems;
            account.DailyRewardStreak=streak;
            account.DailyRewardLastClaimUtc=utc.ToString("O");
            return new DailyRewardClaimResult(true,gold,gems,streak,StartOfNextUtcDay(utc));
        }

        private static int NextClaimStreak(int currentStreak,bool hasLast,int dayGap)
        {
            if(!hasLast||dayGap>1)return 1;
            if(dayGap==1)return Math.Max(0,currentStreak)+1;
            return Math.Max(1,currentStreak)+1;
        }

        private static int RewardAt(int[] rewards,int streak)
        {
            int normalized=Math.Max(1,streak);
            return rewards[(normalized-1)%rewards.Length];
        }

        private static bool TryReadLastClaim(AccountState account,out DateTimeOffset last)
        {
            if(DateTimeOffset.TryParse(account.DailyRewardLastClaimUtc,out last))
            {
                last=last.ToUniversalTime();
                return true;
            }
            last=default(DateTimeOffset);
            return false;
        }

        private static DateTimeOffset StartOfNextUtcDay(DateTimeOffset utc)
        {
            DateTime next=utc.UtcDateTime.Date.AddDays(1);
            return new DateTimeOffset(next,TimeSpan.Zero);
        }
    }
}
