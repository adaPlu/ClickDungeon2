#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ClickDungeon.Presentation.Assets;

namespace ClickDungeon.EditorTools
{
    public static class PresentationAssetGenerator
    {
        public const string OutputDirectory="Assets/ClickDungeon/Presentation/Generated/Resources";
        public const string AssetPath=OutputDirectory+"/ClickDungeonPresentationAssets.asset";
        private const string RuntimeArt="Assets/ClickDungeon/Art/Runtime";
        private const string RuntimeAudio="Assets/ClickDungeon/Audio/Runtime";

        [MenuItem("ClickDungeon/Art/Generate Presentation Asset Database")]
        public static void GenerateMenu()=>Generate(true);

        public static PresentationAssetDatabase Generate(bool log)=>Generate(log,true);

        public static PresentationAssetDatabase Generate(bool log,bool generateAnimations)
        {
            bool hasRuntimeArt=AssetDatabase.IsValidFolder(RuntimeArt);
            bool hasRuntimeAudio=AssetDatabase.IsValidFolder(RuntimeAudio);
            if(hasRuntimeArt){PixelAssetImporter.ConfigureAll();if(generateAnimations)AnimationClipGenerator.GenerateAll();}
            Directory.CreateDirectory(OutputDirectory);

            var sprites=new List<PresentationAssetDatabase.SpriteEntry>();
            if(hasRuntimeArt)
            {
                foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{RuntimeArt}))
                {
                    string path=AssetDatabase.GUIDToAssetPath(guid);string id=PresentationAssetIdMapper.SpriteId(Path.GetFileNameWithoutExtension(path));if(string.IsNullOrEmpty(id))continue;
                    Sprite sprite=ChooseSprite(path);if(sprite!=null&&!sprites.Any(x=>x.Id==id))sprites.Add(new PresentationAssetDatabase.SpriteEntry{Id=id,Sprite=sprite});
                }
            }

            var audio=new List<PresentationAssetDatabase.AudioEntry>();
            if(hasRuntimeAudio)
            {
                foreach(string guid in AssetDatabase.FindAssets("t:AudioClip",new[]{RuntimeAudio}))
                {
                    string path=AssetDatabase.GUIDToAssetPath(guid);string id=AudioId(Path.GetFileNameWithoutExtension(path));if(string.IsNullOrEmpty(id))continue;var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);if(clip!=null&&!audio.Any(x=>x.Id==id))audio.Add(new PresentationAssetDatabase.AudioEntry{Id=id,Clip=clip});
                }
            }

            sprites.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));audio.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));
            var db=AssetDatabase.LoadAssetAtPath<PresentationAssetDatabase>(AssetPath);if(db==null){db=ScriptableObject.CreateInstance<PresentationAssetDatabase>();AssetDatabase.CreateAsset(db,AssetPath);}db.Replace(sprites.ToArray(),audio.ToArray());EditorUtility.SetDirty(db);AssetDatabase.SaveAssets();AssetDatabase.ImportAsset(AssetPath,ImportAssetOptions.ForceUpdate);

            if(!hasRuntimeArt||sprites.Count==0)Debug.LogWarning("ClickDungeon prototype presentation mode: runtime art is unavailable, so UI will use text/color fallbacks. Release validation must remain blocked until runtime art and provenance are restored.");
            if(!hasRuntimeAudio||audio.Count==0)Debug.LogWarning("ClickDungeon prototype presentation mode: runtime audio is unavailable. Release validation must remain blocked until runtime audio and provenance are restored.");
            if(log)Debug.Log($"Generated ClickDungeon presentation database: {sprites.Count} sprites, {audio.Count} audio mappings.");return db;
        }

        private static Sprite ChooseSprite(string path)
        {
            var all=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();if(all.Length==0)return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            return all.FirstOrDefault(s=>s.name=="Idle_1")??all.FirstOrDefault(s=>s.name=="Move_1")??all.FirstOrDefault(s=>s.name=="Attack_1")??all[0];
        }

        private static string AudioId(string file)
        {
            string n=file.Replace("_placeholder",string.Empty);
            switch(n)
            {
                case "ui_reveal":return "event.tile.revealed";case "pickup_gold":return "event.gold.collected";case "pickup_key":return "event.key.collected";case "forbidden_exit":return "event.floor.entered.forbidden";case "safe_exit":return "event.floor.entered.safe";case "chest_open":return "event.chest.opened";case "merchant":return "event.merchant.opened";case "shrine":return "event.shrine.chosen";case "monster_hit":return "event.monster.damaged";case "attack_player":return "event.player.damaged";case "defend_player":return "event.player.defending";case "boss_phase":return "event.boss.phase_changed";case "victory":return "event.campaign.completed";case "defeat":return "event.run.game_over";case "ability":return "event.ability.used";
            }
            if(n.StartsWith("trap_"))return "event.trap."+n.Substring(5);if(n.StartsWith("ambience_"))return "ambience."+n.Substring(9);if(n.StartsWith("music_"))return "music."+n.Substring(6);return string.Empty;
        }
    }
}
#endif
