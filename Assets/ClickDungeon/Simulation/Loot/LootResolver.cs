using System;
using System.Linq;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Randomness;

namespace ClickDungeon.Simulation.Loot
{
    public sealed class LootResult
    {
        public int Gold;
        public ItemInstanceState Item;
        public string BaseItemId = string.Empty;
    }

    public sealed class LootResolver
    {
        private readonly GameContent _content;
        public LootResolver(GameContent content) { _content = content; }

        public LootResult Roll(RunState state, string tableId, int sourceTileIndex)
        {
            var table = _content.LootTable(tableId);
            if (table.Entries.Length == 0) return new LootResult();
            uint seed = SeedDerivation.Derive(state.FloorSeed, $"loot:{tableId}:{sourceTileIndex}:{state.LootRollCounter++}");
            var rng = new XorShift32(seed);
            int total = table.Entries.Sum(e => Math.Max(0, e.Weight));
            if (total <= 0) return new LootResult();
            int roll = rng.NextInt(total);
            LootEntryDefinition selected = table.Entries[0];
            foreach (var entry in table.Entries)
            {
                int weight = Math.Max(0, entry.Weight);
                if (roll < weight) { selected = entry; break; }
                roll -= weight;
            }
            if (selected.Id == "currency.gold") return new LootResult { Gold = 15 };
            if (selected.Id == "currency.gold.large") return new LootResult { Gold = 40 };
            if (!_content.TryItem(selected.Id, out var itemDef)) return new LootResult();

            var item = new ItemInstanceState
            {
                InstanceId = $"loot-{state.Floor}-{sourceTileIndex}-{state.LootRollCounter}",
                BaseItemId = itemDef.Id,
                AffixId = RollAffix(itemDef, rng)
            };
            return new LootResult { BaseItemId = itemDef.Id, Item = item };
        }

        private string RollAffix(ItemDefinition item, IRandomSource rng)
        {
            if (item.Kind != "weapon" && item.Kind != "armor") return string.Empty;
            if (!rng.ChanceBasisPoints(2500)) return string.Empty;
            var values = _content.Affixes.OrderBy(a => a.Id, StringComparer.Ordinal).ToArray();
            return values.Length == 0 ? string.Empty : values[rng.NextInt(values.Length)].Id;
        }
    }
}
