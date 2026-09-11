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

            // Canonical 24-tile production filenames.
            switch (n)
            {
                case "tile_floor_stone": return "tile.floor.stone";
                case "tile_floor_cracked": return "tile.floor.cracked";
                case "tile_floor_moss": return "tile.floor.moss";
                case "tile_water": return "tile.water";
                case "tile_lava": return "tile.lava";
                case "tile_shadow": return "tile.shadow";
                case "tile_trap_pit": return "tile.trap.pit";
                case "tile_trap_bomb": return "tile.trap.bomb";
                case "tile_trap_spike": return "tile.trap.spike";
                case "tile_pressure_plate": return "tile.pressure_plate";
                case "tile_teleport": return "tile.teleport";
                case "tile_fountain_heal": return "tile.fountain.heal";
                case "tile_stair_up": return "tile.stair.up";
                case "tile_stair_up_locked": return "tile.stair.up.locked";
                case "tile_stair_down": return "tile.stair.down";
                case "tile_stair_down_locked": return "tile.stair.down.locked";
                case "tile_wall": return "tile.wall";
                case "tile_wall_corner": return "tile.wall.corner";
                case "tile_key": return "tile.key";
                case "tile_chest_closed": return "tile.chest.closed";
                case "tile_chest_open": return "tile.chest.open";
                case "tile_door_locked": return "tile.door.locked";
                case "tile_door_open": return "tile.door.open";
                case "tile_torch": return "tile.torch";
            }

            // Legacy runtime filenames remain valid compatibility aliases while callers
            // migrate to the canonical tile.* vocabulary.
            if (n.StartsWith("dungeon_floor_", StringComparison.Ordinal)) return "dungeon.floor." + n.Substring(14);
            if (n.StartsWith("dungeon_wall_", StringComparison.Ordinal)) return "dungeon.wall." + n.Substring(13);
            if (n.StartsWith("dungeon_corner_", StringComparison.Ordinal)) return "dungeon.corner." + n.Substring(15);
            if (n == "dungeon_torch") return "dungeon.torch";
            if (n == "dungeon_door_locked") return "dungeon.door.locked";
            if (n == "dungeon_lock") return "dungeon.lock";
            if (n == "dungeon_shadow") return "dungeon.shadow";
            if (n == "dungeon_stair_up") return "dungeon.stair.up";
            if (n == "dungeon_stair_up_locked") return "dungeon.stair.up.locked";
            if (n == "dungeon_stair_down") return "dungeon.stair.down";
            if (n == "dungeon_stair_down_locked") return "dungeon.stair.down.locked";

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
