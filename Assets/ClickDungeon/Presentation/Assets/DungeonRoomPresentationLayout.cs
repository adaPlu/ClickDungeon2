using System;
using System.Collections.Generic;

namespace ClickDungeon.Presentation.Assets
{
    public enum DungeonRoomEdge
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum DungeonRoomCorner
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

    /// <summary>
    /// Engine-free presentation contract for the modular five-by-five dungeon room.
    /// It owns only visual placement decisions; gameplay state remains in the simulation.
    /// </summary>
    public static class DungeonRoomPresentationLayout
    {
        public const int BoardSize=5;

        // Canonical production tile presentation vocabulary. These IDs map one-to-one to
        // the approved tile_*.png runtime filenames. Legacy dungeon.* constants below stay
        // available while the live board is migrated task-by-task.
        public const string TileFloorStoneId="tile.floor.stone";
        public const string TileFloorCrackedId="tile.floor.cracked";
        public const string TileFloorMossId="tile.floor.moss";
        public const string TileWaterId="tile.water";
        public const string TileLavaId="tile.lava";
        public const string TileShadowId="tile.shadow";
        public const string TileTrapPitId="tile.trap.pit";
        public const string TileTrapBombId="tile.trap.bomb";
        public const string TileTrapSpikeId="tile.trap.spike";
        public const string TilePressurePlateId="tile.pressure_plate";
        public const string TileTeleportId="tile.teleport";
        public const string TileHealingFountainId="tile.fountain.heal";
        public const string TileStairUpId="tile.stair.up";
        public const string TileLockedStairUpId="tile.stair.up.locked";
        public const string TileStairDownId="tile.stair.down";
        public const string TileLockedStairDownId="tile.stair.down.locked";
        public const string TileWallId="tile.wall";
        public const string TileWallCornerId="tile.wall.corner";
        public const string TileKeyId="tile.key";
        public const string TileChestClosedId="tile.chest.closed";
        public const string TileChestOpenId="tile.chest.open";
        public const string TileDoorLockedId="tile.door.locked";
        public const string TileDoorOpenId="tile.door.open";
        public const string TileTorchId="tile.torch";

        private static readonly string[] CanonicalTiles=
        {
            TileFloorStoneId,
            TileFloorCrackedId,
            TileFloorMossId,
            TileWaterId,
            TileLavaId,
            TileShadowId,
            TileTrapPitId,
            TileTrapBombId,
            TileTrapSpikeId,
            TilePressurePlateId,
            TileTeleportId,
            TileHealingFountainId,
            TileStairUpId,
            TileLockedStairUpId,
            TileStairDownId,
            TileLockedStairDownId,
            TileWallId,
            TileWallCornerId,
            TileKeyId,
            TileChestClosedId,
            TileChestOpenId,
            TileDoorLockedId,
            TileDoorOpenId,
            TileTorchId
        };

        public static IReadOnlyList<string> CanonicalTileIds=>Array.AsReadOnly(CanonicalTiles);

        // Legacy presentation aliases retained until RuntimeGameUI finishes migration to
        // the canonical tile.* vocabulary in the later rendering task.
        public const string StoneFloorId="dungeon.floor.stone";
        public const string CrackedFloorId="dungeon.floor.cracked";
        public const string WallId="dungeon.wall.top";
        public const string CornerId="dungeon.corner.tl";
        public const string TorchId="dungeon.torch";
        public const string ShadowId="dungeon.shadow";
        public const string LockedDoorId="dungeon.door.locked";
        public const string LockId="dungeon.lock";
        public const string PitTrapId="trap.pitfall";
        public const string BombTrapId="trap.bomb";
        public const string SpikeTrapId="trap.spikes";
        public const string StairUpId="dungeon.stair.up";
        public const string LockedStairUpId="dungeon.stair.up.locked";
        public const string StairDownId="dungeon.stair.down";
        public const string LockedStairDownId="dungeon.stair.down.locked";

        public static string FloorIdForCell(int index)
        {
            if(index<0||index>=BoardSize*BoardSize)throw new ArgumentOutOfRangeException(nameof(index));
            return index==6||index==12||index==18?CrackedFloorId:StoneFloorId;
        }

        public static bool HasTorchAtCell(int index)
        {
            if(index<0||index>=BoardSize*BoardSize)throw new ArgumentOutOfRangeException(nameof(index));
            return index==1||index==3;
        }

        public static int WallRotationDegrees(DungeonRoomEdge edge)
        {
            switch(edge)
            {
                case DungeonRoomEdge.Top:return 0;
                case DungeonRoomEdge.Right:return 90;
                case DungeonRoomEdge.Bottom:return 180;
                case DungeonRoomEdge.Left:return 270;
                default:throw new ArgumentOutOfRangeException(nameof(edge));
            }
        }

        public static int CornerRotationDegrees(DungeonRoomCorner corner)
        {
            switch(corner)
            {
                case DungeonRoomCorner.TopLeft:return 0;
                case DungeonRoomCorner.TopRight:return 90;
                case DungeonRoomCorner.BottomRight:return 180;
                case DungeonRoomCorner.BottomLeft:return 270;
                default:throw new ArgumentOutOfRangeException(nameof(corner));
            }
        }
    }
}
