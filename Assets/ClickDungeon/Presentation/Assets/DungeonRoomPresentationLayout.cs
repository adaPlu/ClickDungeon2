using System;

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
        public const string StoneFloorId="dungeon.floor.stone";
        public const string CrackedFloorId="dungeon.floor.cracked";
        public const string WallId="dungeon.wall.top";
        public const string CornerId="dungeon.corner.tl";
        public const string TorchId="dungeon.torch";
        public const string ShadowId="dungeon.shadow";
        public const string LockedDoorId="dungeon.door.locked";
        public const string LockId="dungeon.lock";
        public const string SpikeTrapId="trap.spikes";

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
