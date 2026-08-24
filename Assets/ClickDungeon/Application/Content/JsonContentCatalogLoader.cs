using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Content
{
    public sealed class JsonContentCatalogLoader
    {
        private IReadOnlyDictionary<string,string> _documents;

        public GameContent LoadFromDirectory(string directory)
        {
            if(string.IsNullOrWhiteSpace(directory))throw new ArgumentException("Content directory is required.",nameof(directory));_documents=null;return LoadCore(directory);
        }

        public GameContent LoadFromDocuments(IReadOnlyDictionary<string,string> documents)
        {
            if(documents==null)throw new ArgumentNullException(nameof(documents));_documents=documents;return LoadCore(string.Empty);
        }

        private GameContent LoadCore(string directory)
        {
            var catalog=new GameContent();
            LoadClasses(catalog,Path.Combine(directory,"classes.json"));LoadAbilities(catalog,Path.Combine(directory,"abilities.json"));LoadMonsters(catalog,Path.Combine(directory,"monsters.json"));LoadBosses(catalog,Path.Combine(directory,"bosses.json"));LoadItems(catalog,Path.Combine(directory,"items.json"));LoadAffixes(catalog,Path.Combine(directory,"affixes.json"));LoadLootTables(catalog,Path.Combine(directory,"loot_tables.json"));LoadShops(catalog,Path.Combine(directory,"shops.json"));LoadVariants(catalog,Path.Combine(directory,"monster_variants.json"));LoadStatuses(catalog,Path.Combine(directory,"statuses.json"));LoadTraps(catalog,Path.Combine(directory,"traps.json"));LoadBiomes(catalog,Path.Combine(directory,"biomes.json"));LoadArchetypes(catalog,Path.Combine(directory,"floor_archetypes.json"));LoadProgression(catalog,Path.Combine(directory,"progression.json"));LoadAchievements(catalog,Path.Combine(directory,"achievements.json"));LoadMigrations(catalog,Path.Combine(directory,"content_migrations.json"));LoadBalance(catalog,Path.Combine(directory,"balance.json"));return catalog;
        }

        private JObject Read(string path)
        {
            string name=Path.GetFileName(path);if(_documents!=null){if(!_documents.TryGetValue(name,out var json))throw new FileNotFoundException("Generated content document missing.",name);return JObject.Parse(json);}if(!File.Exists(path))throw new FileNotFoundException("Required content file missing.",path);return JObject.Parse(File.ReadAllText(path));
        }

        private void LoadClasses(GameContent c,string path)
        {
            foreach(var row in Read(path)["classes"] as JArray ?? new JArray())
            {
                var cls=ParseClass(row.Value<string>("id"));
                c.Add(new HeroDefinition{ClassId=cls,DisplayName=row.Value<string>("display_name")??cls.ToString(),Identity=row.Value<string>("identity")??string.Empty,BoardPassive=row.Value<string>("board_passive")??string.Empty,BaseHp=row.Value<int>("base_hp"),BaseAttack=row.Value<int>("base_attack"),BaseDefense=row.Value<int>("base_defense"),AbilityIds=new string[0]});
            }
        }

        private void LoadAbilities(GameContent c,string path)
        {
            var byClass=new System.Collections.Generic.Dictionary<HeroClassId,System.Collections.Generic.List<string>>();
            foreach(var row in Read(path)["abilities"] as JArray ?? new JArray())
            {
                var cls=ParseClass(row.Value<string>("class_id")); string id=row.Value<string>("id");
                c.Add(new AbilityDefinition{Id=id,DisplayName=row.Value<string>("display_name")??HumanizeId(id),Role=row.Value<string>("role")??string.Empty,EffectText=row.Value<string>("effect")??string.Empty,ClassId=cls,MaxCharges=row.Value<int>("max_charges"),RechargeProgressRequired=row.Value<int>("recharge_progress_required"),UnlockMastery=row.Value<int?>("unlock_mastery")??0});
                if(!byClass.TryGetValue(cls,out var list)){list=new System.Collections.Generic.List<string>();byClass[cls]=list;} list.Add(id);
            }
            foreach(var pair in byClass) c.Hero(pair.Key).AbilityIds=pair.Value.ToArray();
        }

        private void LoadMonsters(GameContent c,string path)
        {
            foreach(var row in Read(path)["monsters"] as JArray ?? new JArray()) AddMonster(c,row);
        }

        private void LoadBosses(GameContent c,string path)
        {
            foreach(var row in Read(path)["bosses"] as JArray ?? new JArray())
            {
                string id=row.Value<string>("id");
                c.Add(new MonsterDefinition{Id=id,DisplayName=row.Value<string>("display_name")??HumanizeId(id),Decision=row.Value<string>("decision")??string.Empty,Hp=row.Value<int>("hp"),Attack=row.Value<int>("attack"),Defense=row.Value<int>("defense"),ThreatPattern=BossThreat(id),PrimaryIntent=BossIntent(id),IntentPower=row.Value<int>("attack"),BehaviorId=row.Value<string>("identity")??"boss",BiomeIds=new string[0]});
                c.Add(new BossDefinition{Id=id,Floor=row.Value<int>("floor")});
            }
        }

        private void LoadItems(GameContent c,string path)
        {
            foreach(var row in Read(path)["items"] as JArray ?? new JArray())
            {
                c.Add(new ItemDefinition{Id=row.Value<string>("id"),DisplayName=row.Value<string>("display_name")??HumanizeId(row.Value<string>("id")),Rarity=row.Value<string>("rarity")??string.Empty,Kind=row.Value<string>("type")??string.Empty,Attack=row.Value<int?>("attack")??0,Defense=row.Value<int?>("defense")??0,Heal=row.Value<int?>("heal")??0,Price=row.Value<int?>("price")??0});
            }
        }

        private void LoadAffixes(GameContent c,string path)
        {
            foreach(var row in Read(path)["affixes"] as JArray ?? new JArray())
                c.Add(new AffixDefinition{Id=row.Value<string>("id"),DisplayName=row.Value<string>("display_name")??HumanizeId(row.Value<string>("id")),EffectId=row.Value<string>("effect")??string.Empty,Rarity=row.Value<string>("rarity")??string.Empty});
        }

        private void LoadLootTables(GameContent c,string path)
        {
            foreach(var row in Read(path)["tables"] as JArray ?? new JArray())
            {
                var entries=(row["entries"] as JArray ?? new JArray()).Select(e=>new LootEntryDefinition{Id=e.Value<string>("id"),Weight=e.Value<int?>("weight")??1}).ToArray();
                c.Add(new LootTableDefinition{Id=row.Value<string>("id"),Entries=entries});
            }
        }

        private void LoadShops(GameContent c,string path)
        {
            foreach(var row in Read(path)["shops"] as JArray ?? new JArray())
            {
                var stock=row["stock"] as JArray;
                c.Add(new ShopDefinition{Id=row.Value<string>("id"),StockItemIds=stock==null?new string[0]:stock.Values<string>().ToArray(),RerollCost=row.Value<int?>("reroll_cost")??0,MaxRerolls=row.Value<int?>("max_rerolls")??0});
            }
        }

        private void LoadVariants(GameContent c,string path)
        {
            foreach(var row in Read(path)["variants"] as JArray ?? new JArray())
            {
                c.Add(new MonsterVariantDefinition{Id=row.Value<string>("id"),Affinity=row.Value<string>("affinity")??string.Empty,EffectId=row.Value<string>("effect")??string.Empty,HpMultiplierBasisPoints=row.Value<int?>("hp_multiplier_bp")??10000,AttackMultiplierBasisPoints=row.Value<int?>("attack_multiplier_bp")??10000,BehaviorAdd=row.Value<string>("behavior_add")??string.Empty});
            }
        }

        private static void AddMonster(GameContent c,JToken row)
        {
            var biomeArray=row["biomes"] as JArray;
            c.Add(new MonsterDefinition{Id=row.Value<string>("id"),DisplayName=row.Value<string>("display_name")??HumanizeId(row.Value<string>("id")),Decision=row.Value<string>("decision")??string.Empty,Hp=row.Value<int>("hp"),Attack=row.Value<int>("attack"),Defense=row.Value<int>("defense"),ThreatPattern=ParseThreat(row.Value<string>("threat")),PrimaryIntent=ParseIntent(row.Value<string>("intent")),IntentPower=row.Value<int?>("intent_power")??row.Value<int>("attack"),BehaviorId=row.Value<string>("behavior")??string.Empty,BiomeIds=biomeArray==null?new string[0]:biomeArray.Values<string>().ToArray()});
        }



        private void LoadStatuses(GameContent c,string path)
        {
            foreach(var row in Read(path)["statuses"] as JArray ?? new JArray())
            {
                c.Add(new StatusDefinition{Id=row.Value<string>("id"),Category=row.Value<string>("category")??string.Empty,TickTiming=row.Value<string>("tick_timing")??string.Empty,StackRule=row.Value<string>("stack_rule")??string.Empty,EffectId=row.Value<string>("effect")??string.Empty,DefaultDuration=row.Value<int?>("default_duration")??1,MaxStacks=row.Value<int?>("max_stacks")??1});
            }
        }

        private void LoadProgression(GameContent c,string path)
        {
            var root=Read(path);var mastery=root["class_mastery"] as JObject;var campaign=root["campaign"] as JObject;var boss=mastery?["boss_mastery_rewards"] as JArray;
            c.Progression=new ProgressionDefinition{MonsterDefeatReward=mastery?.Value<int?>("monster_defeat_reward")??1,BossMasteryRewards=boss==null?new int[0]:boss.Values<int>().ToArray(),ForbiddenFloorMasteryBonus=mastery?.Value<int?>("forbidden_floor_mastery_bonus")??2,CampaignCompletionBonus=mastery?.Value<int?>("campaign_completion_bonus")??25,AbyssDepthReward=mastery?.Value<int?>("abyss_depth_reward")??1,AbyssMilestoneInterval=mastery?.Value<int?>("abyss_milestone_interval")??5,AbyssMilestoneBonus=mastery?.Value<int?>("abyss_milestone_bonus")??2,PrestigeDepth=campaign?.Value<int?>("prestige_depth")??99};
        }

        private void LoadAchievements(GameContent c,string path)
        {
            foreach(var row in Read(path)["achievements"] as JArray ?? new JArray())c.Add(new AchievementDefinition{Id=row.Value<string>("id"),DisplayName=row.Value<string>("display_name")??row.Value<string>("name")??HumanizeId(row.Value<string>("id")),Trigger=row.Value<string>("trigger")??string.Empty,MinimumValue=row.Value<int?>("minimum_value")??0});
        }

        private void LoadMigrations(GameContent c,string path)
        {
            var migrations=Read(path)["migrations"] as JObject;if(migrations==null)return;foreach(var property in migrations.Properties())c.AddContentMigration(property.Name,property.Value.Value<string>());
        }

        private void LoadTraps(GameContent c,string path)
        {
            foreach(var row in Read(path)["traps"] as JArray ?? new JArray()) c.Add(new TrapDefinition{Id=row.Value<string>("id"),Damage=row.Value<int?>("damage")??0,StatusId=row.Value<string>("status")??string.Empty,StatusDuration=row.Value<int?>("status_duration")??0,MinFloor=row.Value<int?>("min_floor")??1});
        }

        private void LoadBiomes(GameContent c,string path)
        {
            foreach(var row in Read(path)["biomes"] as JArray ?? new JArray())
            {
                var floors=row["floors"] as JArray; if(floors==null||floors.Count<2) throw new InvalidDataException($"Biome {row.Value<string>("id")} missing floor range.");
                string id=row.Value<string>("id");c.Add(new BiomeDefinition{Id=id,DisplayName=row.Value<string>("display_name")??HumanizeId(id),FirstFloor=floors[0].Value<int>(),LastFloor=floors[1].Value<int>(),MechanicId=row.Value<string>("mechanic")??string.Empty,ClueStyle=row.Value<string>("clue_style")??string.Empty,RewardBias=row.Value<string>("reward_bias")??string.Empty,AmbienceId=row.Value<string>("ambience_id")??string.Empty});
            }
        }

        private void LoadArchetypes(GameContent c,string path)
        {
            foreach(var row in Read(path)["archetypes"] as JArray ?? new JArray())
            {
                c.Add(new FloorArchetypeDefinition{Id=row.Value<string>("id"),Weight=row.Value<int?>("weight")??1,MonsterDelta=row.Value<int?>("monster_delta")??0,MonsterCount=row.Value<int?>("monster_count"),TrapDelta=row.Value<int?>("trap_delta")??0,TrapCount=row.Value<int?>("trap_count"),ChestDelta=row.Value<int?>("chest_delta")??0,GoldDelta=row.Value<int?>("gold_delta")??0,ShrineCount=row.Value<int?>("shrine_count")??-1,MerchantCount=row.Value<int?>("merchant_count")??0,ForcedOnBossFloor=row.Value<bool?>("forced_on_boss_floor")??false});
            }
        }

        private void LoadBalance(GameContent c,string path)
        {
            var row=Read(path); var forbidden=row["forbidden_route"] as JObject; c.Balance=new BalanceDefinition{CampaignFloors=row.Value<int?>("campaign_floors")??50,BigKeyMaxCarry=row.Value<int?>("big_key_max_carry")??2,ForbiddenMonsterDelta=forbidden?.Value<int?>("monster_delta")??2,ForbiddenTrapDelta=forbidden?.Value<int?>("trap_delta")??1,ForbiddenEliteChanceBasisPoints=forbidden?.Value<int?>("elite_chance_bp")??2500,ForbiddenGoldMultiplierBasisPoints=forbidden?.Value<int?>("gold_multiplier_bp")??15000};
        }

        private static string HumanizeId(string id){if(string.IsNullOrEmpty(id))return string.Empty;int dot=id.LastIndexOf('.');string value=(dot>=0?id.Substring(dot+1):id).Replace('_',' ');return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value);}
        private static HeroClassId ParseClass(string id){switch(id){case "class.ranger":return HeroClassId.Ranger;case "class.thief":return HeroClassId.Thief;case "class.wizard":return HeroClassId.Wizard;default:return HeroClassId.Knight;}}
        private static ThreatPattern ParseThreat(string id){switch(id){case "cross_two":return ThreatPattern.CrossTwo;case "orthogonal_line":return ThreatPattern.OrthogonalLine;case "aura_two":return ThreatPattern.AuraTwo;case "adjacent":return ThreatPattern.Adjacent;default:return ThreatPattern.None;}}
        private static MonsterIntentKind ParseIntent(string id){switch(id){case "heavy_attack":return MonsterIntentKind.HeavyAttack;case "steal_gold":return MonsterIntentKind.StealGold;case "poison":return MonsterIntentKind.ApplyPoison;case "guard":return MonsterIntentKind.Guard;case "summon":return MonsterIntentKind.Summon;case "hazard":return MonsterIntentKind.Hazard;default:return MonsterIntentKind.Attack;}}
        private static ThreatPattern BossThreat(string id)=>id.Contains("wyrm")?ThreatPattern.OrthogonalLine:id.Contains("lich")?ThreatPattern.AuraTwo:ThreatPattern.CrossTwo;
        private static MonsterIntentKind BossIntent(string id)=>id.Contains("lich")?MonsterIntentKind.Summon:id.Contains("wyrm")?MonsterIntentKind.HeavyAttack:MonsterIntentKind.Hazard;
    }
}
