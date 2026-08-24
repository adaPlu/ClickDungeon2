using System;
using System.IO;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Replay
{
    public sealed class ReplayPlaybackResult
    {
        public GameSession Session;
        public int CommandsApplied;
        public int RejectedCommands;
        public string FinalStateHash=string.Empty;
    }

    public static class ReplayRunner
    {
        public static ReplayPlaybackResult Play(ReplayEnvelope replay,GameContent content)
        {
            if(replay==null)throw new ArgumentNullException(nameof(replay));
            if(content==null)throw new ArgumentNullException(nameof(content));
            ReplayCodec.ValidateCompatibility(replay);
            if(!Enum.TryParse(replay.HeroClassId,true,out HeroClassId heroClass)||!Enum.IsDefined(typeof(HeroClassId),heroClass))throw new InvalidDataException($"Replay hero class '{replay.HeroClassId}' is invalid.");

            var generator=new FloorGenerator(content);
            RunState state;
            switch(replay.Mode)
            {
                case RunMode.Campaign:state=generator.CreateNewRun(replay.RootSeed,heroClass,replay.UnlockedAbilityIds);break;
                case RunMode.Abyss:state=generator.CreateAbyssRun(replay.RootSeed,heroClass,replay.UnlockedAbilityIds);break;
                default:throw new InvalidDataException($"Replay run mode {replay.Mode} is unsupported.");
            }
            state.CampaignFloorLimit=replay.CampaignFloorLimit;
            var session=new GameSession(state,generator,content);
            int rejected=0;
            foreach(string encodedCommand in replay.Commands)
            {
                var command=ReplayCommandCodec.Decode(encodedCommand);
                var result=session.Apply(command);
                if(!result.Accepted)rejected++;
            }
            string hash=StateHasher.Hash(state);
            if(!string.IsNullOrEmpty(replay.FinalStateHash)&&!string.Equals(hash,replay.FinalStateHash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException($"Replay diverged. Expected state hash {replay.FinalStateHash}, got {hash}.");
            return new ReplayPlaybackResult{Session=session,CommandsApplied=replay.Commands.Count,RejectedCommands=rejected,FinalStateHash=hash};
        }
    }
}
