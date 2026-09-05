using System;

namespace ClickDungeon.Simulation.Model
{
    [Serializable]
    public sealed class TileState
    {
        public int Index;
        public TileVisibility Visibility = TileVisibility.Hidden;
        public TileResolution Resolution = TileResolution.Available;
        public OccupancyKind Occupancy = OccupancyKind.None;
        public TileContentKind Content = TileContentKind.Empty;
        public ClueFamily Clue = ClueFamily.None;
        public string ContentId = string.Empty;
        public string VariantId = string.Empty;
        public int Amount;
        public int InteractionProgress;
        public int MonsterHp;
        public int MonsterMaxHp;
        public int MonsterAttack;
        public int MonsterDefense;
        public int MonsterTurn;
        public int MonsterRootActions;
        public bool MonsterGuarding;
        public bool IsElite;
        public int BossPhase = 1;
        public ThreatPattern ThreatPattern = ThreatPattern.None;
        public MonsterIntentKind IntentKind = MonsterIntentKind.Attack;
        public int IntentPower;
        public TerrainKind Terrain = TerrainKind.Normal;
        public bool TerrainTriggered;
    }
}
