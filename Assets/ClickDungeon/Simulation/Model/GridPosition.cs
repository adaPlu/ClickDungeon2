using System;

namespace ClickDungeon.Simulation.Model
{
    [Serializable]
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public int Row { get; }
        public int Col { get; }
        public GridPosition(int row, int col) { Row = row; Col = col; }
        public bool IsOrthogonallyAdjacent(GridPosition other) => System.Math.Abs(Row - other.Row) + System.Math.Abs(Col - other.Col) == 1;
        public bool Equals(GridPosition other) => Row == other.Row && Col == other.Col;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => (Row * 397) ^ Col;
        public override string ToString() => $"({Row},{Col})";
    }
}
