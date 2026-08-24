using System;
using System.Collections.Generic;

namespace ClickDungeon.Simulation.Replay
{
    [Serializable]
    public sealed class ReplayRecord
    {
        public int SimulationVersion = 1;
        public int ContentRevision = 1;
        public uint RootSeed;
        public string HeroClassId = "Knight";
        public List<string> Commands = new List<string>();
    }
}
