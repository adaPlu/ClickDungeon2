using System;
using ClickDungeon.Application.Heroes;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Canonical filename-to-presentation-id contract shared by editor import tooling and runtime-facing tests.
    /// Keep this mapping stable: content and UI refer to the returned IDs, not physical file paths.
    /// </summary>
    public static class PresentationAssetIdMapper
    {
        private static readonly string[] HeroDerivedVariants={"portrait","roster","select","gameplay","idle","attack","hit","victory","defeat"};

        public static string SpriteId(string file)
        {
            if(string.IsNullOrWhiteSpace(file))return string.Empty;
            string n=file.Replace("_placeholder",string.Empty);

            if(n.StartsWith("monster_")&&n.EndsWith("_core"))return "monster."+n.Substring(8,n.Length-13);
            if(n.StartsWith("boss_")&&n.EndsWith("_core"))return "boss."+n.Substring(5,n.Length-10);
            if(n.StartsWith("hero_"))
            {
                if(n.EndsWith("_core"))return "hero."+n.Substring(5,n.Length-10);
                foreach(string variant in HeroDerivedVariants)
                {
                    string suffix="_"+variant;
                    if(!n.EndsWith(suffix,StringComparison.OrdinalIgnoreCase))continue;
                    string heroId=n.Substring(5,n.Length-5-suffix.Length);
                    try{return HeroIdentityCatalog.VisualAssetKeyForHero(heroId,variant);}
                    catch(ArgumentException){return string.Empty;}
                }
            }

            if(n.StartsWith("biome_")&&n.EndsWith("_master"))return "biome."+n.Substring(6,n.Length-13);

            // Modular 5x5 dungeon-room contract. These IDs intentionally describe role rather than source sheet.
            // Approved production art can be replaced or re-extracted without changing gameplay/UI references.
            if(n.StartsWith("dungeon_floor_",StringComparison.OrdinalIgnoreCase))return "dungeon.floor."+n.Substring("dungeon_floor_".Length);
            if(n.StartsWith("dungeon_wall_",StringComparison.OrdinalIgnoreCase))return "dungeon.wall."+n.Substring("dungeon_wall_".Length);
            if(n.StartsWith("dungeon_corner_",StringComparison.OrdinalIgnoreCase))return "dungeon.corner."+n.Substring("dungeon_corner_".Length);
            if(n=="dungeon_torch")return "dungeon.torch";
            if(n=="dungeon_door_locked")return "dungeon.door.locked";
            if(n=="dungeon_lock")return "dungeon.lock";
            if(n=="dungeon_shadow")return "dungeon.shadow";

            if(n.StartsWith("trap_"))return "trap."+n.Substring(5);
            if(n=="clue_danger")return "clue.danger";
            if(n=="clue_opportunity")return "clue.opportunity";
            if(n=="clue_passage")return "clue.passage";
            if(n=="gold")return "currency.gold";
            if(n=="small_key"||n=="key_big")return n=="small_key"?"key.small":"key.big";
            if(n=="big_key")return "key.big";
            if(n=="chest_closed")return "chest.standard";
            if(n=="chest_open")return "chest.open";
            if(n=="sealed_vault")return "vault.sealed";
            if(n=="safe_exit")return "exit.safe";
            if(n=="exit_forbidden"||n=="forbidden_exit")return "exit.forbidden";
            if(n=="merchant")return "merchant.standard";
            if(n=="healing_potion")return "item.healing_potion";
            if(n=="trap_disarm_kit")return "item.trap_disarm_kit";
            if(n=="shrine_hp")return "shrine.choice";
            return string.Empty;
        }
    }
}
