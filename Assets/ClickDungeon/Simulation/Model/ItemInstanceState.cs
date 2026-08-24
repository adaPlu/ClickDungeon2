using System;

namespace ClickDungeon.Simulation.Model
{
    [Serializable]
    public sealed class ItemInstanceState
    {
        public string InstanceId = string.Empty;
        public string BaseItemId = string.Empty;
        public string AffixId = string.Empty;
    }
}
