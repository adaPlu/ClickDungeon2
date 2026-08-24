using System;

namespace ClickDungeon.Simulation.Randomness
{
    public sealed class XorShift32 : IRandomSource
    {
        public const int AlgorithmVersion = 1;
        private uint _state;
        public uint State => _state;

        public XorShift32(uint seed)
        {
            _state = seed == 0 ? 0x6D2B79F5u : seed;
        }

        public uint NextUInt()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x == 0 ? 0x6D2B79F5u : x;
            return _state;
        }

        public int NextInt(int exclusiveMax)
        {
            if (exclusiveMax <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            return (int)(NextUInt() % (uint)exclusiveMax);
        }

        public int NextRange(int inclusiveMin, int exclusiveMax)
        {
            if (exclusiveMax <= inclusiveMin) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            return inclusiveMin + NextInt(exclusiveMax - inclusiveMin);
        }

        public bool ChanceBasisPoints(int basisPoints)
        {
            if (basisPoints <= 0) return false;
            if (basisPoints >= 10000) return true;
            return NextInt(10000) < basisPoints;
        }
    }
}
