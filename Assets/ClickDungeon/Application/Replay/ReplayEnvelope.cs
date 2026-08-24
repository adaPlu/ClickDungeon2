using System;
using System.Collections.Generic;

namespace ClickDungeon.Application.Replay
{
    [Serializable]
    public sealed class ReplayEnvelope
    {
        public int SimulationVersion = 2;
        public int ContentRevision = 2;
        public uint RootSeed;
        public string HeroClassId = "Knight";
        public List<string> UnlockedAbilityIds = new List<string>();
        public List<string> Commands = new List<string>();
    }
}
