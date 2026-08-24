using System.Text;

namespace ClickDungeon.Simulation.Randomness
{
    public static class SeedDerivation
    {
        // Stable FNV-1a derivation; never use string.GetHashCode for simulation seeds.
        public static uint Derive(uint parent, string semanticId)
        {
            unchecked
            {
                uint hash = 2166136261u ^ parent;
                byte[] bytes = Encoding.UTF8.GetBytes(semanticId ?? string.Empty);
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash ^= bytes[i];
                    hash *= 16777619u;
                }
                return hash == 0 ? 0x9E3779B9u : hash;
            }
        }
    }
}
