using System;
using System.Collections.Generic;
using ClickDungeon.Application.Versioning;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Replay
{
    [Serializable]
    public sealed class ReplayEnvelope
    {
        public int SimulationVersion = GameVersionInfo.SimulationVersion;
        public int ContentRevision = GameVersionInfo.ContentRevision;
        public uint RootSeed;
        public string HeroClassId = "Knight";
        public RunMode Mode = RunMode.Campaign;
        public int CampaignFloorLimit;
        public List<string> UnlockedAbilityIds = new List<string>();
        public List<string> Commands = new List<string>();
        public string FinalStateHash = string.Empty;
    }
}
