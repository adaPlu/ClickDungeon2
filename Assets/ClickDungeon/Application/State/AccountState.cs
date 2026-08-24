using System;
using System.Collections.Generic;

namespace ClickDungeon.Application.State
{
    [Serializable]
    public sealed class AccountState
    {
        public int SchemaVersion = 1;
        public bool ReducedMotion;
        public bool HapticsEnabled = true;
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 1f;
        public string ColorblindMode = "none";
        public float TextScale = 1f;
        public List<string> AchievementIds = new List<string>();
        public long TotalRuns;
        public long TotalDeaths;
        public long TotalVictories;
    }
}
