using System;
using System.Collections.Generic;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Rules;
using ClickDungeon.Simulation.Status;

namespace ClickDungeon.Simulation.Biome
{
    public static class TerrainResolver
    {
        public static void ResolveEntry(RunState state,int oldIndex,int newIndex,GameContent content,List<GameEvent> events)
        {
            var tile=state.Tiles[newIndex];
            switch(tile.Terrain)
            {
                case TerrainKind.Thorn:
                    if(!tile.TerrainTriggered){Damage(state,1,events,"terrain.thorn");tile.TerrainTriggered=true;}break;
                case TerrainKind.Mire:
                    if(!tile.TerrainTriggered){StatusResolver.AddOrRefresh(state,content,"status.poison",2);events.Add(new GameEvent("terrain.mire.poison",newIndex));tile.TerrainTriggered=true;}break;
                case TerrainKind.Grave:
                    if(!tile.TerrainTriggered){StatusResolver.AddOrRefresh(state,content,"status.curse",3);events.Add(new GameEvent("terrain.grave.curse",newIndex));tile.TerrainTriggered=true;}break;
                case TerrainKind.Charged:
                    if(!tile.TerrainTriggered&&HasAdjacentTerrain(state,newIndex,TerrainKind.Charged)){Damage(state,1,events,"terrain.charged");tile.TerrainTriggered=true;}break;
                case TerrainKind.Lava:
                    Damage(state,1,events,"terrain.lava");break;
                case TerrainKind.Arcane:
                    if(!tile.TerrainTriggered){events.Add(new GameEvent("terrain.arcane.recharge",newIndex,"",1));tile.TerrainTriggered=true;}break;
                case TerrainKind.Flooded:
                    events.Add(new GameEvent("terrain.flooded",newIndex));break;
                case TerrainKind.Ice:
                    events.Add(new GameEvent("terrain.ice",newIndex));break;
                case TerrainKind.Ash:
                    if(!tile.TerrainTriggered&&ThreatResolver.IsThreatened(state,newIndex)){Damage(state,1,events,"terrain.ash_pressure");tile.TerrainTriggered=true;}break;
            }
            if(state.Hp<=0){state.GameOver=true;events.Add(new GameEvent("run.game_over"));}
        }

        public static int TryIceSlideTarget(RunState state,int oldIndex,int newIndex)
        {
            if(state.Tiles[newIndex].Terrain!=TerrainKind.Ice)return -1;var old=Position(oldIndex);var current=Position(newIndex);int dr=current.Row-old.Row,dc=current.Col-old.Col;var next=new GridPosition(current.Row+dr,current.Col+dc);if(next.Row<0||next.Row>=RunState.BoardSize||next.Col<0||next.Col>=RunState.BoardSize)return -1;int index=Index(next);var tile=state.Tiles[index];if(tile.Visibility!=TileVisibility.Revealed||tile.Occupancy==OccupancyKind.Monster||ThreatResolver.IsThreatened(state,index))return -1;return index;
        }

        private static bool HasAdjacentTerrain(RunState state,int index,TerrainKind kind)
        {
            var p=Position(index);for(int i=0;i<state.Tiles.Count;i++)if(i!=index&&p.IsOrthogonallyAdjacent(Position(i))&&state.Tiles[i].Terrain==kind)return true;return false;
        }
        private static void Damage(RunState state,int amount,List<GameEvent> events,string id){state.Hp=Math.Max(0,state.Hp-amount);events.Add(new GameEvent(id,-1,"",amount));}
        private static GridPosition Position(int index)=>new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);
        private static int Index(GridPosition p)=>p.Row*RunState.BoardSize+p.Col;
    }
}
