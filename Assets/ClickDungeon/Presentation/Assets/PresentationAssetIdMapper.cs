using System;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Canonical filename-to-presentation-id contract shared by editor import tooling and runtime-facing tests.
    /// Keep this mapping stable: content and UI refer to the returned IDs, not physical file paths.
    /// </summary>
    public static class PresentationAssetIdMapper
    {
        public static string SpriteId(string file)=>PresentationAssetId.FromRuntimeArtFile(file);
    }

    /// <summary>
    /// Resolves presentation keys from the selected hero identity, not the legacy class ID.
    /// This prevents heroes that share mechanics, such as Ironheart and Sir Clickington, from
    /// silently collapsing to the same generic Knight artwork.
    /// </summary>
    public static class HeroPresentationAssetResolver
    {
        public static string MasterAssetId(string heroId)=>AssetId(heroId,"master");
        public static string PortraitAssetId(string heroId)=>AssetId(heroId,"portrait");
        public static string RosterAssetId(string heroId)=>AssetId(heroId,"roster");
        public static string GameplayAssetId(string heroId)=>AssetId(heroId,"gameplay");
        public static string IdleAssetId(string heroId)=>AssetId(heroId,"idle");
        public static string AttackAssetId(string heroId)=>AssetId(heroId,"attack");
        public static string HitAssetId(string heroId)=>AssetId(heroId,"hit");
        public static string VictoryAssetId(string heroId)=>AssetId(heroId,"victory");
        public static string DefeatAssetId(string heroId)=>AssetId(heroId,"defeat");

        // Legacy menu callers may still ask for a selection key while the remaster migrates them
        // to the canonical roster/master presentation set. It is intentionally not a required
        // runtime production variant.
        public static string SelectionAssetId(string heroId)=>AssetId(heroId,"select");

        private static string AssetId(string heroId,string variant)
        {
            if(string.IsNullOrWhiteSpace(heroId))return string.Empty;
            if(string.IsNullOrWhiteSpace(variant))return string.Empty;
            return "hero."+heroId.Trim().ToLowerInvariant()+"."+variant.Trim().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Converts simulation tile state into the one canonical sprite ID that the board should display.
    /// This deliberately contains presentation policy only; it never changes simulation state.
    /// </summary>
    public static class TilePresentationAssetResolver
    {
        public static string PrimaryAssetId(TileState tile)
        {
            if(tile==null)return string.Empty;
            if(tile.Visibility==TileVisibility.Hidden)return string.Empty;
            if(tile.Visibility==TileVisibility.Clued)return ClueAssetId(tile.Clue);

            // A resolved chest is the one interactable with an approved post-resolution visual.
            // Other resolved content disappears from the active-content layer instead of remaining
            // visually collectible, locked, dangerous, or alive.
            if(tile.Resolution!=TileResolution.Available)
                return tile.Content==TileContentKind.Chest&&tile.Visibility==TileVisibility.Revealed?"chest.open":string.Empty;

            switch(tile.Content)
            {
                case TileContentKind.Empty:return string.Empty;
                case TileContentKind.Gold:return "currency.gold";
                case TileContentKind.SmallKey:return "key.small";
                case TileContentKind.BigKey:return "key.big";
                case TileContentKind.Chest:return "chest.standard";
                case TileContentKind.SealedVault:return "vault.sealed";
                case TileContentKind.SafeExit:return "exit.safe";
                case TileContentKind.ForbiddenExit:return "exit.forbidden";
                case TileContentKind.Trap:return CanonicalTrapAssetId(tile.ContentId);
                default:return tile.ContentId??string.Empty;
            }
        }

        private static string ClueAssetId(ClueFamily clue)
        {
            switch(clue)
            {
                case ClueFamily.Danger:return "clue.danger";
                case ClueFamily.Opportunity:return "clue.opportunity";
                case ClueFamily.PassageArcane:return "clue.passage";
                default:return string.Empty;
            }
        }

        private static string CanonicalTrapAssetId(string contentId)
        {
            switch(contentId)
            {
                case "trap.fire":
                case "trap.poison":
                case "trap.acid":
                case "trap.freeze":
                case "trap.pitfall":return contentId;
                default:return string.Empty;
            }
        }
    }
}
