#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace ClickDungeon.EditorTools
{
    /// <summary>
    /// Canonical filenames expected from the approved ClickDungeon production-art sheets.
    /// This manifest describes presentation assets only; it does not create gameplay content IDs.
    /// </summary>
    public static class ProductionArtManifest
    {
        public static readonly string[] HeroIds =
        {
            "ironheart",
            "clickington",
            "shadowcut",
            "emberwisp",
            "windsong",
            "lightbringer",
            "rageclaw",
            "gearspark",
            "dawnward"
        };

        public static readonly string[] HeroVariants =
        {
            "master", "portrait", "roster", "gameplay", "idle", "attack", "hit", "victory", "defeat"
        };

        // Prefixes reflect the approved roster's threat classification, not existing simulation boss IDs.
        public static readonly MonsterArtFamily[] MonsterFamilies =
        {
            new MonsterArtFamily("goblin_brute_king", true),
            new MonsterArtFamily("crowned_slime", false),
            new MonsterArtFamily("skeleton_warrior", false),
            new MonsterArtFamily("bat_swarm_leader", true),
            new MonsterArtFamily("mimic_chest", false),
            new MonsterArtFamily("fire_imp", false),
            new MonsterArtFamily("armored_boar", false),
            new MonsterArtFamily("spooky_spellbook", false),
            new MonsterArtFamily("cave_spider", false),
            new MonsterArtFamily("theater_curtain_demon", true)
        };

        public static readonly string[] MonsterVariants =
        {
            "master", "portrait", "roster", "gameplay", "spawn", "idle", "attack", "hit", "victory", "defeat"
        };

        public static IEnumerable<string> RequiredRuntimePngNames()
        {
            foreach (string hero in HeroIds)
                foreach (string variant in HeroVariants)
                    yield return $"hero_{hero}_{variant}.png";

            foreach (MonsterArtFamily family in MonsterFamilies)
                foreach (string variant in MonsterVariants)
                    yield return $"{(family.IsBoss ? "boss" : "monster")}_{family.Id}_{variant}.png";

            yield return "iron_sword.png";
        }

        public readonly struct MonsterArtFamily
        {
            public MonsterArtFamily(string id, bool isBoss)
            {
                Id = id ?? throw new ArgumentNullException(nameof(id));
                IsBoss = isBoss;
            }

            public string Id { get; }
            public bool IsBoss { get; }
        }
    }
}
#endif
