using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Simulation.Content
{
    public sealed class HeroDefinition
    {
        public HeroClassId ClassId;
        public string DisplayName = string.Empty;
        public string ClassDisplayName => ClassId.ToString();
        public string Identity = string.Empty;
        public string BoardPassive = string.Empty;
        public int BaseHp;
        public int BaseAttack;
        public int BaseDefense;
        public string[] AbilityIds = new string[0];
    }

    public sealed class AbilityDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Role = string.Empty;
        public string EffectText = string.Empty;
        public HeroClassId ClassId;
        public int MaxCharges;
        public int RechargeProgressRequired;
        public int UnlockMastery;
    }

    public sealed class MonsterDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Decision = string.Empty;
        public int Hp;
        public int Attack;
        public int Defense;
        public ThreatPattern ThreatPattern;
        public MonsterIntentKind PrimaryIntent;
        public int IntentPower;
        public string BehaviorId = string.Empty;
        public string[] BiomeIds = new string[0];
    }

    public sealed class ItemDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Rarity = string.Empty;
        public string Kind = string.Empty;
        public int Attack;
        public int Defense;
        public int Heal;
        public int Price;
    }


    public sealed class AffixDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string EffectId = string.Empty;
        public string Rarity = string.Empty;
    }

    public sealed class LootEntryDefinition
    {
        public string Id = string.Empty;
        public int Weight = 1;
    }

    public sealed class LootTableDefinition
    {
        public string Id = string.Empty;
        public LootEntryDefinition[] Entries = new LootEntryDefinition[0];
    }

    public sealed class ShopDefinition
    {
        public string Id = string.Empty;
        public string[] StockItemIds = new string[0];
        public int RerollCost;
        public int MaxRerolls;
    }

    public sealed class MonsterVariantDefinition
    {
        public string Id = string.Empty;
        public string Affinity = string.Empty;
        public string EffectId = string.Empty;
        public int HpMultiplierBasisPoints = 10000;
        public int AttackMultiplierBasisPoints = 10000;
        public string BehaviorAdd = string.Empty;
    }

    public sealed class TrapDefinition
    {
        public string Id = string.Empty;
        public int Damage;
        public string StatusId = string.Empty;
        public int StatusDuration;
        public int MinFloor = 1;
    }

    public sealed class BiomeDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string ClueStyle = string.Empty;
        public string RewardBias = string.Empty;
        public string AmbienceId = string.Empty;
        public int FirstFloor;
        public int LastFloor;
        public string MechanicId = string.Empty;
    }

    public sealed class FloorArchetypeDefinition
    {
        public string Id = string.Empty;
        public int Weight;
        public int MonsterDelta;
        public int? MonsterCount;
        public int TrapDelta;
        public int? TrapCount;
        public int ChestDelta;
        public int GoldDelta;
        public int ShrineCount = -1;
        public int MerchantCount;
        public bool ForcedOnBossFloor;
    }

    public sealed class BossDefinition
    {
        public string Id = string.Empty;
        public int Floor;
    }


    public sealed class StatusDefinition
    {
        public string Id = string.Empty;
        public string Category = string.Empty;
        public string TickTiming = string.Empty;
        public string StackRule = string.Empty;
        public string EffectId = string.Empty;
        public int DefaultDuration = 1;
        public int MaxStacks = 1;
    }

    public sealed class AchievementDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Trigger = string.Empty;
        public int MinimumValue;
    }

    public sealed class ProgressionDefinition
    {
        public int MonsterDefeatReward = 1;
        public int[] BossMasteryRewards = new int[0];
        public int ForbiddenFloorMasteryBonus = 2;
        public int CampaignCompletionBonus = 25;
        public int AbyssDepthReward = 1;
        public int AbyssMilestoneInterval = 5;
        public int AbyssMilestoneBonus = 2;
        public int PrestigeDepth = 99;

        public int BossRewardForFloor(int floor)
        {
            if(BossMasteryRewards==null||BossMasteryRewards.Length==0)return 10;
            int index=System.Math.Max(0,System.Math.Min(BossMasteryRewards.Length-1,floor/10-1));
            return BossMasteryRewards[index];
        }
    }

    public sealed class BalanceRangeDefinition
    {
        public int Min;
        public int Max;
    }

    public sealed class PowerEnvelopeDefinition
    {
        public int Floor;
        public int Hp;
        public int Attack;
        public int Defense;
        public int ItemTier;
    }

    public sealed class BalanceDefinition
    {
        public int CampaignFloors = 50;
        public int BoardSize = RunState.BoardSize;
        public int BigKeyMaxCarry = 2;
        public BalanceRangeDefinition TargetFloorSeconds = new BalanceRangeDefinition();
        public BalanceRangeDefinition TargetCampaignMinutes = new BalanceRangeDefinition();
        public BalanceRangeDefinition NormalEncounterDecisions = new BalanceRangeDefinition();
        public BalanceRangeDefinition EliteEncounterDecisions = new BalanceRangeDefinition();
        public BalanceRangeDefinition BossEncounterDecisions = new BalanceRangeDefinition();
        public int ForbiddenMonsterDelta = 2;
        public int ForbiddenTrapDelta = 1;
        public int ForbiddenEliteChanceBasisPoints = 2500;
        public int ForbiddenGoldMultiplierBasisPoints = 15000;
        public int ForbiddenRareRewardMultiplierBasisPoints = 16000;
        public PowerEnvelopeDefinition[] PowerEnvelopes = new PowerEnvelopeDefinition[0];
    }
}
