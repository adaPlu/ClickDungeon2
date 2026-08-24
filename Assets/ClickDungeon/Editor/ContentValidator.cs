#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    public static class ContentValidator
    {
        [MenuItem("ClickDungeon/Validate/Content")]
        public static void ValidateMenu(){ValidateOrThrow();Debug.Log("ClickDungeon content validation passed.");}

        public static void ValidateOrThrow()
        {
            string dir=Path.Combine(Application.dataPath,"ClickDungeon","Content","Json"); if(!Directory.Exists(dir))throw new InvalidDataException("Content JSON directory missing.");
            var required=new[]{"classes.json","abilities.json","monsters.json","bosses.json","biomes.json","floor_archetypes.json","items.json","affixes.json","statuses.json","balance.json"};
            var docs=new Dictionary<string,JObject>(StringComparer.Ordinal);
            foreach(string name in required){string path=Path.Combine(dir,name);if(!File.Exists(path))throw new InvalidDataException($"Missing {name}");docs[name]=JObject.Parse(File.ReadAllText(path));}
            EnsureUnique(docs["classes.json"]["classes"] as JArray,"classes"); EnsureUnique(docs["abilities.json"]["abilities"] as JArray,"abilities"); EnsureUnique(docs["monsters.json"]["monsters"] as JArray,"monsters"); EnsureUnique(docs["bosses.json"]["bosses"] as JArray,"bosses");
        }

        private static void EnsureUnique(JArray array,string label)
        {
            if(array==null)throw new InvalidDataException($"Missing {label} array.");var ids=new HashSet<string>(StringComparer.Ordinal);foreach(var row in array){string id=row.Value<string>("id");if(string.IsNullOrWhiteSpace(id))throw new InvalidDataException($"{label} entry missing id");if(!ids.Add(id))throw new InvalidDataException($"Duplicate {label} id {id}");}
        }
    }
}
#endif
