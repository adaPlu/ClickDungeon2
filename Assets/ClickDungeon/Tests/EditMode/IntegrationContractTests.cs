using System.Linq;
using NUnit.Framework;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class IntegrationContractTests
    {
        [Test]
        public void StormAffixResolvesSecondaryMonsterDeath()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(400,HeroClassId.Knight);ClearBoard(state);
            state.PlayerPosition=new GridPosition(2,2);state.Tiles[12].Occupancy=OccupancyKind.Player;state.EquippedWeaponAffixId="affix.storm";
            ConfigureMonster(state.Tiles[13],"monster.rat",5);ConfigureMonster(state.Tiles[14],"monster.slime",1);
            var result=new GameSession(state,gen).Apply(new AttackCommand(13));
            Assert.IsTrue(result.Accepted);Assert.AreEqual(0,state.Tiles[14].MonsterHp);Assert.AreEqual(TileResolution.Resolved,state.Tiles[14].Resolution);Assert.IsTrue(result.Events.Any(e=>e.Type=="monster.defeated"&&e.TileIndex==14));
        }

        [Test]
        public void CampaignDemoCapRejectsFloorSixUntilUnlocked()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(401,HeroClassId.Knight);gen.GenerateFloor(state,5,RouteModifier.Standard);state.CampaignFloorLimit=5;state.BossRequired=false;state.BossDefeated=true;
            var exit=state.Tiles.First(t=>t.Content==TileContentKind.SafeExit);PlacePlayerAdjacent(state,exit.Index);exit.Visibility=TileVisibility.Revealed;
            var blocked=new GameSession(state,gen).Apply(new TakeSafeExitCommand(exit.Index));Assert.IsFalse(blocked.Accepted);Assert.AreEqual("entitlement.full_game_required",blocked.RejectionReason);
            state.CampaignFloorLimit=50;var allowed=new GameSession(state,gen).Apply(new TakeSafeExitCommand(exit.Index));Assert.IsTrue(allowed.Accepted);Assert.AreEqual(6,state.Floor);
        }

        [Test]
        public void RevealedExitCannotBeTakenFromAcrossTheBoard()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(403,HeroClassId.Knight);var exit=state.Tiles.First(t=>t.Content==TileContentKind.SafeExit);exit.Visibility=TileVisibility.Revealed;
            if(state.PlayerPosition.IsOrthogonallyAdjacent(new GridPosition(exit.Index/RunState.BoardSize,exit.Index%RunState.BoardSize)))state.PlayerPosition=new GridPosition(0,0);
            var result=new GameSession(state,gen).Apply(new TakeSafeExitCommand(exit.Index));Assert.IsFalse(result.Accepted);Assert.AreEqual("tile.not_adjacent",result.RejectionReason);
        }

        [Test]
        public void MerchantAndShrineActionsRequireSpatialAccess()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(404,HeroClassId.Knight);ClearBoard(state);state.PlayerPosition=new GridPosition(0,0);state.Tiles[0].Occupancy=OccupancyKind.Player;state.Gold=100;
            state.Tiles[24].Content=TileContentKind.Merchant;state.Tiles[24].ContentId="merchant.standard";state.Tiles[24].Visibility=TileVisibility.Revealed;state.Tiles[24].Resolution=TileResolution.Available;
            state.Tiles[23].Content=TileContentKind.Shrine;state.Tiles[23].ContentId="shrine.choice";state.Tiles[23].Visibility=TileVisibility.Revealed;state.Tiles[23].Resolution=TileResolution.Available;
            var session=new GameSession(state,gen);Assert.AreEqual("tile.not_adjacent",session.Apply(new BuyItemCommand(24,"item.healing_potion")).RejectionReason);Assert.AreEqual("tile.not_adjacent",session.Apply(new ChooseShrineCommand(23,ShrineChoice.Attack)).RejectionReason);
        }

        [Test]
        public void EquipmentCommandSelectsExactAffixedInstance()
        {
            var gen=new FloorGenerator();var state=gen.CreateNewRun(402,HeroClassId.Knight);state.ItemInstances.Add(new ItemInstanceState{InstanceId="fire",BaseItemId="item.rusty_sword",AffixId="affix.flaming"});state.ItemInstances.Add(new ItemInstanceState{InstanceId="storm",BaseItemId="item.rusty_sword",AffixId="affix.storm"});
            var result=new GameSession(state,gen).Apply(new EquipItemCommand("item.rusty_sword","fire"));Assert.IsTrue(result.Accepted);Assert.AreEqual("affix.flaming",state.EquippedWeaponAffixId);
        }

        private static void ClearBoard(RunState state){foreach(var t in state.Tiles){t.Content=TileContentKind.Empty;t.ContentId="tile.empty";t.Visibility=TileVisibility.Revealed;t.Resolution=TileResolution.Resolved;t.Occupancy=OccupancyKind.None;t.MonsterHp=0;t.MonsterMaxHp=0;} }
        private static void ConfigureMonster(TileState t,string id,int hp){t.Content=TileContentKind.Monster;t.ContentId=id;t.Visibility=TileVisibility.Revealed;t.Resolution=TileResolution.Available;t.Occupancy=OccupancyKind.Monster;t.MonsterHp=hp;t.MonsterMaxHp=hp;t.MonsterAttack=1;t.MonsterDefense=0;t.IntentPower=1;}
        private static void PlacePlayerAdjacent(RunState state,int target){foreach(var t in state.Tiles)if(t.Occupancy==OccupancyKind.Player)t.Occupancy=OccupancyKind.None;var p=new GridPosition(target/RunState.BoardSize,target%RunState.BoardSize);GridPosition candidate=p.Col>0?new GridPosition(p.Row,p.Col-1):new GridPosition(p.Row,p.Col+1);state.PlayerPosition=candidate;state.Tiles[candidate.Row*RunState.BoardSize+candidate.Col].Occupancy=OccupancyKind.Player;}
    }
}
