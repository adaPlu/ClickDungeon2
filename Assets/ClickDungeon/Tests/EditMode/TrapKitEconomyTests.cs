using System.Linq;
using NUnit.Framework;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class TrapKitEconomyTests
    {
        [Test]
        public void TrapDisarmKitSafelyResolvesAdjacentIdentifiedTrap()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(700,HeroClassId.Thief);ClearBoard(state);PlacePlayer(state,12);ConfigureTrap(state.Tiles[13]);state.InventoryItemIds.Add("item.trap_disarm_kit");int hp=state.Hp;
            var result=new GameSession(state,gen).Apply(new UseItemCommand("item.trap_disarm_kit",13));
            Assert.IsTrue(result.Accepted);Assert.AreEqual(hp,state.Hp);Assert.AreEqual(TileResolution.Resolved,state.Tiles[13].Resolution);Assert.AreEqual(OccupancyKind.None,state.Tiles[13].Occupancy);Assert.IsFalse(state.InventoryItemIds.Contains("item.trap_disarm_kit"));Assert.IsTrue(result.Events.Any(e=>e.Type=="trap.disarmed"&&e.TileIndex==13));
        }

        [Test]
        public void InvalidTrapTargetDoesNotConsumeKit()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(701,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);ConfigureTrap(state.Tiles[13]);state.Tiles[13].Visibility=TileVisibility.Hidden;state.InventoryItemIds.Add("item.trap_disarm_kit");
            var result=new GameSession(state,gen).Apply(new UseItemCommand("item.trap_disarm_kit",13));
            Assert.IsFalse(result.Accepted);Assert.AreEqual("trap.not_disarmable",result.RejectionReason);Assert.IsTrue(state.InventoryItemIds.Contains("item.trap_disarm_kit"));Assert.AreEqual(TileResolution.Available,state.Tiles[13].Resolution);
        }

        [Test]
        public void StandardMerchantCanSellTrapDisarmKitAtCanonicalPrice()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(702,HeroClassId.Knight);ClearBoard(state);PlacePlayer(state,12);var merchant=state.Tiles[13];merchant.Content=TileContentKind.Merchant;merchant.ContentId="merchant.standard";merchant.Visibility=TileVisibility.Revealed;merchant.Resolution=TileResolution.Available;state.Gold=20;
            var result=new GameSession(state,gen).Apply(new BuyItemCommand(13,"item.trap_disarm_kit"));
            Assert.IsTrue(result.Accepted);Assert.AreEqual(5,state.Gold);Assert.IsTrue(state.InventoryItemIds.Contains("item.trap_disarm_kit"));Assert.IsTrue(result.Events.Any(e=>e.Type=="merchant.item_bought"&&e.Id=="item.trap_disarm_kit"&&e.Amount==15));
        }

        private static void ClearBoard(RunState state){foreach(var t in state.Tiles){t.Content=TileContentKind.Empty;t.ContentId="tile.empty";t.Visibility=TileVisibility.Revealed;t.Resolution=TileResolution.Resolved;t.Occupancy=OccupancyKind.None;t.MonsterHp=0;t.MonsterMaxHp=0;}}
        private static void PlacePlayer(RunState state,int index){state.PlayerPosition=new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);state.Tiles[index].Occupancy=OccupancyKind.Player;}
        private static void ConfigureTrap(TileState tile){tile.Content=TileContentKind.Trap;tile.ContentId="trap.fire";tile.Visibility=TileVisibility.Identified;tile.Resolution=TileResolution.Available;tile.Occupancy=OccupancyKind.Hazard;}
    }
}
