using System;
using System.Collections.Generic;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Simulation.Rules
{
    public static class ThreatResolver
    {
        public static bool IsThreatened(RunState state, int targetIndex)
        {
            foreach (int index in ThreateningMonsters(state, targetIndex)) return true;
            return false;
        }

        public static IEnumerable<int> ThreateningMonsters(RunState state, int targetIndex)
        {
            var target = Position(targetIndex);
            for (int i = 0; i < state.Tiles.Count; i++)
            {
                var tile = state.Tiles[i];
                if ((tile.Content != TileContentKind.Monster && tile.Content != TileContentKind.Boss) || tile.Resolution == TileResolution.Resolved || tile.MonsterHp <= 0 || tile.Visibility != TileVisibility.Revealed) continue;
                if (Threatens(Position(i), target, tile.ThreatPattern)) yield return i;
            }
        }

        public static bool Threatens(GridPosition source, GridPosition target, ThreatPattern pattern)
        {
            int dr = Math.Abs(source.Row - target.Row);
            int dc = Math.Abs(source.Col - target.Col);
            switch (pattern)
            {
                case ThreatPattern.Adjacent: return dr + dc == 1;
                case ThreatPattern.CrossTwo: return (dr == 0 && dc >= 1 && dc <= 2) || (dc == 0 && dr >= 1 && dr <= 2);
                case ThreatPattern.OrthogonalLine: return (dr == 0 && dc > 0) || (dc == 0 && dr > 0);
                case ThreatPattern.AuraTwo: return dr + dc >= 1 && dr + dc <= 2;
                default: return false;
            }
        }

        private static GridPosition Position(int index) => new GridPosition(index / RunState.BoardSize, index % RunState.BoardSize);
    }
}
