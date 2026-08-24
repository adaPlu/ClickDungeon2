#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
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
        public static void RunBatch()
        {
            const int runs=500; int deaths=0,totalCommands=0,totalFloor=0,forbidden=0;
            for(uint seed=1;seed<=runs;seed++)
            {
                var generator=new FloorGenerator();var state=generator.CreateNewRun(seed,HeroClassId.Knight);var session=new GameSession(state,generator);var agent=new XorShift32(seed^0x9E3779B9u);
                for(int step=0;step<500&&!state.GameOver&&!state.CampaignCompleted&&state.Floor<=5;step++)
                {
                    var command=ChooseCommand(state,agent);if(command==null)break;var result=session.Apply(command);if(!result.Accepted)break;totalCommands++;
                    if(command is TakeForbiddenExitCommand)forbidden++;
                }
                if(state.GameOver)deaths++;totalFloor+=state.Floor;
            }
            string line=$"runs={runs}, deaths={deaths}, avg_floor={(double)totalFloor/runs:F2}, avg_commands={(double)totalCommands/runs:F1}, forbidden={forbidden}";
            string dir=Path.Combine(Directory.GetCurrentDirectory(),"balance");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"smoke_metrics.txt"),line+Environment.NewLine);Debug.Log(line);
        }

        private static GameCommand ChooseCommand(RunState state,IRandomSource rng)
        {
            int player=state.PlayerPosition.Row*RunState.BoardSize+state.PlayerPosition.Col;var candidates=new List<GameCommand>();
            for(int i=0;i<state.Tiles.Count;i++)
            {
                var pos=new GridPosition(i/RunState.BoardSize,i%RunState.BoardSize);if(!state.PlayerPosition.IsOrthogonallyAdjacent(pos))continue;var t=state.Tiles[i];
                if((t.Content==TileContentKind.Monster||t.Content==TileContentKind.Boss)&&t.Visibility==TileVisibility.Revealed&&t.MonsterHp>0)candidates.Add(new AttackCommand(i));
                else if(t.Visibility!=TileVisibility.Revealed&&!ThreatResolver.IsThreatened(state,i))candidates.Add(new RevealTileCommand(i));
                else if(t.Visibility==TileVisibility.Revealed&&t.Content==TileContentKind.Gold&&t.Resolution==TileResolution.Available)candidates.Add(new InteractCommand(i));
                else if(t.Visibility==TileVisibility.Revealed&&t.Content==TileContentKind.SmallKey&&t.Resolution==TileResolution.Available)candidates.Add(new InteractCommand(i));
                else if(t.Visibility==TileVisibility.Revealed&&t.Content==TileContentKind.BigKey&&t.Resolution==TileResolution.Available&&state.BigKeys<2)candidates.Add(new InteractCommand(i));
                else if(t.Visibility==TileVisibility.Revealed&&t.Content==TileContentKind.SafeExit&&!state.BossRequired)candidates.Add(new TakeSafeExitCommand(i));
                else if(t.Visibility==TileVisibility.Revealed&&t.Content==TileContentKind.ForbiddenExit&&state.BigKeys>0&&!state.BossRequired)candidates.Add(new TakeForbiddenExitCommand(i));
                else if(t.Visibility==TileVisibility.Revealed&&t.Content==TileContentKind.Empty)candidates.Add(new MoveCommand(i));
            }
            if(candidates.Count==0)return null;return candidates[rng.NextInt(candidates.Count)];
        }
    }
}
#endif
