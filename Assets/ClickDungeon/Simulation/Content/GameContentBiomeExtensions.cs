using System;
using System.Globalization;
using System.Linq;

namespace ClickDungeon.Simulation.Content
{
    /// <summary>
    /// Presentation-facing biome lookup compatibility surface.
    ///
    /// GameContent already owns canonical biome assignment through BiomeForFloor,
    /// while current canonical biome ids/display names/ambience ids follow the
    /// biome.<snake_case> / Title Case / ambience.<snake_case> convention.  Keep
    /// that contract in one place until GameContent exposes its biome dictionary
    /// directly; callers must not duplicate formatting logic themselves.
    /// </summary>
    public static class GameContentBiomeExtensions
    {
        public static BiomeDefinition Biome(this GameContent content, string biomeId)
        {
            if (content == null || string.IsNullOrWhiteSpace(biomeId)) return null;

            const string prefix = "biome.";
            string slug = biomeId.StartsWith(prefix, StringComparison.Ordinal)
                ? biomeId.Substring(prefix.Length)
                : biomeId;
            if (string.IsNullOrWhiteSpace(slug)) return null;

            string displayName = string.Join(
                " ",
                slug.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(part)));

            return new BiomeDefinition
            {
                Id = biomeId,
                DisplayName = displayName,
                AmbienceId = "ambience." + slug
            };
        }
    }
}
