using System;
using System.Collections.Generic;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Simulation.Status
{
    public static class StatusResolver
    {
        public static void AddOrRefresh(RunState state,GameContent content,string id,int durationOverride=0)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            if(content==null)throw new ArgumentNullException(nameof(content));
            var definition=content.Status(id);int duration=durationOverride>0?durationOverride:definition.DefaultDuration;
            foreach(var status in state.Statuses)
            {
                if(status.StatusId!=id)continue;
                status.RemainingActions=Math.Max(status.RemainingActions,duration);
                if(definition.StackRule=="stack_to_3"||definition.MaxStacks>1)status.Stacks=Math.Min(Math.Max(1,definition.MaxStacks),status.Stacks+1);
                else status.Stacks=1;
                return;
            }
            state.Statuses.Add(new StatusInstance{StatusId=id,RemainingActions=Math.Max(1,duration),Stacks=1});
        }

        public static void AdvanceMeaningfulAction(RunState state,GameContent content,List<GameEvent> events,IReadOnlyDictionary<string,StatusInstance> eligibleStatuses=null,bool advanceCamouflage=true)
        {
            AdvanceTimedStatuses(state,content,events,eligibleStatuses,"meaningful_action");
            if(advanceCamouflage&&state.CamouflageActions>0)state.CamouflageActions--;if(state.Hp<=0){state.GameOver=true;events.Add(new GameEvent("run.game_over"));}
        }

        public static void AdvanceFloorAction(RunState state,GameContent content,List<GameEvent> events,IReadOnlyDictionary<string,StatusInstance> eligibleStatuses=null)
        {
            AdvanceTimedStatuses(state,content,events,eligibleStatuses,"floor_action");
            if(state.Hp<=0){state.GameOver=true;events.Add(new GameEvent("run.game_over"));}
        }

        private static void AdvanceTimedStatuses(RunState state,GameContent content,List<GameEvent> events,IReadOnlyDictionary<string,StatusInstance> eligibleStatuses,string tickTiming)
        {
            for(int i=state.Statuses.Count-1;i>=0;i--)
            {
                var status=state.Statuses[i];var definition=content.Status(status.StatusId);
                StatusInstance eligible=null;
                if(eligibleStatuses!=null)
                {
                    if(!eligibleStatuses.TryGetValue(status.StatusId,out eligible))continue;
                }
                if(definition.TickTiming!=tickTiming)continue;
                int tickStacks=eligible?.Stacks??status.Stacks;
                int remainingAfterTick=(eligible?.RemainingActions??status.RemainingActions)-1;
                bool unchangedSinceSnapshot=eligible==null||(status.RemainingActions==eligible.RemainingActions&&status.Stacks==eligible.Stacks);
                if(definition.EffectId=="damage_per_stack")
                {
                    int damage=Math.Max(1,tickStacks);state.Hp=Math.Max(0,state.Hp-damage);events.Add(new GameEvent("status.damage.tick",-1,status.StatusId,damage));
                }
                else if(definition.EffectId=="recharge_drain")
                {
                    int drained=0;foreach(var charge in state.AbilityStates){if(charge.RechargeProgress<=0)continue;charge.RechargeProgress--;drained++;}
                    events.Add(new GameEvent("status.curse.tick",-1,status.StatusId,drained));
                }
                if(unchangedSinceSnapshot){status.RemainingActions=remainingAfterTick;if(status.RemainingActions<=0)state.Statuses.RemoveAt(i);}
            }
        }

        public static bool HasEffect(RunState state,GameContent content,string effectId)
        {
            foreach(var status in state.Statuses)if(content.Status(status.StatusId).EffectId==effectId)return true;return false;
        }

        public static void Consume(RunState state,GameContent content,string statusId,int amount=1)
        {
            for(int i=state.Statuses.Count-1;i>=0;i--)
            {
                var status=state.Statuses[i];if(status.StatusId!=statusId)continue;status.RemainingActions-=Math.Max(1,amount);if(status.RemainingActions<=0)state.Statuses.RemoveAt(i);return;
            }
        }

        public static void Remove(RunState state,string statusId)
        {
            for(int i=state.Statuses.Count-1;i>=0;i--)
            {
                if(state.Statuses[i].StatusId!=statusId)continue;
                state.Statuses.RemoveAt(i);
                return;
            }
        }
    }
}
