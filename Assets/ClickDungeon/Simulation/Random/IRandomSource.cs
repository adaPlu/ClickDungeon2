namespace ClickDungeon.Simulation.Randomness
{
    public interface IRandomSource
    {
        uint State { get; }
        uint NextUInt();
        int NextInt(int exclusiveMax);
        int NextRange(int inclusiveMin, int exclusiveMax);
        bool ChanceBasisPoints(int basisPoints);
    }
}
