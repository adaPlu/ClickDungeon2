using System;
using System.Collections.Generic;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Simulation.Progression
{
    public static class AchievementEvaluator
    {
        public static IEnumerable<string> Evaluate(GameContent content,RunState state,GameEvent evt)
        {
            if(content==null||state==null||evt==null)yield break;
            foreach(var achievement in content.Achievements)
            {
                if(!Matches(achievement.Trigger,evt.Type))continue;
                int value=ValueFor(state,evt);
                if(value>=achievement.MinimumValue)yield return achievement.Id;
            }
        }

        private static bool Matches(string trigger,string eventType)
        {
            if(string.IsNullOrEmpty(trigger)||string.IsNullOrEmpty(eventType))return false;
            if(trigger=="floor.entered")return eventType.StartsWith("floor.entered.",StringComparison.Ordinal);
            return string.Equals(trigger,eventType,StringComparison.Ordinal);
        }

        private static int ValueFor(RunState state,GameEvent evt)
        {
            if(evt.Type=="abyss.depth.entered")return evt.Amount;
            if(evt.Type.StartsWith("floor.entered.",StringComparison.Ordinal))return state.Floor;
            return evt.Amount;
        }
    }
}
