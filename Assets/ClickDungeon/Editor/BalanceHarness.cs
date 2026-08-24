#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ClickDungeon.Application.Content;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Randomness;
using ClickDungeon.Simulation.Rules;

namespace ClickDungeon.EditorTools
{
    public static class BalanceHarness
    {
        private enum AgentProfile { RandomLegal, Cautious, GreedyLoot, ForbiddenRoute }

        private sealed class Metrics
        {
            public int Runs,Completed,Deaths,Stalls,Commands,Rejected,ForbiddenTransitions,FinalFloor,FinalGold;
            public void Add(RunState state,int commands,int rejected,int forbidden,bool stalled)
            {
                Runs++;Commands+=commands;Rejected+=rejected;ForbiddenTransitions+=forbidden;FinalFloor+=state.Floor;FinalGold+=state.Gold;
                if(state.CampaignCompleted)Completed++;else if(state.GameOver)Deaths++;else if(stalled)Stalls++;
            }
            public string Summary(HeroClassId hero,AgentProfile profile)
            {
                double n=Math.Max(1,Runs);return $"hero={hero}, agent={profile}, runs={Runs}, completion={(100.0*Completed/n):F1}%, deaths={(100.0*Deaths/n):F1}%, stalls={(100.0*Stalls/n):F1}%, avg_floor={(double)FinalFloor/n:F2}, avg_commands={(double)Commands/n:F1}, avg_rejected={(double)Rejected/n:F2}, forbidden={ForbiddenTransitions}, avg_gold={(double)FinalGold/n:F1}";
            }
        }

        public static void RunBatch()
        {
            const int seedsPerCombination=100;
            const int maxCommandsPerRun=6000;
            string root=Directory.GetCurrentDirectory();string contentDir=Path.Combine(root,"Assets","ClickDungeon","Content","Json");var content=new JsonContentCatalogLoader().LoadFromDirectory(contentDir);
            var lines=new List<string>{"ClickDungeon2 deterministic balance smoke","NOTE: automated agents expose balance/pathing defects; they do not prove fun."};
            foreach(HeroClassId hero in Enum.GetValues(typeof(HeroClassId)))
            foreach(AgentProfile profile in Enum.GetValues(typeof(AgentProfile)))
            {
                var metrics=new Metrics();
                for(uint seed=1;seed<=seedsPerCombination;seed++)
                {
                    uint runSeed=seed+(uint)((int)hero*10000)+((uint)(int)profile*100000);
                    var generator=new FloorGenerator(content);var state=generator.CreateNewRun(runSeed,hero);state.CampaignFloorLimit=content.Balance.CampaignFloors;var session=new GameSession(state,generator,content);var rng=new XorShift32(runSeed^0x9E3779B9u);
                    int commands=0,rejected=0,forbidden=0;bool stalled=false;
                    while(commands<maxCommandsPerRun&&!state.GameOver&&!state.CampaignCompleted)
                    {
                        var candidates=BuildCandidates(state,profile,rng,content);bool accepted=false;
                        foreach(var command in candidates)
                        {
                            var result=session.Apply(command);if(!result.Accepted){rejected++;continue;}accepted=true;commands++;if(command is TakeForbiddenExitCommand)forbidden++;break;
                        }
                        if(!accepted){stalled=true;break;}
                    }
                    if(commands>=maxCommandsPerRun&&!state.GameOver&&!state.CampaignCompleted)stalled=true;
                    metrics.Add(state,commands,rejected,forbidden,stalled);
                }
                string line=metrics.Summary(hero,profile);lines.Add(line);Debug.Log(line);
            }
            string dir=Path.Combine(root,"balance");Directory.CreateDirectory(dir);File.WriteAllLines(Path.Combine(dir,"campaign_agent_metrics.txt"),lines);
        }

        private static List<GameCommand> BuildCandidates(RunState state,AgentProfile profile,IRandomSource rng,ClickDungeon.Simulation.Content.GameContent content)
        {
            var attacks=new List<GameCommand>();var survival=new List<GameCommand>();var rewards=new List<GameCommand>();var exits=new List<GameCommand>();var reveal=new List<GameCommand>();var movement=new List<GameCommand>();
            int hpPct=state.MaxHp<=0?0:(state.Hp*100/state.MaxHp);
            if(hpPct<=45&&state.InventoryItemIds.Contains("item.healing_potion"))survival.Add(new UseItemCommand("item.healing_potion"));
            for(int i=0;i<state.Tiles.Count;i++)
            {
                var pos=new GridPosition(i/RunState.BoardSize,i%RunState.BoardSize);bool adjacent=state.PlayerPosition.IsOrthogonallyAdjacent(pos);var t=state.Tiles[i];if(!adjacent)continue;
                if((t.Content==TileContentKind.Monster||t.Content==TileContentKind.Boss)&&t.Visibility==TileVisibility.Revealed&&t.Resolution==TileResolution.Available&&t.MonsterHp>0){attacks.Add(new AttackCommand(i));continue;}
                if(t.Content==TileContentKind.Trap&&t.Resolution==TileResolution.Available&&(t.Visibility==TileVisibility.Identified||t.Visibility==TileVisibility.Revealed)&&state.InventoryItemIds.Contains("item.trap_disarm_kit")){survival.Add(new UseItemCommand("item.trap_disarm_kit",i));continue;}
                if(t.Visibility!=TileVisibility.Revealed&&!ThreatResolver.IsThreatened(state,i)){reveal.Add(new RevealTileCommand(i));continue;}
                if(t.Visibility!=TileVisibility.Revealed)continue;
                if(t.Content==TileContentKind.Gold&&t.Resolution==TileResolution.Available)rewards.Add(new InteractCommand(i));
                else if(t.Content==TileContentKind.SmallKey&&t.Resolution==TileResolution.Available)rewards.Add(new InteractCommand(i));
                else if(t.Content==TileContentKind.BigKey&&t.Resolution==TileResolution.Available&&state.BigKeys<content.Balance.BigKeyMaxCarry)rewards.Add(new InteractCommand(i));
                else if(t.Content==TileContentKind.Chest&&t.Resolution==TileResolution.Available&&state.SmallKeys>0)rewards.Add(new InteractCommand(i));
                else if(t.Content==TileContentKind.Shrine&&t.Resolution==TileResolution.Available)rewards.Add(new ChooseShrineCommand(i,hpPct<60?ShrineChoice.MaxHp:ShrineChoice.Attack));
                else if(t.Content==TileContentKind.SealedVault&&t.Resolution==TileResolution.Available&&state.BigKeys>0&&profile==AgentProfile.GreedyLoot)rewards.Add(new UnlockVaultCommand(i));
                else if(t.Content==TileContentKind.Merchant&&t.Resolution==TileResolution.Available&&state.Gold>=content.Item("item.healing_potion").Price&&!state.InventoryItemIds.Contains("item.healing_potion"))rewards.Add(new BuyItemCommand(i,"item.healing_potion"));
                else if(t.Content==TileContentKind.SafeExit&&(!state.BossRequired||state.BossDefeated))exits.Add(new TakeSafeExitCommand(i));
                else if(t.Content==TileContentKind.ForbiddenExit&&state.BigKeys>0&&(!state.BossRequired||state.BossDefeated))exits.Add(new TakeForbiddenExitCommand(i));
                else if(t.Occupancy!=OccupancyKind.Monster&&t.Visibility==TileVisibility.Revealed&&!ThreatResolver.IsThreatened(state,i))movement.Add(new MoveCommand(i));
            }
            if(attacks.Count>0)return Ordered(attacks,rng);
            if(survival.Count>0)return Ordered(survival,rng);
            if(profile==AgentProfile.GreedyLoot&&rewards.Count>0)return Ordered(rewards,rng);
            if(profile==AgentProfile.ForbiddenRoute){var hard=exits.OfType<TakeForbiddenExitCommand>().Cast<GameCommand>().ToList();if(hard.Count>0)return Ordered(hard,rng);}
            if(profile==AgentProfile.Cautious){var safe=exits.OfType<TakeSafeExitCommand>().Cast<GameCommand>().ToList();if(safe.Count>0)return Ordered(safe,rng);}
            if(rewards.Count>0)return Ordered(rewards,rng);
            if(reveal.Count>0)return Ordered(reveal,rng);
            if(exits.Count>0)return Ordered(exits,rng);
            if(movement.Count>0)return Ordered(movement,rng);
            return new List<GameCommand>();
        }

        private static List<GameCommand> Ordered(List<GameCommand> source,IRandomSource rng)
        {
            if(source.Count<2)return source;var copy=new List<GameCommand>(source);for(int i=copy.Count-1;i>0;i--){int j=rng.NextInt(i+1);var tmp=copy[i];copy[i]=copy[j];copy[j]=tmp;}return copy;
        }
    }
}
#endif
