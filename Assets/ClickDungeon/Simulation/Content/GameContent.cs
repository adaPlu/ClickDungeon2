using System;
using System.Collections.Generic;
using System.Linq;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Simulation.Content
{
    public sealed class GameContent
    {
        private readonly Dictionary<HeroClassId, HeroDefinition> _heroes = new Dictionary<HeroClassId, HeroDefinition>();
        private readonly Dictionary<string, AbilityDefinition> _abilities = new Dictionary<string, AbilityDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, MonsterDefinition> _monsters = new Dictionary<string, MonsterDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, AffixDefinition> _affixes = new Dictionary<string, AffixDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, LootTableDefinition> _lootTables = new Dictionary<string, LootTableDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, ShopDefinition> _shops = new Dictionary<string, ShopDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, MonsterVariantDefinition> _variants = new Dictionary<string, MonsterVariantDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, TrapDefinition> _traps = new Dictionary<string, TrapDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, StatusDefinition> _statuses = new Dictionary<string, StatusDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, AchievementDefinition> _achievements = new Dictionary<string, AchievementDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string,string> _contentMigrations = new Dictionary<string,string>(StringComparer.Ordinal);
        private readonly Dictionary<string, BiomeDefinition> _biomes = new Dictionary<string, BiomeDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, FloorArchetypeDefinition> _archetypes = new Dictionary<string, FloorArchetypeDefinition>(StringComparer.Ordinal);
        private readonly List<BossDefinition> _bosses = new List<BossDefinition>();
        public BalanceDefinition Balance { get; set; } = new BalanceDefinition();
        public ProgressionDefinition Progression { get; set; } = new ProgressionDefinition();

        public IEnumerable<FloorArchetypeDefinition> Archetypes => _archetypes.Values;
        public IEnumerable<AffixDefinition> Affixes => _affixes.Values;
        public IEnumerable<MonsterVariantDefinition> Variants => _variants.Values;
        public IEnumerable<AchievementDefinition> Achievements => _achievements.Values.OrderBy(a=>a.Id,StringComparer.Ordinal);
        public void Add(HeroDefinition value) => _heroes[value.ClassId] = value;
        public void Add(AbilityDefinition value) => _abilities[value.Id] = value;
        public void Add(MonsterDefinition value) => _monsters[value.Id] = value;
        public void Add(ItemDefinition value) => _items[value.Id] = value;
        public void Add(AffixDefinition value) => _affixes[value.Id] = value;
        public void Add(LootTableDefinition value) => _lootTables[value.Id] = value;
        public void Add(ShopDefinition value) => _shops[value.Id] = value;
        public void Add(MonsterVariantDefinition value) => _variants[value.Id] = value;
        public void Add(TrapDefinition value) => _traps[value.Id] = value;
        public void Add(StatusDefinition value) => _statuses[value.Id] = value;
        public void Add(AchievementDefinition value) => _achievements[value.Id] = value;
        public void AddContentMigration(string oldId,string newId){if(!string.IsNullOrEmpty(oldId)&&!string.IsNullOrEmpty(newId))_contentMigrations[oldId]=newId;}
        public void Add(BiomeDefinition value) => _biomes[value.Id] = value;
        public void Add(FloorArchetypeDefinition value) => _archetypes[value.Id] = value;
        public void Add(BossDefinition value) { _bosses.RemoveAll(b => b.Floor == value.Floor); _bosses.Add(value); }

        public HeroDefinition Hero(HeroClassId id) => _heroes.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing hero definition {id}");
        public AbilityDefinition Ability(string id) => _abilities.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing ability definition {id}");
        public IEnumerable<AbilityDefinition> AbilitiesForClass(HeroClassId id) => _abilities.Values.Where(a => a.ClassId == id).OrderBy(a => a.UnlockMastery).ThenBy(a => a.Id, StringComparer.Ordinal);
        public bool TryMonster(string id, out MonsterDefinition value) => _monsters.TryGetValue(id, out value);
        public MonsterDefinition Monster(string id) => _monsters.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing monster definition {id}");
        public bool TryItem(string id, out ItemDefinition value) => _items.TryGetValue(id, out value);
        public ItemDefinition Item(string id) => _items.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing item definition {id}");
        public bool TryAffix(string id, out AffixDefinition value) => _affixes.TryGetValue(id, out value);
        public AffixDefinition Affix(string id) => _affixes.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing affix definition {id}");
        public LootTableDefinition LootTable(string id) => _lootTables.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing loot table {id}");
        public ShopDefinition Shop(string id) => _shops.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing shop definition {id}");
        public bool TryVariant(string id, out MonsterVariantDefinition value) => _variants.TryGetValue(id, out value);
        public TrapDefinition Trap(string id) => _traps.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing trap definition {id}");
        public StatusDefinition Status(string id) => _statuses.TryGetValue(id,out var value)?value:throw new KeyNotFoundException($"Missing status definition {id}");
        public string MigrateContentId(string id){if(string.IsNullOrEmpty(id))return id;string current=id;var seen=new HashSet<string>(StringComparer.Ordinal);while(_contentMigrations.TryGetValue(current,out var next)&&seen.Add(current))current=next;return current;}
        public string[] TrapIdsForFloor(int floor) { var values=_traps.Values.Where(t=>t.MinFloor<=floor).OrderBy(t=>t.Id,StringComparer.Ordinal).Select(t=>t.Id).ToArray(); return values.Length>0?values:new[]{"trap.fire","trap.poison"}; }
        public FloorArchetypeDefinition Archetype(string id) => _archetypes.TryGetValue(id, out var value) ? value : throw new KeyNotFoundException($"Missing archetype definition {id}");

        public string BiomeForFloor(int floor)
        {
            foreach (var biome in _biomes.Values.OrderBy(b => b.FirstFloor)) if (floor >= biome.FirstFloor && floor <= biome.LastFloor) return biome.Id;
            if (_biomes.Count == 0) return "biome.cavern";
            return _biomes.Values.OrderBy(b => Math.Abs(b.LastFloor - floor)).First().Id;
        }

        public string BossForFloor(int floor)
        {
            var boss = _bosses.FirstOrDefault(b => b.Floor == floor);
            return boss?.Id ?? string.Empty;
        }

        public string[] MonsterIdsForBiome(string biomeId)
        {
            var values = _monsters.Values.Where(m => !m.Id.StartsWith("boss.", StringComparison.Ordinal) && (m.BiomeIds.Length == 0 || Array.IndexOf(m.BiomeIds, biomeId) >= 0)).Select(m => m.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray();
            return values.Length > 0 ? values : new[] { "monster.rat", "monster.slime", "monster.goblin" };
        }

        public static GameContent CreateDevelopmentFallback()
        {
            var c = new GameContent();
            c.Add(new HeroDefinition { ClassId=HeroClassId.Knight, BaseHp=18, BaseAttack=2, BaseDefense=2, AbilityIds=new[]{"ability.knight.shield_wall","ability.knight.taunt","ability.knight.fortify","ability.knight.valiant_strike","ability.knight.guardians_oath"} });
            c.Add(new HeroDefinition { ClassId=HeroClassId.Ranger, BaseHp=14, BaseAttack=3, BaseDefense=1, AbilityIds=new[]{"ability.ranger.piercing_shot","ability.ranger.rapid_volley","ability.ranger.camouflage","ability.ranger.net_trap","ability.ranger.eagle_eye"} });
            c.Add(new HeroDefinition { ClassId=HeroClassId.Thief, BaseHp=13, BaseAttack=3, BaseDefense=1, AbilityIds=new[]{"ability.thief.trap_scan","ability.thief.shadowstep","ability.thief.disarm_expert","ability.thief.ambush","ability.thief.veil_of_smoke"} });
            c.Add(new HeroDefinition { ClassId=HeroClassId.Wizard, BaseHp=12, BaseAttack=4, BaseDefense=0, AbilityIds=new[]{"ability.wizard.fireball","ability.wizard.frost_nova","ability.wizard.chain_lightning","ability.wizard.arcane_shield","ability.wizard.meteor"} });
            AddAbilities(c,HeroClassId.Knight,new[]{("ability.knight.shield_wall",3,8),("ability.knight.taunt",2,10),("ability.knight.fortify",2,10),("ability.knight.valiant_strike",2,9),("ability.knight.guardians_oath",1,14)});
            AddAbilities(c,HeroClassId.Ranger,new[]{("ability.ranger.piercing_shot",3,8),("ability.ranger.rapid_volley",2,10),("ability.ranger.camouflage",2,9),("ability.ranger.net_trap",2,8),("ability.ranger.eagle_eye",3,7)});
            AddAbilities(c,HeroClassId.Thief,new[]{("ability.thief.trap_scan",3,6),("ability.thief.shadowstep",2,8),("ability.thief.disarm_expert",3,7),("ability.thief.ambush",2,8),("ability.thief.veil_of_smoke",2,9)});
            AddAbilities(c,HeroClassId.Wizard,new[]{("ability.wizard.fireball",3,8),("ability.wizard.frost_nova",2,9),("ability.wizard.chain_lightning",2,10),("ability.wizard.arcane_shield",2,8),("ability.wizard.meteor",1,14)});

            AddMonster(c,"monster.rat",3,1,0,ThreatPattern.Adjacent,MonsterIntentKind.Attack,2,"fast_weak_threat",new[]{"biome.cavern"});
            AddMonster(c,"monster.slime",4,1,0,ThreatPattern.Adjacent,MonsterIntentKind.ApplyPoison,1,"hazard_residue",new[]{"biome.cavern","biome.sunken_temple","biome.mire"});
            AddMonster(c,"monster.goblin",4,2,1,ThreatPattern.Adjacent,MonsterIntentKind.StealGold,2,"steal_and_retreat",new[]{"biome.cavern"});
            AddMonster(c,"monster.skeleton",5,2,2,ThreatPattern.Adjacent,MonsterIntentKind.Guard,2,"defensive_undead",new[]{"biome.crypt","biome.frozen_ruins"});
            AddMonster(c,"monster.spider",4,2,1,ThreatPattern.CrossTwo,MonsterIntentKind.ApplyPoison,2,"web_zone",new[]{"biome.sunken_temple","biome.thorn_wilds","biome.mire"});
            AddMonster(c,"monster.wolf",5,3,1,ThreatPattern.Adjacent,MonsterIntentKind.Attack,3,"pack_hunter",new[]{"biome.thorn_wilds"});
            AddMonster(c,"monster.bandit",6,3,2,ThreatPattern.Adjacent,MonsterIntentKind.StealGold,3,"loot_raider",new[]{"biome.thorn_wilds"});
            AddMonster(c,"monster.cultist",6,3,2,ThreatPattern.AuraTwo,MonsterIntentKind.Guard,3,"enemy_support",new[]{"biome.crypt","biome.storm_plateau"});
            AddMonster(c,"monster.warlock",6,4,2,ThreatPattern.OrthogonalLine,MonsterIntentKind.Summon,4,"summoner",new[]{"biome.storm_plateau","biome.arcane_nexus"});
            AddMonster(c,"monster.wraith",7,4,3,ThreatPattern.CrossTwo,MonsterIntentKind.Attack,4,"phase_threat",new[]{"biome.crypt","biome.arcane_nexus"});
            AddMonster(c,"monster.golem",8,4,4,ThreatPattern.Adjacent,MonsterIntentKind.HeavyAttack,6,"telegraphed_smash",new[]{"biome.frozen_ruins","biome.storm_plateau"});
            AddMonster(c,"monster.vampire",6,4,3,ThreatPattern.Adjacent,MonsterIntentKind.Attack,4,"life_steal",new[]{"biome.crypt","biome.arcane_nexus"});
            AddMonster(c,"monster.demon",8,5,3,ThreatPattern.CrossTwo,MonsterIntentKind.Hazard,5,"burn_zone",new[]{"biome.lava_field","biome.ash_wastes"});
            AddMonster(c,"monster.hellhound",9,6,3,ThreatPattern.Adjacent,MonsterIntentKind.Hazard,5,"ember_charge",new[]{"biome.lava_field","biome.ash_wastes"});
            AddMonster(c,"monster.revenant",8,5,3,ThreatPattern.Adjacent,MonsterIntentKind.HeavyAttack,5,"relentless_undead",new[]{"biome.frozen_ruins"});
            AddMonster(c,"monster.orc",6,3,2,ThreatPattern.Adjacent,MonsterIntentKind.Attack,3,"bruiser",new[]{"biome.cavern","biome.thorn_wilds"});
            AddMonster(c,"monster.troll",7,3,3,ThreatPattern.Adjacent,MonsterIntentKind.HeavyAttack,4,"regenerator",new[]{"biome.thorn_wilds"});
            AddMonster(c,"monster.witch",6,4,2,ThreatPattern.OrthogonalLine,MonsterIntentKind.Hazard,4,"hexer",new[]{"biome.sunken_temple","biome.mire"});
            AddMonster(c,"monster.dragon",10,6,4,ThreatPattern.OrthogonalLine,MonsterIntentKind.Hazard,7,"breath_line",new[]{"biome.lava_field"});
            AddMonster(c,"monster.lich",9,5,4,ThreatPattern.AuraTwo,MonsterIntentKind.Summon,5,"necromancer",new[]{"biome.arcane_nexus"});
            AddMonster(c,"monster.archdemon",11,7,5,ThreatPattern.CrossTwo,MonsterIntentKind.Hazard,7,"infernal_elite",new[]{"biome.ash_wastes"});
            AddMonster(c,"monster.ancient_wyrm",12,7,5,ThreatPattern.OrthogonalLine,MonsterIntentKind.HeavyAttack,8,"wyrm_sweep",new[]{"biome.ash_wastes"});
            AddMonster(c,"monster.bat",3,2,0,ThreatPattern.Adjacent,MonsterIntentKind.Attack,2,"evasive_swarm",new[]{"biome.cavern"});
            AddMonster(c,"boss.lich_sovereign",28,6,4,ThreatPattern.AuraTwo,MonsterIntentKind.Summon,6,"boss_lich",new string[0]);
            AddMonster(c,"boss.rootbound_leviathan",36,7,5,ThreatPattern.CrossTwo,MonsterIntentKind.Hazard,7,"boss_root",new string[0]);
            AddMonster(c,"boss.frostbog_colossus",44,8,6,ThreatPattern.AuraTwo,MonsterIntentKind.HeavyAttack,8,"boss_frostbog",new string[0]);
            AddMonster(c,"boss.archdemon_overlord",52,9,7,ThreatPattern.CrossTwo,MonsterIntentKind.Hazard,9,"boss_archdemon",new string[0]);
            AddMonster(c,"boss.primal_ancient_wyrm",64,10,8,ThreatPattern.OrthogonalLine,MonsterIntentKind.HeavyAttack,10,"boss_wyrm",new string[0]);
            c.Add(new BossDefinition{Id="boss.lich_sovereign",Floor=10});c.Add(new BossDefinition{Id="boss.rootbound_leviathan",Floor=20});c.Add(new BossDefinition{Id="boss.frostbog_colossus",Floor=30});c.Add(new BossDefinition{Id="boss.archdemon_overlord",Floor=40});c.Add(new BossDefinition{Id="boss.primal_ancient_wyrm",Floor=50});
            string[] biomeIds={"biome.cavern","biome.crypt","biome.sunken_temple","biome.thorn_wilds","biome.mire","biome.frozen_ruins","biome.storm_plateau","biome.lava_field","biome.arcane_nexus","biome.ash_wastes"}; for(int i=0;i<biomeIds.Length;i++)c.Add(new BiomeDefinition{Id=biomeIds[i],FirstFloor=i*5+1,LastFloor=i*5+5});
            AddArchetype(c,"archetype.standard",30);AddArchetype(c,"archetype.monster_den",15,3,null,0,null,0,-1,-1,0,false);AddArchetype(c,"archetype.treasure_vault",12,0,null,1,null,2,2,-1,0,false);AddArchetype(c,"archetype.trap_gallery",12,0,null,3,null,1,0,-1,0,false);AddArchetype(c,"archetype.shrine_crossroads",10,-1,null,0,null,0,0,2,0,false);AddArchetype(c,"archetype.merchant_refuge",8,0,1,0,0,0,0,-1,1,false);AddArchetype(c,"archetype.cursed_depth",8,2,null,2,null,0,1,-1,0,false);AddArchetype(c,"archetype.boss_approach",0,0,null,0,null,0,0,-1,0,true);
            c.Add(new StatusDefinition{Id="status.poison",Category="persistent",TickTiming="meaningful_action",StackRule="stack_to_3",EffectId="damage_per_stack",DefaultDuration=3,MaxStacks=3});c.Add(new StatusDefinition{Id="status.burn",Category="persistent",TickTiming="meaningful_action",StackRule="refresh",EffectId="damage_per_stack",DefaultDuration=3,MaxStacks=1});c.Add(new StatusDefinition{Id="status.root",Category="tactical",TickTiming="enemy_response",StackRule="refresh",EffectId="root",DefaultDuration=2,MaxStacks=1});c.Add(new StatusDefinition{Id="status.vulnerable",Category="tactical",TickTiming="incoming_hit",StackRule="refresh",EffectId="incoming_damage_plus_one",DefaultDuration=2,MaxStacks=1});c.Add(new StatusDefinition{Id="status.stun",Category="tactical",TickTiming="enemy_response",StackRule="refresh",EffectId="stun",DefaultDuration=1,MaxStacks=1});c.Add(new StatusDefinition{Id="status.intent_delay",Category="tactical",TickTiming="enemy_response",StackRule="refresh",EffectId="intent_delay",DefaultDuration=1,MaxStacks=1});c.Add(new StatusDefinition{Id="status.curse",Category="persistent",TickTiming="floor_action",StackRule="unique",EffectId="recharge_drain",DefaultDuration=5,MaxStacks=1});
            c.Add(new TrapDefinition{Id="trap.fire",Damage=2,StatusId="status.burn",StatusDuration=3,MinFloor=1});c.Add(new TrapDefinition{Id="trap.poison",Damage=1,StatusId="status.poison",StatusDuration=3,MinFloor=1});c.Add(new TrapDefinition{Id="trap.acid",Damage=2,StatusId="status.vulnerable",StatusDuration=3,MinFloor=11});c.Add(new TrapDefinition{Id="trap.freeze",Damage=1,StatusId="status.root",StatusDuration=1,MinFloor=21});c.Add(new TrapDefinition{Id="trap.pitfall",Damage=3,StatusId="",StatusDuration=0,MinFloor=31});
            c.Add(new ItemDefinition{Id="item.healing_potion",Kind="consumable",Heal=6,Price=12});c.Add(new ItemDefinition{Id="item.rusty_sword",Kind="weapon",Attack=1,Price=25});c.Add(new ItemDefinition{Id="item.iron_sword",Kind="weapon",Attack=2,Price=45});c.Add(new ItemDefinition{Id="item.steel_blade",Kind="weapon",Attack=3,Price=75});c.Add(new ItemDefinition{Id="item.leather_armor",Kind="armor",Defense=1,Price=25});c.Add(new ItemDefinition{Id="item.chainmail",Kind="armor",Defense=2,Price=50});c.Add(new ItemDefinition{Id="item.plate_mail",Kind="armor",Defense=3,Price=85});c.Add(new ItemDefinition{Id="item.trap_disarm_kit",Kind="consumable",Price=15});
            c.Add(new AffixDefinition{Id="affix.flaming",EffectId="first_damaging_hit_applies_burn",Rarity="uncommon"});c.Add(new AffixDefinition{Id="affix.frost",EffectId="first_damaging_hit_delays_intent",Rarity="uncommon"});c.Add(new AffixDefinition{Id="affix.storm",EffectId="first_damaging_hit_chains_one_damage",Rarity="rare"});c.Add(new AffixDefinition{Id="affix.vital",EffectId="increase_max_hp_or_potion_value",Rarity="uncommon"});c.Add(new AffixDefinition{Id="affix.keen",EffectId="conditional_bonus_offense",Rarity="rare"});
            c.Add(new LootTableDefinition{Id="loot.chest.standard",Entries=new[]{new LootEntryDefinition{Id="currency.gold",Weight=50},new LootEntryDefinition{Id="item.healing_potion",Weight=20},new LootEntryDefinition{Id="item.rusty_sword",Weight=15},new LootEntryDefinition{Id="item.leather_armor",Weight=15}}});
            c.Add(new LootTableDefinition{Id="loot.vault.sealed",Entries=new[]{new LootEntryDefinition{Id="item.iron_sword",Weight=30},new LootEntryDefinition{Id="item.chainmail",Weight=30},new LootEntryDefinition{Id="item.steel_blade",Weight=15},new LootEntryDefinition{Id="item.plate_mail",Weight=15},new LootEntryDefinition{Id="currency.gold.large",Weight=10}}});
            c.Add(new ShopDefinition{Id="shop.standard",StockItemIds=new[]{"item.healing_potion","item.trap_disarm_kit","item.rusty_sword","item.leather_armor"},RerollCost=8,MaxRerolls=1});
            c.Add(new MonsterVariantDefinition{Id="variant.frost",HpMultiplierBasisPoints=11000,AttackMultiplierBasisPoints=10000,BehaviorAdd="intent_delay"});c.Add(new MonsterVariantDefinition{Id="variant.storm",HpMultiplierBasisPoints=10000,AttackMultiplierBasisPoints=11000,BehaviorAdd="chain_threat"});c.Add(new MonsterVariantDefinition{Id="variant.venomous",HpMultiplierBasisPoints=10000,AttackMultiplierBasisPoints=10000,BehaviorAdd="poison"});c.Add(new MonsterVariantDefinition{Id="variant.ember",HpMultiplierBasisPoints=10000,AttackMultiplierBasisPoints=11000,BehaviorAdd="burn"});c.Add(new MonsterVariantDefinition{Id="variant.shadow",HpMultiplierBasisPoints=10500,AttackMultiplierBasisPoints=10500,BehaviorAdd="threat_extension"});c.Add(new MonsterVariantDefinition{Id="variant.arcane",HpMultiplierBasisPoints=10000,AttackMultiplierBasisPoints=11000,BehaviorAdd="ability_disruption"});
            c.Progression=new ProgressionDefinition{MonsterDefeatReward=1,BossMasteryRewards=new[]{10,18,28,40,60},ForbiddenFloorMasteryBonus=2,CampaignCompletionBonus=25,AbyssDepthReward=1,AbyssMilestoneInterval=5,AbyssMilestoneBonus=2,PrestigeDepth=99};
            c.Add(new AchievementDefinition{Id="achievement.first_descent",DisplayName="First Descent",Trigger="floor.entered",MinimumValue=2});c.Add(new AchievementDefinition{Id="achievement.forbidden_path",DisplayName="Forbidden Path",Trigger="floor.entered.forbidden"});c.Add(new AchievementDefinition{Id="achievement.vault_keeper",DisplayName="Vault Keeper",Trigger="vault.opened"});c.Add(new AchievementDefinition{Id="achievement.campaign_complete",DisplayName="Against the Depths",Trigger="campaign.completed"});c.Add(new AchievementDefinition{Id="achievement.depth_99",DisplayName="Ninety-Nine",Trigger="abyss.depth.entered",MinimumValue=99});
            c.AddContentMigration("monster.old_frost_rat","monster.rat");c.AddContentMigration("item.health_potion","item.healing_potion");
            return c;
        }

        private static void AddAbilities(GameContent c,HeroClassId cls,(string id,int max,int recharge)[] values){int[] thresholds={0,24,56,96,150};int i=0;foreach(var v in values){c.Add(new AbilityDefinition{Id=v.id,ClassId=cls,MaxCharges=v.max,RechargeProgressRequired=v.recharge,UnlockMastery=thresholds[Math.Min(i,thresholds.Length-1)]});i++;}}
        private static void AddMonster(GameContent c,string id,int hp,int attack,int defense,ThreatPattern threat,MonsterIntentKind intent,int power,string behavior,string[] biomes){c.Add(new MonsterDefinition{Id=id,Hp=hp,Attack=attack,Defense=defense,ThreatPattern=threat,PrimaryIntent=intent,IntentPower=power,BehaviorId=behavior,BiomeIds=biomes});}
        private static void AddArchetype(GameContent c,string id,int weight,int monsterDelta=0,int? monsterCount=null,int trapDelta=0,int? trapCount=null,int chestDelta=0,int goldDelta=0,int shrineCount=-1,int merchantCount=0,bool forced=false){c.Add(new FloorArchetypeDefinition{Id=id,Weight=weight,MonsterDelta=monsterDelta,MonsterCount=monsterCount,TrapDelta=trapDelta,TrapCount=trapCount,ChestDelta=chestDelta,GoldDelta=goldDelta,ShrineCount=shrineCount,MerchantCount=merchantCount,ForcedOnBossFloor=forced});}
    }
}
