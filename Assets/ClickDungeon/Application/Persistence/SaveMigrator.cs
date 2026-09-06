using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ClickDungeon.Application.State;
using ClickDungeon.Application.Versioning;
using ClickDungeon.Application.Heroes;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Persistence
{
    public static class SaveMigrator
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None
        });

        public static SaveDocument DeserializeAndMigrate(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Save JSON is empty.", nameof(json));

            JObject root;
            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader) { DateParseHandling = DateParseHandling.None })
            {
                root = JObject.Load(jsonReader);
            }

            int schema = root.Value<int?>("schema_version") ?? 1;
            if (schema == GameVersionInfo.SaveSchemaVersion)
            {
                var current = root.ToObject<SaveDocument>(Serializer) ?? throw new JsonSerializationException("Save document invalid.");
                if (current.simulation_version > GameVersionInfo.SimulationVersion) throw new InvalidOperationException($"Save requires newer simulation version {current.simulation_version}.");
                if (current.content_revision > GameVersionInfo.ContentRevision) throw new InvalidOperationException($"Save requires newer content revision {current.content_revision}.");
                return current;
            }
            if (schema > GameVersionInfo.SaveSchemaVersion) throw new InvalidOperationException($"Save requires newer schema {schema}.");
            if (schema != 1) throw new InvalidOperationException($"Unsupported save schema {schema}.");

            var runToken = root["payload"];
            var run = runToken?.ToObject<RunState>(Serializer);
            if (run == null) throw new JsonSerializationException("Legacy save payload is missing.");
            var now = root.Value<string>("updated_at") ?? DateTimeOffset.UtcNow.ToString("O");
            var payload = new SlotSavePayload
            {
                ActiveRun = run,
                Meta = new SlotMetaState
                {
                    HeroClassId = run.HeroClass.ToString(),
                    HeroId = HeroIdentityCatalog.StandardHeroId(run.HeroClass),
                    BestFloor = run.Floor,
                    CampaignCompleted = run.CampaignCompleted,
                    CreatedAt = now,
                    LastPlayedAt = now
                }
            };
            var migrated = new SaveDocument
            {
                revision_number = root.Value<long?>("revision_number") ?? 0,
                updated_at = now,
                payload = payload
            };
            migrated.checksum = ChecksumUtility.Sha256(JsonConvert.SerializeObject(migrated.payload, Formatting.None));
            return migrated;
        }
    }
}
