using System;
using System.Collections.Generic;
using System.Linq;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Randomness;
using ClickDungeon.Simulation.Rules;

namespace ClickDungeon.Simulation.Balance
{
    public enum BalancePolicy { RandomLegal, GreedyLoot, Cautious, HardRoute }

    public sealed class BalanceCohortMetrics
    {
        public HeroClassId HeroClass;
        public BalancePolicy Policy;
        public int Runs;
        public int Deaths;
        public int CampaignCompletions;
        public int StalledRuns;
        public int TotalCommands;
        public int TotalHighestFloor;
        public int ForbiddenExits;
        public int TotalEndingGold;
        public double CompletionRate => Runs==0?0:(double)CampaignCompletions/Runs;
        public double DeathRate => Runs==0?0:(double)Deaths/Runs;
        public double AverageHighestFloor => Runs==0?0:(double)TotalHighestFloor/Runs;
        public double AverageCommands => Runs==0?0:(double)TotalCommands/Runs;
        public double AverageEndingGold => Runs==0?0:(double)TotalEndingGold/Runs;
    }

    public sealed class BalanceBatchResult
    {
        public readonly List<BalanceCohortMetrics> Cohorts=new List<BalanceCohortMetrics>();
        public BalanceCohortMetrics Find(HeroClassId hero,BalancePolicy policy)=>Cohorts.First(c=>c.HeroClass==hero&&c.Policy==policy);
    }

    /// <summary>
    /// Deterministic automated balance probe. It is intentionally not a fun oracle: its purpose is
    /// to expose gross progression/economy/difficulty differences between classes and simple play styles.
    /// Human playtesting remains required for production balance decisions.
    /// </summary>
    public sealed class BalanceEvaluator
    {
        private readonly GameContent _content;
        public BalanceEvaluator(GameContent content){_content=content??throw new ArgumentNullException(nameof(content));}

        public BalanceBatchResult Evaluate(int runsPerCohort=25,int maxCommandsPerRun=1200,uint seedBase=1)
        {
            if(runsPerCohort<=0)throw new ArgumentOutOfRangeException(nameof(runsPerCohort));
            if(maxCommandsPerRun<=0)throw new ArgumentOutOfRangeException(nameof(maxCommandsPerRun));
            var result=new BalanceBatchResult();
            foreach(HeroClassId hero in Enum.GetValues(typeof(HeroClassId)))
            foreach(BalancePolicy policy in Enum.GetValues(typeof(BalancePolicy)))
            {
                var metrics=new BalanceCohortMetrics{HeroClass=hero,Policy=policy,Runs=runsPerCohort};
                for(int runIndex=0;runIndex<runsPerCohort;runIndex++)RunOne(metrics,hero,policy,maxCommandsPerRun,seedBase+(uint)(runIndex+1)*7919u+(uint)hero*104729u+(uint)policy*15485863u);
                result.Cohorts.Add(metrics);
            }
            return result;
        }

        private void RunOne(BalanceCohortMetrics metrics,HeroClassId hero,BalancePolicy policy,int maxCommands,uint seed)
        {
            var generator=new FloorGenerator(_content);var state=generator.CreateNewRun(seed,hero);state.CampaignFloorLimit=_content.Balance.CampaignFloors;
            var session=new GameSession(state,generator,_content);var chooserRng=new XorShift32(seed^0xA511E9B3u);int highest=state.Floor;int accepted=0;bool stalled=false;
            var visitCounts=new int[RunState.BoardSize*RunState.BoardSize];int visitFloor=state.Floor;visitCounts[Index(state.PlayerPosition)]=1;
            for(int step=0;step<maxCommands&&!state.GameOver&&!state.CampaignCompleted;step++)
            {
                if(state.Floor!=visitFloor){Array.Clear(visitCounts,0,visitCounts.Length);visitFloor=state.Floor;visitCounts[Index(state.PlayerPosition)]=1;}
                var command=Choose(state,policy,chooserRng,visitCounts);if(command==null){stalled=true;break;}
                var commandResult=session.Apply(command);
                if(!commandResult.Accepted){stalled=true;break;}
                accepted++;if(command is MoveCommand)visitCounts[Index(state.PlayerPosition)]++;if(command is TakeForbiddenExitCommand)metrics.ForbiddenExits++;highest=Math.Max(highest,state.Floor);
            }
            if(!state.GameOver&&!state.CampaignCompleted&&accepted>=maxCommands)stalled=true;
            if(state.GameOver)metrics.Deaths++;if(state.CampaignCompleted)metrics.CampaignCompletions++;if(stalled&&!state.GameOver&&!state.CampaignCompleted)metrics.StalledRuns++;
            metrics.TotalCommands+=accepted;metrics.TotalHighestFloor+=highest;metrics.TotalEndingGold+=state.Gold;
        }

        private GameCommand Choose(RunState state,BalancePolicy policy,IRandomSource rng,int[] visitCounts)
        {
            var candidates=LegalCandidates(state,policy).ToList();if(candidates.Count==0)return null;
            if(policy==BalancePolicy.RandomLegal)return candidates[rng.NextInt(candidates.Count)];
            int best=int.MinValue;var bestCommands=new List<GameCommand>();
            foreach(var command in candidates)
            {
                int score=Score(state,command,policy,visitCounts);
                if(score>best){best=score;bestCommands.Clear();bestCommands.Add(command);}else if(score==best)bestCommands.Add(command);
            }
            return bestCommands[rng.NextInt(bestCommands.Count)];
        }

        private IEnumerable<GameCommand> LegalCandidates(RunState state,BalancePolicy policy)
        {
            if(state.Hp*2<state.MaxHp&&state.InventoryItemIds.Contains("item.healing_potion"))yield return new UseItemCommand("item.healing_potion");
            foreach(var command in EquipmentUpgradeCandidates(state))yield return command;
            foreach(var command in SignatureAbilityCandidates(state))yield return command;
            for(int i=0;i<state.Tiles.Count;i++)
            {
                var tile=state.Tiles[i];if(!IsAdjacent(state,i))continue;
                if((tile.Content==TileContentKind.Monster||tile.Content==TileContentKind.Boss)&&tile.Visibility==TileVisibility.Revealed&&tile.Resolution==TileResolution.Available&&tile.MonsterHp>0){yield return new AttackCommand(i);continue;}
                if(tile.Content==TileContentKind.Trap&&tile.Resolution==TileResolution.Available&&(tile.Visibility==TileVisibility.Identified||tile.Visibility==TileVisibility.Revealed)&&state.InventoryItemIds.Contains("item.trap_disarm_kit")){yield return new UseItemCommand("item.trap_disarm_kit",i);continue;}
                if(tile.Visibility!=TileVisibility.Revealed&&(state.CamouflageActions>0||!ThreatResolver.IsThreatened(state,i))){yield return new RevealTileCommand(i);continue;}
                if(tile.Visibility!=TileVisibility.Revealed)continue;
                if(tile.Resolution==TileResolution.Available)
                {
                    if(tile.Content==TileContentKind.Gold||tile.Content==TileContentKind.SmallKey||tile.Content==TileContentKind.Consumable||tile.Content==TileContentKind.Equipment)yield return new InteractCommand(i);
                    else if(tile.Content==TileContentKind.BigKey&&state.BigKeys<_content.Balance.BigKeyMaxCarry)yield return new InteractCommand(i);
                    else if(tile.Content==TileContentKind.Chest&&state.SmallKeys>0)yield return new InteractCommand(i);
                    else if(tile.Content==TileContentKind.Shrine)
                    {
                        yield return new ChooseShrineCommand(i,ShrineChoice.MaxHp);
                        yield return new ChooseShrineCommand(i,ShrineChoice.Attack);
                        yield return new ChooseShrineCommand(i,ShrineChoice.Defense);
                    }
                    else if(tile.Content==TileContentKind.SealedVault&&state.BigKeys>0&&policy!=BalancePolicy.HardRoute)yield return new UnlockVaultCommand(i);
                    else if(tile.Content==TileContentKind.Merchant)
                    {
                        if(state.Gold>=_content.Item("item.healing_potion").Price)yield return new BuyItemCommand(i,"item.healing_potion");
                    }
                }
                if(tile.Content==TileContentKind.ForbiddenExit&&tile.Visibility==TileVisibility.Revealed&&state.BigKeys>0&&!state.BossRequired&&policy==BalancePolicy.HardRoute)yield return new TakeForbiddenExitCommand(i);
                if(tile.Content==TileContentKind.SafeExit&&tile.Visibility==TileVisibility.Revealed&&!state.BossRequired)yield return new TakeSafeExitCommand(i);
                if(tile.Visibility==TileVisibility.Revealed&&tile.Occupancy!=OccupancyKind.Monster&&(state.CamouflageActions>0||state.ShieldPoints>0||!ThreatResolver.IsThreatened(state,i)))yield return new MoveCommand(i);
            }
            if(AdjacentLivingMonster(state)>=0)yield return new DefendCommand();
        }

        private IEnumerable<GameCommand> EquipmentUpgradeCandidates(RunState state)
        {
            int currentAttack=0,currentDefense=0;
            if(!string.IsNullOrEmpty(state.EquippedWeaponId)&&_content.TryItem(state.EquippedWeaponId,out var equippedWeapon))currentAttack=equippedWeapon.Attack;
            if(!string.IsNullOrEmpty(state.EquippedArmorId)&&_content.TryItem(state.EquippedArmorId,out var equippedArmor))currentDefense=equippedArmor.Defense;
            ItemInstanceState bestWeapon=null,bestArmor=null;int bestAttack=currentAttack,bestDefense=currentDefense;
            foreach(var instance in state.ItemInstances)
            {
                if(!_content.TryItem(instance.BaseItemId,out var item))continue;
                if(item.Kind=="weapon"&&item.Attack>bestAttack){bestAttack=item.Attack;bestWeapon=instance;}
                else if(item.Kind=="armor"&&item.Defense>bestDefense){bestDefense=item.Defense;bestArmor=instance;}
            }
            if(bestWeapon!=null)yield return new EquipItemCommand(bestWeapon.BaseItemId,bestWeapon.InstanceId);
            if(bestArmor!=null)yield return new EquipItemCommand(bestArmor.BaseItemId,bestArmor.InstanceId);
        }

        private IEnumerable<GameCommand> SignatureAbilityCandidates(RunState state)
        {
            var charge=state.AbilityStates.FirstOrDefault(a=>a.Charges>0);if(charge==null)yield break;
            string id=charge.AbilityId;
            if(id=="ability.knight.shield_wall")
            {
                if(state.ShieldPoints<=0&&AdjacentLivingMonster(state)>=0)yield return new UseAbilityCommand(id);
                yield break;
            }
            if(id=="ability.ranger.piercing_shot")
            {
                for(int i=0;i<state.Tiles.Count;i++)if(IsLivingMonster(state,i)&&IsSameLine(state,i))yield return new UseAbilityCommand(id,i);
                yield break;
            }
            if(id=="ability.thief.trap_scan")
            {
                for(int i=0;i<state.Tiles.Count;i++)
                {
                    var tile=state.Tiles[i];if(tile.Content==TileContentKind.Trap&&tile.Visibility!=TileVisibility.Revealed&&Manhattan(state.PlayerPosition,Position(i))<=2){yield return new UseAbilityCommand(id);yield break;}
                }
                yield break;
            }
            if(id=="ability.wizard.fireball")for(int i=0;i<state.Tiles.Count;i++)if(IsLivingMonster(state,i))yield return new UseAbilityCommand(id,i);
        }

        private int Score(RunState state,GameCommand command,BalancePolicy policy,int[] visitCounts)
        {
            if(command is EquipItemCommand)return 1150;
            if(command is UseAbilityCommand ability)
            {
                if(ability.AbilityId=="ability.knight.shield_wall")return 1125;
                if(ability.AbilityId=="ability.ranger.piercing_shot"||ability.AbilityId=="ability.wizard.fireball")return 1075;
                if(ability.AbilityId=="ability.thief.trap_scan")return 775;
            }
            if(command is AttackCommand)return policy==BalancePolicy.Cautious?1000:850;
            if(command is UseItemCommand item)return item.ItemId=="item.healing_potion"?1100:900;
            if(command is TakeForbiddenExitCommand)return policy==BalancePolicy.HardRoute?1200:-200;
            if(command is TakeSafeExitCommand)return policy==BalancePolicy.Cautious?1050:700;
            if(command is UnlockVaultCommand)return policy==BalancePolicy.GreedyLoot?1100:650;
            if(command is InteractCommand interact)
            {
                var tile=state.Tiles[interact.TileIndex];if(tile.Content==TileContentKind.BigKey)return policy==BalancePolicy.HardRoute?1050:800;if(tile.Content==TileContentKind.Chest)return policy==BalancePolicy.GreedyLoot?1000:750;if(tile.Content==TileContentKind.Gold)return policy==BalancePolicy.GreedyLoot?950:700;return 720;
            }
            if(command is ChooseShrineCommand shrine)
            {
                if(shrine.Choice==ShrineChoice.MaxHp&&state.Hp*2<state.MaxHp)return 1000;
                if(policy==BalancePolicy.Cautious)return shrine.Choice==ShrineChoice.Defense?960:shrine.Choice==ShrineChoice.MaxHp?940:850;
                if(policy==BalancePolicy.GreedyLoot)return shrine.Choice==ShrineChoice.Attack?940:shrine.Choice==ShrineChoice.MaxHp?880:840;
                if(policy==BalancePolicy.HardRoute)return shrine.Choice==ShrineChoice.Attack?980:shrine.Choice==ShrineChoice.Defense?900:880;
                return 880;
            }
            if(command is BuyItemCommand)return policy==BalancePolicy.Cautious?920:600;
            if(command is RevealTileCommand)return policy==BalancePolicy.Cautious?500:650;
            if(command is MoveCommand move)
            {
                int visits=visitCounts!=null&&move.TileIndex>=0&&move.TileIndex<visitCounts.Length?visitCounts[move.TileIndex]:0;
                return 450-Math.Min(300,visits*50);
            }
            if(command is DefendCommand)return policy==BalancePolicy.Cautious?800:300;
            return 0;
        }

        private static bool IsLivingMonster(RunState state,int index){var t=state.Tiles[index];return (t.Content==TileContentKind.Monster||t.Content==TileContentKind.Boss)&&t.Visibility==TileVisibility.Revealed&&t.Resolution==TileResolution.Available&&t.MonsterHp>0;}
        private static int AdjacentLivingMonster(RunState state){for(int i=0;i<state.Tiles.Count;i++)if(IsAdjacent(state,i)&&IsLivingMonster(state,i))return i;return -1;}
        private static bool IsSameLine(RunState state,int index){var p=Position(index);return p.Row==state.PlayerPosition.Row||p.Col==state.PlayerPosition.Col;}
        private static int Manhattan(GridPosition a,GridPosition b)=>Math.Abs(a.Row-b.Row)+Math.Abs(a.Col-b.Col);
        private static GridPosition Position(int index)=>new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);
        private static bool IsAdjacent(RunState state,int index)=>state.PlayerPosition.IsOrthogonallyAdjacent(Position(index));
        private static int Index(GridPosition p)=>p.Row*RunState.BoardSize+p.Col;
    }
}
