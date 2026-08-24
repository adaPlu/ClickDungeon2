using System;
using System.Collections.Generic;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Status;

namespace ClickDungeon.Simulation.Combat
{
    public static class MonsterIntentResolver
    {
        public static void Initialize(TileState tile, MonsterDefinition definition)
        {
            tile.IntentKind = definition.PrimaryIntent;
            tile.IntentPower = definition.IntentPower;
        }

        public static void Resolve(RunState state, TileState monster, GameContent content, List<GameEvent> events)
        {
            if (monster.MonsterHp <= 0 || monster.Resolution == TileResolution.Resolved) return;
            if (monster.MonsterRootActions > 0)
            {
                monster.MonsterRootActions--;
                events.Add(new GameEvent("monster.intent.delayed", monster.Index, monster.ContentId));
                if(monster.ContentId=="monster.troll"&&monster.MonsterHp>0){int before=monster.MonsterHp;monster.MonsterHp=Math.Min(monster.MonsterMaxHp,monster.MonsterHp+1);if(monster.MonsterHp>before)events.Add(new GameEvent("monster.regenerated",monster.Index,monster.ContentId,1));}
            AdvanceIntent(monster);
                return;
            }

            switch (monster.IntentKind)
            {
                case MonsterIntentKind.StealGold:
                    if (state.Gold > 0)
                    {
                        int stolen = Math.Min(Math.Max(1, monster.IntentPower), state.Gold);
                        state.Gold -= stolen;
                        events.Add(new GameEvent("monster.gold_stolen", monster.Index, monster.ContentId, stolen));
                    }
                    else DealDamage(state, monster, content, events, monster.MonsterAttack);
                    break;
                case MonsterIntentKind.ApplyPoison:
                    DealDamage(state, monster, content, events, Math.Max(1, monster.IntentPower));
                    StatusResolver.AddOrRefresh(state,content,"status.poison");
                    events.Add(new GameEvent("status.applied", -1, "status.poison", 3));
                    break;
                case MonsterIntentKind.Guard:
                    monster.MonsterGuarding = true;
                    events.Add(new GameEvent("monster.guarding", monster.Index, monster.ContentId));
                    if(monster.ContentId=="monster.cultist")BuffAlly(state,monster,events);
                    break;
                case MonsterIntentKind.HeavyAttack:
                    DealDamage(state, monster, content, events, Math.Max(monster.MonsterAttack, monster.IntentPower));
                    break;
                case MonsterIntentKind.Hazard:
                    DealDamage(state, monster, content, events, Math.Max(1, monster.IntentPower));
                    PaintHazard(state,monster,events);
                    events.Add(new GameEvent("monster.hazard", monster.Index, monster.ContentId, monster.IntentPower));
                    break;
                case MonsterIntentKind.Summon:
                    DealDamage(state, monster, content, events, Math.Max(1, monster.MonsterAttack - 1));
                    TrySummon(state,monster,content,events);
                    break;
                default:
                    DealDamage(state, monster, content, events, Math.Max(monster.MonsterAttack, monster.IntentPower));
                    break;
            }
            ApplyVariantBehavior(state,monster,content,events);
            AdvanceIntent(monster);
        }

        private static void ApplyVariantBehavior(RunState state,TileState monster,GameContent content,List<GameEvent> events)
        {
            switch(monster.VariantId)
            {
                case "variant.frost":monster.MonsterRootActions=Math.Max(monster.MonsterRootActions,1);events.Add(new GameEvent("variant.frost.intent_delayed",monster.Index,monster.ContentId));break;
                case "variant.venomous":StatusResolver.AddOrRefresh(state,content,"status.poison",2);events.Add(new GameEvent("variant.venomous.poison",monster.Index,monster.ContentId,2));break;
                case "variant.ember":StatusResolver.AddOrRefresh(state,content,"status.burn",2);events.Add(new GameEvent("variant.ember.burn",monster.Index,monster.ContentId,2));break;
                case "variant.arcane":foreach(var a in state.AbilityStates){a.RechargeProgress=Math.Max(0,a.RechargeProgress-1);}events.Add(new GameEvent("variant.arcane.disrupt",monster.Index,monster.ContentId,1));break;
            }
        }

        private static int DealDamage(RunState state, TileState monster, GameContent content, List<GameEvent> events, int raw)
        {
            if(monster.ContentId=="monster.wolf")raw+=CountOtherWolves(state,monster)>0?1:0;
            int applied = DamageResolver.ApplyIncoming(state, raw, content);
            events.Add(new GameEvent("player.damaged", -1, monster.ContentId, applied));
            if(monster.ContentId=="monster.vampire"&&applied>0){int before=monster.MonsterHp;monster.MonsterHp=Math.Min(monster.MonsterMaxHp,monster.MonsterHp+applied);if(monster.MonsterHp>before)events.Add(new GameEvent("monster.life_stolen",monster.Index,monster.ContentId,monster.MonsterHp-before));}
            if (state.GameOver) events.Add(new GameEvent("run.game_over"));
            return applied;
        }


        private static int CountOtherWolves(RunState state,TileState monster)
        {
            int count=0;foreach(var t in state.Tiles)if(t.Index!=monster.Index&&t.ContentId=="monster.wolf"&&t.Visibility==TileVisibility.Revealed&&t.MonsterHp>0)count++;return count;
        }

        private static void BuffAlly(RunState state,TileState cultist,List<GameEvent> events)
        {
            foreach(var ally in state.Tiles){if(ally.Index==cultist.Index||ally.Occupancy!=OccupancyKind.Monster||ally.MonsterHp<=0)continue;ally.IntentPower+=1;events.Add(new GameEvent("monster.ally_buffed",ally.Index,cultist.ContentId,1));return;}
        }

        private static void TrySummon(RunState state,TileState summoner,GameContent content,List<GameEvent> events)
        {
            string id=summoner.ContentId.Contains("lich")?"monster.skeleton":"monster.bat";var def=content.Monster(id);foreach(var tile in state.Tiles){if(tile.Content!=TileContentKind.Empty||tile.Resolution!=TileResolution.Available||tile.Visibility==TileVisibility.Revealed)continue;tile.Content=TileContentKind.Monster;tile.ContentId=id;tile.Clue=ClueFamily.Danger;tile.Visibility=TileVisibility.Clued;tile.MonsterMaxHp=def.Hp;tile.MonsterHp=def.Hp;tile.MonsterAttack=def.Attack;tile.MonsterDefense=def.Defense;tile.ThreatPattern=def.ThreatPattern;tile.IntentKind=def.PrimaryIntent;tile.IntentPower=def.IntentPower;events.Add(new GameEvent("monster.summoned",tile.Index,id));return;}events.Add(new GameEvent("monster.summon_failed",summoner.Index,summoner.ContentId));
        }

        private static void PaintHazard(RunState state,TileState source,List<GameEvent> events)
        {
            TerrainKind terrain=source.ContentId.Contains("dragon")||source.ContentId.Contains("demon")||source.ContentId.Contains("hellhound")?TerrainKind.Lava:TerrainKind.Arcane;foreach(var tile in state.Tiles){if(tile.Index==source.Index||tile.Content==TileContentKind.SafeExit||tile.Content==TileContentKind.ForbiddenExit)continue;tile.Terrain=terrain;tile.TerrainTriggered=false;if(tile.Visibility==TileVisibility.Hidden){tile.Visibility=TileVisibility.Clued;tile.Clue=ClueFamily.Danger;}events.Add(new GameEvent("monster.terrain_hazard",tile.Index,source.ContentId));return;}
        }

        private static void AdvanceIntent(TileState monster)
        {
            monster.MonsterTurn++;
            if (monster.ContentId == "monster.goblin") monster.IntentKind = monster.MonsterTurn % 2 == 0 ? MonsterIntentKind.StealGold : MonsterIntentKind.Attack;
            else if (monster.ContentId == "monster.slime") monster.IntentKind = monster.MonsterTurn % 2 == 0 ? MonsterIntentKind.ApplyPoison : MonsterIntentKind.Attack;
            else if (monster.ContentId == "monster.golem") monster.IntentKind = monster.MonsterTurn % 2 == 0 ? MonsterIntentKind.HeavyAttack : MonsterIntentKind.Guard;
        }

    }
}
