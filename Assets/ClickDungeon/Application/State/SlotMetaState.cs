using System;
using System.Collections.Generic;

namespace ClickDungeon.Application.State
{
    [Serializable]
    public sealed class SlotMetaState
    {
        public string HeroClassId = "Knight";
        public int ClassMastery;
        public List<string> UnlockedAbilityIds = new List<string>();
        public int BestFloor;
        public int BestAbyssDepth;
        public bool CampaignCompleted;
        public long PlaySeconds;
        public long Deaths;
        public string CreatedAt = string.Empty;
        public string LastPlayedAt = string.Empty;
    }
}
