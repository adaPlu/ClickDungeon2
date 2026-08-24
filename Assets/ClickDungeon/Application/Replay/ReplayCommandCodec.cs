using System;
using System.Globalization;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Replay
{
    public static class ReplayCommandCodec
    {
        public static string Encode(GameCommand command)
        {
            if(command==null)throw new ArgumentNullException(nameof(command));
            if(command is RevealTileCommand reveal)return $"reveal|{reveal.TileIndex}";
            if(command is MoveCommand move)return $"move|{move.TileIndex}";
            if(command is InteractCommand interact)return $"interact|{interact.TileIndex}";
            if(command is AttackCommand attack)return $"attack|{attack.TileIndex}";
            if(command is DefendCommand)return "defend";
            if(command is UseAbilityCommand ability)return $"ability|{Escape(ability.AbilityId)}|{ability.TargetTileIndex}";
            if(command is UseItemCommand item)return $"item|{Escape(item.ItemId)}";
            if(command is ChooseShrineCommand shrine)return $"shrine|{shrine.TileIndex}|{(int)shrine.Choice}";
            if(command is BuyItemCommand buy)return $"buy|{buy.MerchantTileIndex}|{Escape(buy.ItemId)}";
            if(command is EquipItemCommand equip)return $"equip|{Escape(equip.ItemId)}|{Escape(equip.InstanceId)}";
            if(command is TakeSafeExitCommand safe)return $"safe_exit|{safe.TileIndex}";
            if(command is TakeForbiddenExitCommand forbidden)return $"forbidden_exit|{forbidden.TileIndex}";
            if(command is UnlockVaultCommand vault)return $"vault|{vault.TileIndex}";
            throw new NotSupportedException($"Replay serialization does not support {command.GetType().Name}.");
        }

        public static GameCommand Decode(string encoded)
        {
            if(string.IsNullOrWhiteSpace(encoded))throw new FormatException("Replay command is empty.");
            string[] parts=encoded.Split('|');
            switch(parts[0])
            {
                case "reveal":Require(parts,2);return new RevealTileCommand(Int(parts[1]));
                case "move":Require(parts,2);return new MoveCommand(Int(parts[1]));
                case "interact":Require(parts,2);return new InteractCommand(Int(parts[1]));
                case "attack":Require(parts,2);return new AttackCommand(Int(parts[1]));
                case "defend":Require(parts,1);return new DefendCommand();
                case "ability":Require(parts,3);return new UseAbilityCommand(Unescape(parts[1]),Int(parts[2]));
                case "item":Require(parts,2);return new UseItemCommand(Unescape(parts[1]));
                case "shrine":
                    Require(parts,3);int choice=Int(parts[2]);if(!Enum.IsDefined(typeof(ShrineChoice),choice))throw new FormatException($"Unknown shrine choice {choice}.");return new ChooseShrineCommand(Int(parts[1]),(ShrineChoice)choice);
                case "buy":Require(parts,3);return new BuyItemCommand(Int(parts[1]),Unescape(parts[2]));
                case "equip":Require(parts,3);return new EquipItemCommand(Unescape(parts[1]),Unescape(parts[2]));
                case "safe_exit":Require(parts,2);return new TakeSafeExitCommand(Int(parts[1]));
                case "forbidden_exit":Require(parts,2);return new TakeForbiddenExitCommand(Int(parts[1]));
                case "vault":Require(parts,2);return new UnlockVaultCommand(Int(parts[1]));
                default:throw new FormatException($"Unknown replay command '{parts[0]}'.");
            }
        }

        private static void Require(string[] parts,int expected){if(parts.Length!=expected)throw new FormatException($"Replay command '{parts[0]}' expected {expected-1} argument(s), got {parts.Length-1}.");}
        private static int Int(string value){if(!int.TryParse(value,NumberStyles.Integer,CultureInfo.InvariantCulture,out int parsed))throw new FormatException($"Replay integer '{value}' is invalid.");return parsed;}
        private static string Escape(string value)=>Uri.EscapeDataString(value??string.Empty);
        private static string Unescape(string value)=>Uri.UnescapeDataString(value??string.Empty);
    }
}
