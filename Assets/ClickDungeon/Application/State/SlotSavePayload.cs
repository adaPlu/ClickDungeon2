using System;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.State
{
    [Serializable]
    public sealed class SlotSavePayload
    {
        public SlotMetaState Meta = new SlotMetaState();
        public RunState ActiveRun;
    }
}
