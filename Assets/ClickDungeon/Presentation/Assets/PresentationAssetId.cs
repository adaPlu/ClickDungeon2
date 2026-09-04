using System;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Canonical presentation IDs derived from files in Art/Runtime.
    /// Stable gameplay/content IDs stay separate from higher-fidelity presentation variants.
    /// </summary>
    public static class PresentationAssetId
    {
        private static readonly string[] HeroVariants = { "master", "gameplay", "portrait", "roster", "victory", "defeat" };

        public static string FromRuntimeArtFile(string file)
        {
            if (string.IsNullOrEmpty(file)) return string.Empty;
            string n = file.Replace("_placeholder", string.Empty);

            if (n.StartsWith("hero_", StringComparison.Ordinal))
            {
                string body = n.Substring(5);
                if (body.EndsWith("_core", StringComparison.Ordinal))
                    return "hero." + body.Substring(0, body.Length - 5);

                foreach (string variant in HeroVariants)
                {
                    string suffix = "_" + variant;
                    if (!body.EndsWith(suffix, StringComparison.Ordinal)) continue;
                    string hero = body.Substring(0, body.Length - suffix.Length);
                    return string.IsNullOrEmpty(hero) ? string.Empty : $"hero.{hero}.{variant}";
                }
                return string.Empty;
            }

            if (n.StartsWith("monster_", StringComparison.Ordinal) && n.EndsWith("_core", StringComparison.Ordinal))
                return "monster." + n.Substring(8, n.Length - 13);
            if (n.StartsWith("boss_", StringComparison.Ordinal) && n.EndsWith("_core", StringComparison.Ordinal))
                return "boss." + n.Substring(5, n.Length - 10);
            if (n.StartsWith("biome_", StringComparison.Ordinal) && n.EndsWith("_master", StringComparison.Ordinal))
                return "biome." + n.Substring(6, n.Length - 13);
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
            if (n == "trap_disarm_kit") return "item.trap_disarm_kit";
            if (n == "shrine_hp") return "shrine.choice";
            return string.Empty;
        }
    }
}
