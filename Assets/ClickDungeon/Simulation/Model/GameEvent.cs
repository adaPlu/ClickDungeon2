namespace ClickDungeon.Simulation.Model
{
    public sealed class GameEvent
    {
        public string Type { get; }
        public int TileIndex { get; }
        public string Id { get; }
        public int Amount { get; }
        public GameEvent(string type, int tileIndex = -1, string id = "", int amount = 0)
        {
            Type = type; TileIndex = tileIndex; Id = id; Amount = amount;
        }
    }
}
