using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Simulation.Commands
{
    public abstract class GameCommand { }
    public sealed class RevealTileCommand : GameCommand { public int TileIndex; public RevealTileCommand(int tileIndex) { TileIndex = tileIndex; } }
    public sealed class MoveCommand : GameCommand { public int TileIndex; public MoveCommand(int tileIndex) { TileIndex = tileIndex; } }
    public sealed class InteractCommand : GameCommand { public int TileIndex; public InteractCommand(int tileIndex) { TileIndex = tileIndex; } }
    public sealed class AttackCommand : GameCommand { public int TileIndex; public AttackCommand(int tileIndex) { TileIndex = tileIndex; } }
    public sealed class DefendCommand : GameCommand { }
    public sealed class UseAbilityCommand : GameCommand { public string AbilityId; public int TargetTileIndex; public UseAbilityCommand(string abilityId, int targetTileIndex=-1) { AbilityId=abilityId; TargetTileIndex=targetTileIndex; } }
    public sealed class UseItemCommand : GameCommand { public string ItemId; public UseItemCommand(string itemId) { ItemId=itemId; } }
    public sealed class ChooseShrineCommand : GameCommand { public int TileIndex; public ShrineChoice Choice; public ChooseShrineCommand(int tileIndex, ShrineChoice choice) { TileIndex=tileIndex; Choice=choice; } }
    public sealed class BuyItemCommand : GameCommand { public int MerchantTileIndex; public string ItemId; public BuyItemCommand(int merchantTileIndex,string itemId) { MerchantTileIndex=merchantTileIndex; ItemId=itemId; } }
    public sealed class EquipItemCommand : GameCommand
    {
        public string ItemId;
        public string InstanceId;
        public EquipItemCommand(string itemId,string instanceId="") { ItemId=itemId; InstanceId=instanceId??string.Empty; }
    }
    public sealed class TakeSafeExitCommand : GameCommand { public int TileIndex; public TakeSafeExitCommand(int tileIndex) { TileIndex = tileIndex; } }
    public sealed class TakeForbiddenExitCommand : GameCommand { public int TileIndex; public TakeForbiddenExitCommand(int tileIndex) { TileIndex = tileIndex; } }
    public sealed class UnlockVaultCommand : GameCommand { public int TileIndex; public UnlockVaultCommand(int tileIndex) { TileIndex = tileIndex; } }
}
