using System;
using System.Collections.Generic;
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
    /// Validates that a runtime-art filename collection contains exactly one file resolving to each
    /// canonical dungeon tile identity. Non-tile runtime art is ignored so the validator can operate
    /// over the shared Art/Runtime tree without treating heroes, monsters or items as errors.
    /// </summary>
    public static class CanonicalDungeonTileSetValidator
    {
        public static string Validate(IEnumerable<string> runtimeFileNames)
        {
            var canonicalIds = new HashSet<string>(DungeonRoomPresentationLayout.CanonicalTileIds, StringComparer.Ordinal);
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);

            if (runtimeFileNames != null)
            {
                foreach (string file in runtimeFileNames)
                {
                    string id = PresentationAssetIdMapper.SpriteId(file);
                    if (string.IsNullOrEmpty(id) || !canonicalIds.Contains(id)) continue;

                    int count;
                    counts.TryGetValue(id, out count);
                    counts[id] = count + 1;
                }
            }

            var missing = new List<string>();
            var duplicates = new List<string>();
            foreach (string id in DungeonRoomPresentationLayout.CanonicalTileIds)
            {
                int count;
                if (!counts.TryGetValue(id, out count))
                    missing.Add(id);
                else if (count > 1)
                    duplicates.Add(id);
            }

            var errors = new List<string>();
            if (missing.Count > 0)
                errors.Add("Missing canonical tile identities: " + string.Join(", ", missing));
            if (duplicates.Count > 0)
                errors.Add("Duplicate canonical tile identities: " + string.Join(", ", duplicates));
            return string.Join("\n", errors);
        }
    }

    /// <summary>
    /// Resolves presentation keys from the selected hero identity, not the legacy class ID.
    /// This prevents heroes that share mechanics, such as Ironheart and Sir Clickington, from
    /// silently collapsing to the same generic Knight artwork.
    /// </summary>
    public static class HeroPresentationAssetResolver
    {
        public static string PortraitAssetId(string heroId)=>AssetId(heroId,"portrait");
        public static string SelectionAssetId(string heroId)=>AssetId(heroId,"select");
        public static string RosterAssetId(string heroId)=>AssetId(heroId,"roster");
        public static string GameplayAssetId(string heroId)=>AssetId(heroId,"gameplay");

        private static string AssetId(string heroId,string variant)
        {
            if(string.IsNullOrWhiteSpace(heroId))return string.Empty;
            if(string.IsNullOrWhiteSpace(variant))return string.Empty;
            return "hero."+heroId.Trim().ToLowerInvariant()+"."+variant.Trim().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Converts simulation tile state into canonical layered presentation keys. These methods are
    /// pure presentation policy: they never mutate tile/run state and never choose gameplay effects.
    /// </summary>
    public static class TilePresentationAssetResolver
    {
        /// <summary>
        /// Base terrain beneath every cell. Existing terrain semantics remain authoritative; art is
        /// selected only from state already stored by simulation/generation.
        /// </summary>
        public static string BaseAssetId(TileState tile,int index)
        {
            if(tile==null)return BaseFloorId(index);
            switch(tile.Terrain)
            {
                case TerrainKind.Flooded:return DungeonRoomPresentationLayout.TileWaterId;
                case TerrainKind.Lava:return DungeonRoomPresentationLayout.TileLavaId;
                case TerrainKind.Arcane:return DungeonRoomPresentationLayout.TileShadowId;
                default:return BaseFloorId(index);
            }
        }

        /// <summary>
        /// Stateful board structures and interactables that occupy the structural layer. Hidden and
        /// clued tiles never leak their exact structure; identified traps are the sole pre-reveal case.
        /// </summary>
        public static string StructuralAssetId(TileState tile)
        {
            if(tile==null)return string.Empty;
            bool revealed=tile.Visibility==TileVisibility.Revealed;
            bool identifiedTrap=tile.Content==TileContentKind.Trap&&tile.Visibility==TileVisibility.Identified;
            if(!revealed&&!identifiedTrap)return string.Empty;

            if(tile.Content==TileContentKind.Trap)
            {
                if(tile.Resolution!=TileResolution.Available)return string.Empty;
                switch(tile.ContentId)
                {
                    case "trap.pitfall":return DungeonRoomPresentationLayout.TileTrapPitId;
                    case "trap.bomb":return DungeonRoomPresentationLayout.TileTrapBombId;
                    case "trap.spike":
                    case "trap.spikes":return DungeonRoomPresentationLayout.TileTrapSpikeId;
                    default:return string.Empty;
                }
            }

            if(!revealed)return string.Empty;
            switch(tile.Content)
            {
                case TileContentKind.Chest:
                    return tile.Resolution==TileResolution.Resolved?DungeonRoomPresentationLayout.TileChestOpenId:DungeonRoomPresentationLayout.TileChestClosedId;
                case TileContentKind.SmallKey:
                case TileContentKind.BigKey:
                    return tile.Resolution==TileResolution.Available?DungeonRoomPresentationLayout.TileKeyId:string.Empty;
                case TileContentKind.SafeExit:
                    return tile.Resolution==TileResolution.Disabled?DungeonRoomPresentationLayout.TileLockedStairDownId:DungeonRoomPresentationLayout.TileStairDownId;
                case TileContentKind.ForbiddenExit:
                    return DungeonRoomPresentationLayout.TileLockedStairDownId;
                case TileContentKind.SealedVault:
                    return tile.Resolution==TileResolution.Resolved?DungeonRoomPresentationLayout.TileDoorOpenId:DungeonRoomPresentationLayout.TileDoorLockedId;
                case TileContentKind.SpecialEvent:
                    return SpecialEventAssetId(tile.ContentId);
                default:return string.Empty;
            }
        }

        /// <summary>
        /// Compatibility/content layer used for occupants, clues and legacy assets. Stateful canonical
        /// structures are rendered separately by StructuralAssetId in the live board.
        /// </summary>
        public static string PrimaryAssetId(TileState tile)
        {
            if(tile==null)return string.Empty;
            if(tile.Visibility==TileVisibility.Hidden)return string.Empty;
            if(tile.Visibility==TileVisibility.Clued)return ClueAssetId(tile.Clue);

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

        private static string BaseFloorId(int index)
        {
            if(index==4||index==20)return DungeonRoomPresentationLayout.TileFloorMossId;
            if(index==6||index==12||index==18)return DungeonRoomPresentationLayout.TileFloorCrackedId;
            return DungeonRoomPresentationLayout.TileFloorStoneId;
        }

        private static string SpecialEventAssetId(string contentId)
        {
            switch(contentId)
            {
                case "special.pressure_plate":return DungeonRoomPresentationLayout.TilePressurePlateId;
                case "special.teleport":return DungeonRoomPresentationLayout.TileTeleportId;
                case "special.fountain.heal":return DungeonRoomPresentationLayout.TileHealingFountainId;
                default:return string.Empty;
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
