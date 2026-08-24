using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using ClickDungeon.Application.State;
using ClickDungeon.Application.Versioning;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Application.Platform;

namespace ClickDungeon.Application.Persistence
{
    public sealed class LocalSaveRepository
    {
        private readonly string _directory;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings { Formatting = Formatting.None };
        public LocalSaveRepository(string directory = null) { _directory = directory ?? Path.Combine(UnityEngine.Application.persistentDataPath, "ClickDungeon2", "slots"); }

        public void SaveSlot(int slot, SlotSavePayload payload, long revision)
        {
            if(slot<1||slot>4) throw new ArgumentOutOfRangeException(nameof(slot));
            if(payload==null) throw new ArgumentNullException(nameof(payload));
            Directory.CreateDirectory(_directory);
            payload.Meta.LastPlayedAt=DateTimeOffset.UtcNow.ToString("O");
            var doc=new SaveDocument{revision_number=revision,updated_at=payload.Meta.LastPlayedAt,payload=payload};
            string payloadJson=JsonConvert.SerializeObject(doc.payload,Settings);
            doc.checksum=ChecksumUtility.Sha256(payloadJson);
            AtomicWrite(slot,JsonConvert.SerializeObject(doc,Formatting.Indented));PersistentDataSync.RequestSync();
        }

        public SaveDocument LoadSlot(int slot)
        {
            if(slot<1||slot>4) throw new ArgumentOutOfRangeException(nameof(slot));
            string primary=Path.Combine(_directory,$"slot_{slot}.json");
            string backup=primary+".bak"; Exception last=null;
            foreach(string path in new[]{primary,backup})
            {
                if(!File.Exists(path)) continue;
                try { var doc=SaveMigrator.DeserializeAndMigrate(File.ReadAllText(path,Encoding.UTF8)); Validate(doc); return doc; }
                catch(Exception ex) { last=ex; }
            }
            if(last!=null) throw new InvalidDataException("No valid save copy available.",last);
            return null;
        }

        public bool SlotExists(int slot) => File.Exists(Path.Combine(_directory,$"slot_{slot}.json")) || File.Exists(Path.Combine(_directory,$"slot_{slot}.json.bak"));

        public void DeleteSlot(int slot)
        {
            foreach(string suffix in new[]{"",".bak",".tmp"})
            {
                string path=Path.Combine(_directory,$"slot_{slot}.json{suffix}"); if(File.Exists(path)) File.Delete(path);
            }
            PersistentDataSync.RequestSync();
        }

        public void Save(int slot, RunState state, long revision)
        {
            var now=DateTimeOffset.UtcNow.ToString("O");
            SaveSlot(slot,new SlotSavePayload{ActiveRun=state,Meta=new SlotMetaState{HeroClassId=state.HeroClass.ToString(),BestFloor=state.Floor,CampaignCompleted=state.CampaignCompleted,CreatedAt=now,LastPlayedAt=now}},revision);
        }
        public SaveDocument Load(int slot) => LoadSlot(slot);

        private void AtomicWrite(int slot,string json)
        {
            string primary=Path.Combine(_directory,$"slot_{slot}.json"); string temp=primary+".tmp"; string backup=primary+".bak";
            File.WriteAllText(temp,json,Encoding.UTF8);
            var verify=SaveMigrator.DeserializeAndMigrate(File.ReadAllText(temp,Encoding.UTF8)); Validate(verify);
            if(File.Exists(primary)) File.Copy(primary,backup,true);
            File.Copy(temp,primary,true); File.Delete(temp);
        }

        private static void Validate(SaveDocument doc)
        {
            if(doc==null||doc.payload==null) throw new InvalidDataException("Save payload missing.");
            if(doc.schema_version!=GameVersionInfo.SaveSchemaVersion) throw new InvalidDataException($"Unsupported schema {doc.schema_version}.");
            if(doc.simulation_version>GameVersionInfo.SimulationVersion) throw new InvalidDataException($"Save requires newer simulation version {doc.simulation_version}.");
            if(doc.content_revision>GameVersionInfo.ContentRevision) throw new InvalidDataException($"Save requires newer content revision {doc.content_revision}.");
            string expected=ChecksumUtility.Sha256(JsonConvert.SerializeObject(doc.payload,Settings));
            if(!string.Equals(expected,doc.checksum,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Checksum mismatch.");
        }
    }
}
