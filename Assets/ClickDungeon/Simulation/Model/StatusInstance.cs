using System;

namespace ClickDungeon.Simulation.Model
{
    [Serializable]
    public sealed class StatusInstance
    {
        public string StatusId = string.Empty;
        public int RemainingActions;
        public int Stacks = 1;
    }
}
