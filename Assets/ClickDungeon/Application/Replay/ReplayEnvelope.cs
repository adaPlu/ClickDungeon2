using System;
using System.Collections.Generic;
using ClickDungeon.Application.Versioning;

namespace ClickDungeon.Application.Replay
{
    [Serializable]
    public sealed class ReplayEnvelope
    {
        public int SimulationVersion = GameVersionInfo.SimulationVersion;
        public int ContentRevision = GameVersionInfo.ContentRevision;
        public uint RootSeed;
        public string HeroClassId = "Knight";
        public List<string> UnlockedAbilityIds = new List<string>();
        public List<string> Commands = new List<string>();
    }
}
