using System;
using System.Collections.Generic;
using System.Linq;
using ClickDungeon.Simulation.Combat;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Randomness;

namespace ClickDungeon.Simulation.Generation
{
    public sealed class FloorGenerator
    {
        private readonly GameContent _content;
        public GameContent Content => _content;
        public FloorGenerator(GameContent content=null) { _content=content??GameContent.CreateDevelopmentFallback(); }

        public RunState CreateNewRun(uint rootSeed, HeroClassId heroClass, IReadOnlyCollection<string> unlockedAbilityIds=null)
        {
            var state = new RunState { RootSeed = rootSeed, HeroClass = heroClass };
            ConfigureHero(state, heroClass, unlockedAbilityIds);
            state.InventoryItemIds.Add("item.healing_potion");
            GenerateFloor(state, 1, RouteModifier.Standard);
            return state;
        }

        public RunState CreateAbyssRun(uint rootSeed,HeroClassId heroClass,IReadOnlyCollection<string> unlockedAbilityIds=null)
        {
            var state=new RunState{RootSeed=rootSeed,HeroClass=heroClass,Mode=RunMode.Abyss,AbyssDepth=1,Floor=_content.Balance.CampaignFloors+1};
            ConfigureHero(state,heroClass,unlockedAbilityIds);
            state.InventoryItemIds.Add("item.healing_potion");
            GenerateFloor(state,state.Floor,RouteModifier.Standard);
            return state;
        }

        public void GenerateFloor(RunState state, int floor, RouteModifier route)
        {
            state.Floor = floor;
            state.RouteModifier = route;
            state.BiomeId = state.Mode==RunMode.Abyss?_content.BiomeForFloor(((Math.Max(1,floor-_content.Balance.CampaignFloors)-1)%_content.Balance.CampaignFloors)+1):_content.BiomeForFloor(floor);
            state.ArchetypeId = SelectArchetype(state, floor);
            state.FloorSeed = SeedDerivation.Derive(state.RootSeed, $"floor:{floor}:{route}:{state.ArchetypeId}");
            var rng = new XorShift32(state.FloorSeed);
            string bossId=BossIdForState(state,floor);state.BossRequired=!string.IsNullOrEmpty(bossId);
            state.BossDefeated = !state.BossRequired;
            state.Tiles = BuildPool(state, rng);
            FisherYates(state.Tiles, rng);

            const int start = 12;
            var displaced = state.Tiles[start];
            state.Tiles[start] = new TileState { Content = TileContentKind.Empty, ContentId = "tile.start", Visibility = TileVisibility.Revealed, Resolution = TileResolution.Resolved, Occupancy = OccupancyKind.Player };
            ReplaceFirstEmpty(state.Tiles, displaced, start);
            for (int i = 0; i < state.Tiles.Count; i++) state.Tiles[i].Index = i;
            state.PlayerPosition = new GridPosition(2, 2);
            InitializeMonsters(state);
            ApplyTerrain(state, rng);
            ApplyClues(state, rng);
            ApplyClassKnowledge(state, rng);
            state.FloorRngState = rng.State;
        }

        private List<TileState> BuildPool(RunState state, IRandomSource rng)
        {
            var list = new List<TileState>(25);
            int monsterCount=3, trapCount=2, chestCount=2, goldCount=4, shrineCount=1, merchantCount=0;
            var archetype=_content.Archetype(state.ArchetypeId);
            monsterCount=archetype.MonsterCount??Math.Max(0,monsterCount+archetype.MonsterDelta);
            trapCount=archetype.TrapCount??Math.Max(0,trapCount+archetype.TrapDelta);
            chestCount=Math.Max(0,chestCount+archetype.ChestDelta); goldCount=Math.Max(0,goldCount+archetype.GoldDelta);
            if(archetype.ShrineCount>=0)shrineCount=archetype.ShrineCount; merchantCount=archetype.MerchantCount;
            if(state.RouteModifier==RouteModifier.Forbidden){monsterCount+=_content.Balance.ForbiddenMonsterDelta;trapCount+=_content.Balance.ForbiddenTrapDelta;goldCount+=1;}
            int goldAmount=state.RouteModifier==RouteModifier.Forbidden?Math.Max(8,8*_content.Balance.ForbiddenGoldMultiplierBasisPoints/10000):8;
            Add(list,TileContentKind.Gold,"currency.gold",goldCount,goldAmount);
            string[] pool=_content.MonsterIdsForBiome(state.BiomeId);
            for(int i=0;i<monsterCount;i++)list.Add(NewTile(TileContentKind.Monster,pool[rng.NextInt(pool.Length)]));
            string[] trapPool=_content.TrapIdsForFloor(state.Floor);for(int i=0;i<trapCount;i++)list.Add(NewTile(TileContentKind.Trap,trapPool[rng.NextInt(trapPool.Length)]));
            Add(list,TileContentKind.Chest,"chest.standard",chestCount);Add(list,TileContentKind.Shrine,"shrine.choice",shrineCount);Add(list,TileContentKind.SmallKey,"key.small",2);Add(list,TileContentKind.BigKey,"key.big",1);Add(list,TileContentKind.SealedVault,"vault.sealed",1);Add(list,TileContentKind.SafeExit,"exit.safe",1);Add(list,TileContentKind.ForbiddenExit,"exit.forbidden",1);
            Add(list,TileContentKind.Merchant,"merchant.standard",merchantCount);
            if(state.BossRequired)list.Add(NewTile(TileContentKind.Boss,BossIdForState(state,state.Floor)));
            while(list.Count<25)list.Add(NewTile(TileContentKind.Empty,"tile.empty"));while(list.Count>25)RemoveLowestPriority(list);return list;
        }

        private static void RemoveLowestPriority(List<TileState> list)
        {
            var removable = new[]{TileContentKind.Empty,TileContentKind.Gold,TileContentKind.Chest,TileContentKind.Trap,TileContentKind.Monster};
            foreach(var kind in removable)
            {
                int index=list.FindLastIndex(t=>t.Content==kind);
                if(index>=0){ list.RemoveAt(index); return; }
            }
            list.RemoveAt(list.Count-1);
        }

        private void InitializeMonsters(RunState state)
        {
            foreach(var tile in state.Tiles)
            {
                if(tile.Content!=TileContentKind.Monster && tile.Content!=TileContentKind.Boss) continue;
                var def=_content.Monster(tile.ContentId);
                int floorScale=Math.Max(0,(state.Floor-1)/5)+(state.Mode==RunMode.Abyss?Math.Max(0,state.AbyssDepth/5):0);
                int routeBonus=state.RouteModifier==RouteModifier.Forbidden?2:0;
                tile.MonsterMaxHp=def.Hp+floorScale+routeBonus;
                tile.MonsterHp=tile.MonsterMaxHp;
                tile.MonsterAttack=def.Attack+floorScale/2+(state.RouteModifier==RouteModifier.Forbidden?1:0);
                tile.MonsterDefense=def.Defense+floorScale/3;
                tile.ThreatPattern=def.ThreatPattern;
                ApplyVariant(state,tile);
                uint eliteSeed=SeedDerivation.Derive(state.FloorSeed,$"elite:{tile.Index}:{tile.ContentId}");
                tile.IsElite=state.RouteModifier==RouteModifier.Forbidden && new XorShift32(eliteSeed).ChanceBasisPoints(_content.Balance.ForbiddenEliteChanceBasisPoints);
                if(tile.IsElite){ tile.MonsterMaxHp+=3; tile.MonsterHp+=3; tile.MonsterAttack+=1; }
                if(state.BiomeId=="biome.ash_wastes")tile.MonsterAttack+=1;
                MonsterIntentResolver.Initialize(tile,def);
            }
        }

        private void ApplyVariant(RunState state,TileState tile)
        {
            string variantId=PreferredVariantForBiome(state.BiomeId);
            if(string.IsNullOrEmpty(variantId)||!_content.TryVariant(variantId,out var variant))return;
            uint seed=SeedDerivation.Derive(state.FloorSeed,$"variant:{tile.Index}:{tile.ContentId}");
            if(!new XorShift32(seed).ChanceBasisPoints(state.RouteModifier==RouteModifier.Forbidden?4000:2000))return;
            tile.VariantId=variant.Id;
            tile.MonsterMaxHp=Math.Max(1,tile.MonsterMaxHp*variant.HpMultiplierBasisPoints/10000);tile.MonsterHp=tile.MonsterMaxHp;
            tile.MonsterAttack=Math.Max(1,tile.MonsterAttack*variant.AttackMultiplierBasisPoints/10000);
            if(variant.BehaviorAdd=="chain_threat")tile.ThreatPattern=ThreatPattern.CrossTwo;
            else if(variant.BehaviorAdd=="threat_extension")tile.ThreatPattern=ThreatPattern.AuraTwo;
        }

        private static string PreferredVariantForBiome(string biomeId)
        {
            switch(biomeId)
            {
                case "biome.crypt":return "variant.shadow";
                case "biome.sunken_temple":case "biome.mire":case "biome.thorn_wilds":return "variant.venomous";
                case "biome.frozen_ruins":return "variant.frost";
                case "biome.storm_plateau":return "variant.storm";
                case "biome.lava_field":return "variant.ember";
                case "biome.arcane_nexus":return "variant.arcane";
                case "biome.ash_wastes":return "variant.shadow";
                default:return string.Empty;
            }
        }

        private static void Add(List<TileState> list,TileContentKind kind,string id,int count,int amount=0){for(int i=0;i<count;i++){var t=NewTile(kind,id);t.Amount=amount;list.Add(t);}}
        private static TileState NewTile(TileContentKind kind,string id)=>new TileState{Content=kind,ContentId=id,Amount=kind==TileContentKind.Gold?8:0};
        private static void FisherYates(List<TileState> list,IRandomSource rng){for(int i=list.Count-1;i>0;i--){int j=rng.NextInt(i+1);var tmp=list[i];list[i]=list[j];list[j]=tmp;}}
        private static void ReplaceFirstEmpty(List<TileState> list,TileState displaced,int excluded){for(int i=0;i<list.Count;i++)if(i!=excluded&&list[i].Content==TileContentKind.Empty){list[i]=displaced;return;} for(int i=0;i<list.Count;i++)if(i!=excluded&&list[i].Content==TileContentKind.Gold){list[i]=displaced;return;}}


        private static void ApplyTerrain(RunState state,IRandomSource rng)
        {
            TerrainKind terrain;int count;
            switch(state.BiomeId)
            {
                case "biome.crypt":terrain=TerrainKind.Grave;count=4;break;
                case "biome.sunken_temple":terrain=TerrainKind.Flooded;count=6;break;
                case "biome.thorn_wilds":terrain=TerrainKind.Thorn;count=5;break;
                case "biome.mire":terrain=TerrainKind.Mire;count=5;break;
                case "biome.frozen_ruins":terrain=TerrainKind.Ice;count=6;break;
                case "biome.storm_plateau":terrain=TerrainKind.Charged;count=6;break;
                case "biome.lava_field":terrain=TerrainKind.Lava;count=5;break;
                case "biome.arcane_nexus":terrain=TerrainKind.Arcane;count=5;break;
                case "biome.ash_wastes":terrain=TerrainKind.Ash;count=6;break;
                default:return;
            }
            var candidates=Enumerable.Range(0,state.Tiles.Count).Where(i=>i!=12&&state.Tiles[i].Content!=TileContentKind.SafeExit&&state.Tiles[i].Content!=TileContentKind.ForbiddenExit&&state.Tiles[i].Content!=TileContentKind.Boss).ToList();
            for(int n=0;n<count&&candidates.Count>0;n++){int pick=rng.NextInt(candidates.Count);int i=candidates[pick];candidates.RemoveAt(pick);state.Tiles[i].Terrain=terrain;}
        }

        private static void ApplyClues(RunState state,IRandomSource rng)
        {
            foreach(var tile in state.Tiles){if(tile.Visibility==TileVisibility.Revealed)continue;var clue=ClueFor(tile.Content);if(clue!=ClueFamily.None&&rng.ChanceBasisPoints(6500)){tile.Clue=clue;tile.Visibility=TileVisibility.Clued;}}
        }

        private static void ApplyClassKnowledge(RunState state,IRandomSource rng)
        {
            if(state.HeroClass==HeroClassId.Thief)
            {
                foreach(var tile in state.Tiles) if(tile.Content==TileContentKind.Trap && tile.Visibility==TileVisibility.Clued) tile.Visibility=TileVisibility.Identified;
            }
            else if(state.HeroClass==HeroClassId.Ranger)
            {
                var candidates=state.Tiles.Where(t=>t.Visibility==TileVisibility.Hidden && ClueFor(t.Content)!=ClueFamily.None).ToList();
                if(candidates.Count>0){var tile=candidates[rng.NextInt(candidates.Count)];tile.Clue=ClueFor(tile.Content);tile.Visibility=TileVisibility.Clued;}
            }
        }

        private static ClueFamily ClueFor(TileContentKind kind)
        {
            switch(kind)
            {
                case TileContentKind.Monster: case TileContentKind.Boss: case TileContentKind.Trap:return ClueFamily.Danger;
                case TileContentKind.Gold:case TileContentKind.Equipment:case TileContentKind.Consumable:case TileContentKind.Shrine:case TileContentKind.Chest:case TileContentKind.SmallKey:case TileContentKind.BigKey:case TileContentKind.SealedVault:case TileContentKind.Merchant:return ClueFamily.Opportunity;
                case TileContentKind.SafeExit:case TileContentKind.ForbiddenExit:case TileContentKind.SpecialEvent:return ClueFamily.PassageArcane;
                default:return ClueFamily.None;
            }
        }

        private string BossIdForState(RunState state,int floor)
        {
            if(state.Mode==RunMode.Campaign)return _content.BossForFloor(floor);
            int depth=Math.Max(1,state.AbyssDepth);if(depth%10!=0)return string.Empty;
            int cycle=((depth/10)-1)%5;return _content.BossForFloor((cycle+1)*10);
        }

        private string SelectArchetype(RunState state,int floor)
        {
            var forced=_content.Archetypes.FirstOrDefault(a=>a.ForcedOnBossFloor && !string.IsNullOrEmpty(BossIdForState(state,floor))); if(forced!=null)return forced.Id;
            var pool=_content.Archetypes.Where(a=>!a.ForcedOnBossFloor&&a.Weight>0).OrderBy(a=>a.Id,StringComparer.Ordinal).ToArray(); if(pool.Length==0)return "archetype.standard";
            int total=pool.Sum(a=>a.Weight);var rng=new XorShift32(SeedDerivation.Derive(state.RootSeed,$"archetype:{state.Mode}:{floor}:{state.AbyssDepth}"));int roll=rng.NextInt(Math.Max(1,total));foreach(var a in pool){if(roll<a.Weight)return a.Id;roll-=a.Weight;}return pool[pool.Length-1].Id;
        }

        private void ConfigureHero(RunState state,HeroClassId cls,IReadOnlyCollection<string> unlocked)
        {
            var hero=_content.Hero(cls);state.MaxHp=hero.BaseHp;state.Hp=hero.BaseHp;state.Attack=hero.BaseAttack;state.Defense=hero.BaseDefense;state.AbilityStates.Clear();
            var ids=unlocked!=null&&unlocked.Count>0?new List<string>(unlocked):new List<string>{hero.AbilityIds[0]};
            foreach(string id in ids){var def=_content.Ability(id);state.AbilityStates.Add(new AbilityChargeState{AbilityId=id,Charges=def.MaxCharges});}
        }
    }
}
