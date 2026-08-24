using System;
using ClickDungeon.Application.State;
using ClickDungeon.Application.Versioning;

namespace ClickDungeon.Application.Persistence
{
    [Serializable]
    public sealed class SaveDocument
    {
        public int schema_version = GameVersionInfo.SaveSchemaVersion;
        public string game_version = GameVersionInfo.GameVersion;
        public int simulation_version = GameVersionInfo.SimulationVersion;
        public int content_revision = GameVersionInfo.ContentRevision;
        public long revision_number;
        public string updated_at = string.Empty;
        public string checksum = string.Empty;
        public SlotSavePayload payload;
    }
}
