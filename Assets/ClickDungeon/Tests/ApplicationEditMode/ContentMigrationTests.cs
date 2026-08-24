using NUnit.Framework;
using ClickDungeon.Application.Content;
using ClickDungeon.Application.State;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

public sealed class ContentMigrationTests
{
    [Test] public void DeprecatedIdsAreMigratedBeforeRunUse()
    {
        var content=GameContent.CreateDevelopmentFallback();var state=new RunState{EquippedWeaponId="item.health_potion"};
        state.InventoryItemIds.Add("item.health_potion");state.InventoryItemIds.Add("item.health_potion");state.Tiles.Add(new TileState{Index=0,ContentId="monster.old_frost_rat"});
        var payload=new SlotSavePayload{ActiveRun=state,Meta=new SlotMetaState()};ContentMigrationService.Apply(payload,content);
        Assert.AreEqual("item.healing_potion",state.EquippedWeaponId);Assert.AreEqual(2,state.InventoryItemIds.Count);Assert.AreEqual("item.healing_potion",state.InventoryItemIds[0]);Assert.AreEqual("item.healing_potion",state.InventoryItemIds[1]);Assert.AreEqual("monster.rat",state.Tiles[0].ContentId);
    }
}
