using System;
using ClickDungeon.Application.State;

namespace ClickDungeon.Application.Persistence
{
    [Serializable]
    public sealed class SaveDocument
    {
        public int schema_version = 2;
        public string game_version = "0.2.0";
        public int simulation_version = 2;
        public int content_revision = 2;
        public long revision_number;
        public string updated_at = string.Empty;
        public string checksum = string.Empty;
        public SlotSavePayload payload;
    }
}
