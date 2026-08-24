#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using ClickDungeon.Application.Content;

namespace ClickDungeon.EditorTools
{
    public static class ContentAssetGenerator
    {
        public const string CanonicalDirectory="Assets/ClickDungeon/Content/Json";
        public const string GeneratedDirectory="Assets/ClickDungeon/Content/Generated/Resources";
        public const string AssetPath=GeneratedDirectory+"/ClickDungeonGeneratedContent.asset";

        [MenuItem("ClickDungeon/Content/Generate Runtime Database")]
        public static void GenerateMenu() { Generate(true); }

        public static GeneratedContentDatabase Generate(bool log)
        {
            Directory.CreateDirectory(GeneratedDirectory);
            string[] files=Directory.GetFiles(CanonicalDirectory,"*.json",SearchOption.TopDirectoryOnly);Array.Sort(files,StringComparer.Ordinal);
            var docs=new List<GeneratedContentDatabase.Document>();int revision=1;
            foreach(string file in files)
            {
                string json=File.ReadAllText(file);JObject root=JObject.Parse(json);revision=Math.Max(revision,root.Value<int?>("revision")??1);
                docs.Add(new GeneratedContentDatabase.Document{FileName=Path.GetFileName(file),Json=json});
            }
            var db=AssetDatabase.LoadAssetAtPath<GeneratedContentDatabase>(AssetPath);
            if(db==null){db=ScriptableObject.CreateInstance<GeneratedContentDatabase>();AssetDatabase.CreateAsset(db,AssetPath);}
            db.ReplaceDocuments(revision,docs);EditorUtility.SetDirty(db);AssetDatabase.SaveAssets();AssetDatabase.ImportAsset(AssetPath,ImportAssetOptions.ForceUpdate);
            // Validate generated data immediately. A generated asset that cannot reconstruct the catalog must never enter a build.
            db.CreateCatalog();
            if(log)Debug.Log($"Generated ClickDungeon runtime content database revision {revision} with {docs.Count} documents at {AssetPath}.");
            return db;
        }
    }
}
#endif
