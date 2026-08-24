using System.Linq;
using NUnit.Framework;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class EconomyTrapKitTests
    {
        [Test]
        public void TrapDisarmKitConsumesOneKitAndSafelyResolvesAdjacentIdentifiedTrap()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(501,HeroClassId.Thief);ClearBoard(state);
            state.PlayerPosition=new GridPosition(2,2);state.Tiles[12].Occupancy=OccupancyKind.Player;
            var trap=state.Tiles[13];trap.Content=TileContentKind.Trap;trap.ContentId="trap.poison";trap.Visibility=TileVisibility.Identified;trap.Resolution=TileResolution.Available;trap.Occupancy=OccupancyKind.Hazard;
            state.InventoryItemIds.Add("item.trap_disarm_kit");int hp=state.Hp;

            var result=new GameSession(state,gen).Apply(new UseItemCommand("item.trap_disarm_kit",13));

            Assert.IsTrue(result.Accepted);Assert.AreEqual(TileResolution.Resolved,trap.Resolution);Assert.AreEqual(OccupancyKind.None,trap.Occupancy);Assert.AreEqual(hp,state.Hp);Assert.IsFalse(state.InventoryItemIds.Contains("item.trap_disarm_kit"));Assert.IsTrue(result.Events.Any(e=>e.Type=="trap.disarmed"&&e.TileIndex==13));
        }

        [Test]
        public void TrapDisarmKitRejectsHiddenOrRemoteTrapWithoutConsumingKit()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(502,HeroClassId.Thief);ClearBoard(state);
            state.PlayerPosition=new GridPosition(0,0);state.Tiles[0].Occupancy=OccupancyKind.Player;
            var trap=state.Tiles[24];trap.Content=TileContentKind.Trap;trap.ContentId="trap.fire";trap.Visibility=TileVisibility.Hidden;trap.Resolution=TileResolution.Available;trap.Occupancy=OccupancyKind.Hazard;
            state.InventoryItemIds.Add("item.trap_disarm_kit");

            var result=new GameSession(state,gen).Apply(new UseItemCommand("item.trap_disarm_kit",24));

            Assert.IsFalse(result.Accepted);Assert.AreEqual("tile.not_adjacent",result.RejectionReason);Assert.IsTrue(state.InventoryItemIds.Contains("item.trap_disarm_kit"));Assert.AreEqual(TileResolution.Available,trap.Resolution);
        }

        [Test]
        public void StandardMerchantCanSellTrapDisarmKitAtCanonicalPrice()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(503,HeroClassId.Knight);ClearBoard(state);
            state.PlayerPosition=new GridPosition(2,2);state.Tiles[12].Occupancy=OccupancyKind.Player;state.Gold=20;
            var merchant=state.Tiles[13];merchant.Content=TileContentKind.Merchant;merchant.ContentId="merchant.standard";merchant.Visibility=TileVisibility.Revealed;merchant.Resolution=TileResolution.Available;

            var result=new GameSession(state,gen).Apply(new BuyItemCommand(13,"item.trap_disarm_kit"));

            Assert.IsTrue(result.Accepted);Assert.AreEqual(5,state.Gold);Assert.IsTrue(state.InventoryItemIds.Contains("item.trap_disarm_kit"));Assert.IsTrue(result.Events.Any(e=>e.Type=="merchant.item_bought"&&e.Id=="item.trap_disarm_kit"&&e.Amount==15));
        }

        private static void ClearBoard(RunState state)
        {
            foreach(var t in state.Tiles){t.Content=TileContentKind.Empty;t.ContentId="tile.empty";t.Visibility=TileVisibility.Revealed;t.Resolution=TileResolution.Resolved;t.Occupancy=OccupancyKind.None;t.MonsterHp=0;t.MonsterMaxHp=0;}
        }
    }
}
