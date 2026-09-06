using System;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Canonical presentation IDs derived from files in Art/Runtime.
    /// Stable gameplay/content IDs stay separate from higher-fidelity presentation variants.
    /// </summary>
    public static class PresentationAssetId
    {
        private static readonly string[] HeroVariants = { "master", "select", "gameplay", "portrait", "roster", "idle", "attack", "hit", "victory", "defeat" };
        private static readonly string[] MonsterVariants =
        {
            "master", "gameplay", "portrait", "roster", "spawn", "idle", "attack", "hit", "victory", "defeat",
            "special", "cast", "summon", "charge", "teleport", "enrage", "split", "bounce", "splat"
        };

        public static string FromRuntimeArtFile(string file)
        {
            if (string.IsNullOrEmpty(file)) return string.Empty;
            string n = file.Replace("_placeholder", string.Empty);

            if (n.StartsWith("hero_", StringComparison.Ordinal))
                return CharacterId(n.Substring(5), "hero", HeroVariants);

            if (n.StartsWith("monster_", StringComparison.Ordinal))
                return CharacterId(n.Substring(8), "monster", MonsterVariants);

            if (n.StartsWith("boss_", StringComparison.Ordinal))
                return CharacterId(n.Substring(5), "boss", MonsterVariants);

            if (n.StartsWith("biome_", StringComparison.Ordinal) && n.EndsWith("_master", StringComparison.Ordinal))
                return "biome." + n.Substring(6, n.Length - 13);

            if (n.StartsWith("dungeon_floor_", StringComparison.Ordinal)) return "dungeon.floor." + n.Substring(14);
            if (n.StartsWith("dungeon_wall_", StringComparison.Ordinal)) return "dungeon.wall." + n.Substring(13);
            if (n.StartsWith("dungeon_corner_", StringComparison.Ordinal)) return "dungeon.corner." + n.Substring(15);
            if (n == "dungeon_torch") return "dungeon.torch";
            if (n == "dungeon_door_locked") return "dungeon.door.locked";
            if (n == "dungeon_lock") return "dungeon.lock";
            if (n == "dungeon_shadow") return "dungeon.shadow";

            if (n == "trap_disarm_kit") return "item.trap_disarm_kit";
            if (n.StartsWith("trap_", StringComparison.Ordinal)) return "trap." + n.Substring(5);

            if (n == "clue_danger") return "clue.danger";
            if (n == "clue_opportunity") return "clue.opportunity";
            if (n == "clue_passage") return "clue.passage";
            if (n == "gold") return "currency.gold";
            if (n == "small_key") return "key.small";
            if (n == "key_big" || n == "big_key") return "key.big";
            if (n == "chest_closed") return "chest.standard";
            if (n == "chest_open") return "chest.open";
            if (n == "sealed_vault") return "vault.sealed";
            if (n == "safe_exit") return "exit.safe";
            if (n == "exit_forbidden" || n == "forbidden_exit") return "exit.forbidden";
            if (n == "merchant") return "merchant.standard";
            if (n == "healing_potion") return "item.healing_potion";
            if (n == "iron_sword") return "item.iron_sword";
            if (n == "shrine_hp") return "shrine.choice";
            return string.Empty;
        }

        private static string CharacterId(string body, string prefix, string[] variants)
        {
            if (body.EndsWith("_core", StringComparison.Ordinal))
                return prefix + "." + body.Substring(0, body.Length - 5);

            foreach (string variant in variants)
            {
                string suffix = "_" + variant;
                if (!body.EndsWith(suffix, StringComparison.Ordinal)) continue;
                string character = body.Substring(0, body.Length - suffix.Length);
                return string.IsNullOrEmpty(character) ? string.Empty : $"{prefix}.{character}.{variant}";
            }

            return string.Empty;
        }
    }
}
