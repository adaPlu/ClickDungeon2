using System;
using System.Collections.Generic;
using System.Linq;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Randomness;

namespace ClickDungeon.Simulation.Boss
{
    public static class BossResolver
    {
        public static void AfterDamage(RunState state,TileState boss,List<GameEvent> events)
        {
            if(boss.Content!=TileContentKind.Boss||boss.MonsterHp<=0)return;int maxPhases=MaxPhases(boss.ContentId);int desired=1;
            for(int phase=2;phase<=maxPhases;phase++){double threshold=1.0-(double)(phase-1)/maxPhases;if((double)boss.MonsterHp/boss.MonsterMaxHp<=threshold)desired=phase;}
            while(boss.BossPhase<desired){boss.BossPhase++;events.Add(new GameEvent("boss.phase_changed",boss.Index,boss.ContentId,boss.BossPhase));ApplyPhase(state,boss,events);}
        }

        private static void ApplyPhase(RunState state,TileState boss,List<GameEvent> events)
        {
            if(boss.ContentId=="boss.lich_sovereign")PaintHazards(state,boss,TerrainKind.Grave,2,events,"boss.lich.curse_cells");
            else if(boss.ContentId=="boss.rootbound_leviathan")PaintHazards(state,boss,TerrainKind.Thorn,2+boss.BossPhase,events,"boss.leviathan.roots");
            else if(boss.ContentId=="boss.frostbog_colossus")PaintHazards(state,boss,boss.BossPhase%2==0?TerrainKind.Ice:TerrainKind.Mire,3,events,"boss.colossus.terrain_shift");
            else if(boss.ContentId=="boss.archdemon_overlord")PaintHazards(state,boss,TerrainKind.Lava,2+boss.BossPhase,events,"boss.archdemon.ignite");
            else if(boss.ContentId=="boss.primal_ancient_wyrm")
            {
                boss.ThreatPattern=boss.BossPhase==2?ThreatPattern.CrossTwo:boss.BossPhase==3?ThreatPattern.OrthogonalLine:ThreatPattern.AuraTwo;PaintHazards(state,boss,boss.BossPhase%2==0?TerrainKind.Charged:TerrainKind.Lava,2,events,"boss.wyrm.board_shift");
            }
            boss.IntentPower+=1;
        }

        private static void PaintHazards(RunState state,TileState boss,TerrainKind terrain,int count,List<GameEvent> events,string eventId)
        {
            var candidates=Enumerable.Range(0,state.Tiles.Count).Where(i=>i!=boss.Index&&i!=Index(state.PlayerPosition)&&state.Tiles[i].Content!=TileContentKind.SafeExit&&state.Tiles[i].Content!=TileContentKind.ForbiddenExit).ToList();var rng=new XorShift32(SeedDerivation.Derive(state.RootSeed,$"boss:{state.Floor}:phase:{boss.BossPhase}:{boss.Index}"));
            for(int n=0;n<count&&candidates.Count>0;n++){int pick=rng.NextInt(candidates.Count);int i=candidates[pick];candidates.RemoveAt(pick);var tile=state.Tiles[i];tile.Terrain=terrain;tile.TerrainTriggered=false;if(tile.Visibility!=TileVisibility.Revealed){tile.Clue=ClueFamily.Danger;if(tile.Visibility==TileVisibility.Hidden)tile.Visibility=TileVisibility.Clued;}events.Add(new GameEvent(eventId,i,boss.ContentId,boss.BossPhase));}
        }
        private static int MaxPhases(string id){if(id=="boss.lich_sovereign"||id=="boss.rootbound_leviathan")return 2;if(id=="boss.frostbog_colossus"||id=="boss.archdemon_overlord")return 3;return 4;}
        private static int Index(GridPosition p)=>p.Row*RunState.BoardSize+p.Col;
    }
}
