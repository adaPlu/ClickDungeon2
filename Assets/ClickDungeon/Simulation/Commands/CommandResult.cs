using System.Collections.Generic;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Simulation.Commands
{
    public sealed class CommandResult
    {
        public bool Accepted { get; }
        public string RejectionReason { get; }
        public IReadOnlyList<GameEvent> Events { get; }
        private CommandResult(bool accepted, string reason, IReadOnlyList<GameEvent> events) { Accepted = accepted; RejectionReason = reason; Events = events; }
        public static CommandResult Reject(string reason) => new CommandResult(false, reason, new GameEvent[0]);
        public static CommandResult Accept(List<GameEvent> events) => new CommandResult(true, string.Empty, events);
    }
}
