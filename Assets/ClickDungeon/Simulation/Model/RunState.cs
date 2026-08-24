using System;
using System.Collections.Generic;

namespace ClickDungeon.Simulation.Model
{
    [Serializable]
    public sealed class RunState
    {
        public const int BoardSize = 5;
        public uint RootSeed;
        public uint FloorSeed;
        public uint FloorRngState;
        public long CommandNumber;
        public RunMode Mode = RunMode.Campaign;
        public int Floor = 1;
        public int AbyssDepth;
        public string BiomeId = "biome.cavern";
        public string ArchetypeId = "archetype.standard";
        public RouteModifier RouteModifier = RouteModifier.Standard;
        public HeroClassId HeroClass = HeroClassId.Knight;
        public GridPosition PlayerPosition = new GridPosition(2, 2);
        public int Hp = 18;
        public int MaxHp = 18;
        public int Attack = 2;
        public int Defense = 1;
        public int Gold;
        public int SmallKeys;
        public int BigKeys;
        public int ShieldPoints;
        public int FortifyActions;
        public int CamouflageActions;
        public int RootedActions;
        public bool Defending;
        public bool GameOver;
        public bool CampaignCompleted;
        // 0 means use the full campaign length from content. Mobile demo runs set this to the free-intro cap.
        public int CampaignFloorLimit;
        public bool BossRequired;
        public bool BossDefeated;
        public int MonstersDefeated;
        public int TilesResolved;
        public string EquippedWeaponId = string.Empty;
        public string EquippedWeaponAffixId = string.Empty;
        public string EquippedArmorId = string.Empty;
        public string EquippedArmorAffixId = string.Empty;
        public List<TileState> Tiles = new List<TileState>(25);
        public List<StatusInstance> Statuses = new List<StatusInstance>();
        // Legacy/base-id inventory remains for simple stackable consumables and schema compatibility.
        public List<string> InventoryItemIds = new List<string>();
        public List<ItemInstanceState> ItemInstances = new List<ItemInstanceState>();
        public int LootRollCounter;
        public List<AbilityChargeState> AbilityStates = new List<AbilityChargeState>();
    }
}
