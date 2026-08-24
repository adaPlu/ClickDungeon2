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

        public static PresentationAssetDatabase Generate(bool log)
        {
            PixelAssetImporter.ConfigureAll();AnimationClipGenerator.GenerateAll();Directory.CreateDirectory(OutputDirectory);
            var sprites=new List<PresentationAssetDatabase.SpriteEntry>();
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{RuntimeArt}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);string id=SpriteId(Path.GetFileNameWithoutExtension(path));if(string.IsNullOrEmpty(id))continue;
                Sprite sprite=ChooseSprite(path);if(sprite!=null&&!sprites.Any(x=>x.Id==id))sprites.Add(new PresentationAssetDatabase.SpriteEntry{Id=id,Sprite=sprite});
            }
            var audio=new List<PresentationAssetDatabase.AudioEntry>();
            foreach(string guid in AssetDatabase.FindAssets("t:AudioClip",new[]{RuntimeAudio}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);string id=AudioId(Path.GetFileNameWithoutExtension(path));if(string.IsNullOrEmpty(id))continue;var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);if(clip!=null&&!audio.Any(x=>x.Id==id))audio.Add(new PresentationAssetDatabase.AudioEntry{Id=id,Clip=clip});
            }
            sprites.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));audio.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));
            var db=AssetDatabase.LoadAssetAtPath<PresentationAssetDatabase>(AssetPath);if(db==null){db=ScriptableObject.CreateInstance<PresentationAssetDatabase>();AssetDatabase.CreateAsset(db,AssetPath);}db.Replace(sprites.ToArray(),audio.ToArray());EditorUtility.SetDirty(db);AssetDatabase.SaveAssets();AssetDatabase.ImportAsset(AssetPath,ImportAssetOptions.ForceUpdate);
            if(log)Debug.Log($"Generated ClickDungeon presentation database: {sprites.Count} sprites, {audio.Count} audio mappings.");return db;
        }

        private static Sprite ChooseSprite(string path)
        {
            var all=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();if(all.Length==0)return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            return all.FirstOrDefault(s=>s.name=="Idle_1")??all.FirstOrDefault(s=>s.name=="Move_1")??all.FirstOrDefault(s=>s.name=="Attack_1")??all[0];
        }

        private static string SpriteId(string file)
        {
            string n=file.Replace("_placeholder",string.Empty);
            if(n.StartsWith("monster_")&&n.EndsWith("_core"))return "monster."+n.Substring(8,n.Length-13);
            if(n.StartsWith("boss_")&&n.EndsWith("_core"))return "boss."+n.Substring(5,n.Length-10);
            if(n.StartsWith("hero_")&&n.EndsWith("_core"))return "hero."+n.Substring(5,n.Length-10);
            if(n.StartsWith("biome_")&&n.EndsWith("_master"))return "biome."+n.Substring(6,n.Length-13);
            if(n.StartsWith("trap_"))return "trap."+n.Substring(5);
            if(n=="clue_danger")return "clue.danger";if(n=="clue_opportunity")return "clue.opportunity";if(n=="clue_passage")return "clue.passage";
            if(n=="gold")return "currency.gold";if(n=="small_key"||n=="key_big")return n=="small_key"?"key.small":"key.big";if(n=="big_key")return "key.big";
            if(n=="chest_closed")return "chest.standard";if(n=="chest_open")return "chest.open";if(n=="sealed_vault")return "vault.sealed";
            if(n=="safe_exit")return "exit.safe";if(n=="exit_forbidden"||n=="forbidden_exit")return "exit.forbidden";if(n=="merchant")return "merchant.standard";
            if(n=="healing_potion")return "item.healing_potion";if(n=="trap_disarm_kit")return "item.trap_disarm_kit";if(n=="shrine_hp")return "shrine.choice";
            return string.Empty;
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
