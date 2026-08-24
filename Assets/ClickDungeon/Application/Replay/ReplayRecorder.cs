using System;
using System.Linq;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Replay
{
    public sealed class ReplayRecorder
    {
        private readonly ReplayEnvelope _replay;
        public ReplayEnvelope Replay => _replay;

        public ReplayRecorder(RunState initialState)
        {
            if(initialState==null)throw new ArgumentNullException(nameof(initialState));
            _replay=new ReplayEnvelope
            {
                RootSeed=initialState.RootSeed,
                HeroClassId=initialState.HeroClass.ToString(),
                Mode=initialState.Mode,
                CampaignFloorLimit=initialState.CampaignFloorLimit,
                UnlockedAbilityIds=initialState.AbilityStates.Select(a=>a.AbilityId).ToList()
            };
        }

        public void Record(GameCommand command)
        {
            _replay.Commands.Add(ReplayCommandCodec.Encode(command));
        }

        public ReplayEnvelope Finish(RunState finalState)
        {
            if(finalState==null)throw new ArgumentNullException(nameof(finalState));
            _replay.FinalStateHash=StateHasher.Hash(finalState);
            return _replay;
        }
    }
}
