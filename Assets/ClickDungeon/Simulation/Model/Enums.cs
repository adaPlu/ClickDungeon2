namespace ClickDungeon.Simulation.Model
{
    public enum TileVisibility { Hidden, Clued, Identified, Revealed }
    public enum TileResolution { Available, Resolved, Disabled }
    public enum OccupancyKind { None, Player, Monster, Object, Hazard }
    public enum TileContentKind { Empty, Gold, Consumable, Equipment, Shrine, Chest, Trap, Monster, Boss, SmallKey, BigKey, SafeExit, ForbiddenExit, SealedVault, Merchant, SpecialEvent }
    public enum ClueFamily { None, Danger, Opportunity, PassageArcane }
    public enum RouteModifier { Standard, Forbidden }
    public enum RunMode { Campaign, Abyss }
    public enum HeroClassId { Knight, Ranger, Thief, Wizard }
    public enum ThreatPattern { None, Adjacent, CrossTwo, OrthogonalLine, AuraTwo }
    public enum MonsterIntentKind { Attack, HeavyAttack, StealGold, ApplyPoison, Guard, Summon, Hazard }
    public enum DamageType { Physical, Fire, Frost, Lightning, Poison, Arcane, Shadow }
    public enum ShrineChoice { MaxHp, Attack, Defense }
    public enum TerrainKind { Normal, Grave, Flooded, Thorn, Mire, Ice, Charged, Lava, Arcane, Ash }
}
