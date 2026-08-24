using System;
using System.Collections.Generic;
using System.Linq;
using ClickDungeon.Simulation.Combat;
using ClickDungeon.Simulation.Boss;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Rules;

namespace ClickDungeon.Simulation.Abilities
{
    public sealed class AbilityResolver
    {
        private readonly GameContent _content;
        public AbilityResolver(GameContent content) { _content = content; }

        public bool TryUse(RunState state, string abilityId, int targetIndex, List<GameEvent> events, out string rejection)
        {
            rejection = string.Empty;
            var charge = state.AbilityStates.FirstOrDefault(a => a.AbilityId == abilityId);
            if (charge == null) { rejection="ability.not_unlocked"; return false; }
            if (charge.Charges <= 0) { rejection="ability.no_charges"; return false; }
            if (_content.Ability(abilityId).ClassId != state.HeroClass) { rejection="ability.wrong_class"; return false; }

            bool used;
            switch (abilityId)
            {
                case "ability.knight.shield_wall": state.ShieldPoints += 4; used=true; events.Add(new GameEvent("ability.shield_wall",-1,abilityId,4)); break;
                case "ability.knight.taunt": used=ModifyIntent(state,targetIndex,MonsterIntentKind.Attack,events,abilityId); break;
                case "ability.knight.fortify": state.FortifyActions=Math.Max(state.FortifyActions,3); used=true; events.Add(new GameEvent("ability.fortify",-1,abilityId,3)); break;
                case "ability.knight.valiant_strike": used=DamageTarget(state,targetIndex,3,events,abilityId); break;
                case "ability.knight.guardians_oath": state.ShieldPoints += 7; state.FortifyActions=Math.Max(state.FortifyActions,2); used=true; events.Add(new GameEvent("ability.guardians_oath",-1,abilityId,7)); break;

                case "ability.ranger.piercing_shot": used=DamageLineTarget(state,targetIndex,2,events,abilityId); break;
                case "ability.ranger.rapid_volley": used=DamageTarget(state,targetIndex,1,events,abilityId,true); break;
                case "ability.ranger.camouflage": state.CamouflageActions=Math.Max(state.CamouflageActions,3); used=true; events.Add(new GameEvent("ability.camouflage",-1,abilityId,3)); break;
                case "ability.ranger.net_trap": used=RootTarget(state,targetIndex,2,events,abilityId); break;
                case "ability.ranger.eagle_eye": used=IdentifyLine(state,targetIndex,events,abilityId); break;

                case "ability.thief.trap_scan": used=IdentifyTraps(state,2,events,abilityId); break;
                case "ability.thief.shadowstep": used=Shadowstep(state,targetIndex,events,abilityId); break;
                case "ability.thief.disarm_expert": used=DisarmTrap(state,targetIndex,events,abilityId); break;
                case "ability.thief.ambush": used=Ambush(state,targetIndex,events,abilityId); break;
                case "ability.thief.veil_of_smoke": state.CamouflageActions=Math.Max(state.CamouflageActions,4); used=true; events.Add(new GameEvent("ability.veil_of_smoke",-1,abilityId,4)); break;

                case "ability.wizard.fireball": used=AreaDamage(state,targetIndex,3,1,events,abilityId); break;
                case "ability.wizard.frost_nova": used=FrostNova(state,events,abilityId); break;
                case "ability.wizard.chain_lightning": used=ChainLightning(state,events,abilityId); break;
                case "ability.wizard.arcane_shield": state.ShieldPoints += 5; used=true; events.Add(new GameEvent("ability.arcane_shield",-1,abilityId,5)); break;
                case "ability.wizard.meteor": used=Meteor(state,events,abilityId); break;
                default: rejection="ability.unsupported"; return false;
            }

            if (!used) { rejection="ability.invalid_target"; return false; }
            charge.Charges--;
            events.Add(new GameEvent("ability.used",targetIndex,abilityId));
            return true;
        }

        private bool DamageTarget(RunState state,int index,int bonus,List<GameEvent> events,string abilityId,bool doubleHit=false)
        {
            if (!TryLivingMonster(state,index,out var tile)) return false;
            int hits=doubleHit?2:1;
            for(int h=0;h<hits && tile.MonsterHp>0;h++)
            {
                int damage=DamageResolver.PlayerAttackDamage(state,tile,_content,bonus);
                tile.MonsterHp=Math.Max(0,tile.MonsterHp-damage);
                events.Add(new GameEvent("ability.damage",index,abilityId,damage));
            }
            ResolveDeath(state,tile,events);
            return true;
        }

        private bool DamageLineTarget(RunState state,int index,int bonus,List<GameEvent> events,string abilityId)
        {
            if(!TryLivingMonster(state,index,out var tile)) return false;
            var target=Position(index); var p=state.PlayerPosition;
            if(p.Row!=target.Row && p.Col!=target.Col) return false;
            return DamageTarget(state,index,bonus,events,abilityId);
        }

        private bool ModifyIntent(RunState state,int index,MonsterIntentKind intent,List<GameEvent> events,string abilityId)
        {
            if(!TryLivingMonster(state,index,out var tile)) return false;
            tile.IntentKind=intent; tile.IntentPower=Math.Max(1,tile.MonsterAttack-1);
            events.Add(new GameEvent("ability.intent_changed",index,abilityId,tile.IntentPower)); return true;
        }

        private bool RootTarget(RunState state,int index,int actions,List<GameEvent> events,string abilityId)
        {
            if(!TryLivingMonster(state,index,out var tile)) return false;
            tile.MonsterRootActions=Math.Max(tile.MonsterRootActions,actions); events.Add(new GameEvent("ability.root",index,abilityId,actions)); return true;
        }

        private bool IdentifyLine(RunState state,int index,List<GameEvent> events,string abilityId)
        {
            if(index<0||index>=state.Tiles.Count) return false;
            var target=Position(index); var p=state.PlayerPosition;
            int dr=Math.Sign(target.Row-p.Row); int dc=Math.Sign(target.Col-p.Col);
            if(dr!=0 && dc!=0) return false;
            int r=p.Row+dr,c=p.Col+dc,count=0;
            while(r>=0&&r<RunState.BoardSize&&c>=0&&c<RunState.BoardSize&&count<4)
            {
                int i=r*RunState.BoardSize+c; var tile=state.Tiles[i];
                if(tile.Visibility!=TileVisibility.Revealed) { tile.Visibility=TileVisibility.Identified; events.Add(new GameEvent("tile.identified",i,abilityId)); }
                r+=dr; c+=dc; count++;
            }
            return count>0;
        }

        private bool IdentifyTraps(RunState state,int radius,List<GameEvent> events,string abilityId)
        {
            int found=0;
            for(int i=0;i<state.Tiles.Count;i++)
            {
                var tile=state.Tiles[i]; if(tile.Content!=TileContentKind.Trap || tile.Visibility==TileVisibility.Revealed) continue;
                if(Manhattan(state.PlayerPosition,Position(i))<=radius) { tile.Visibility=TileVisibility.Identified; found++; events.Add(new GameEvent("trap.identified",i,abilityId)); }
            }
            events.Add(new GameEvent("ability.scan_complete",-1,abilityId,found));
            return true;
        }

        private bool Shadowstep(RunState state,int index,List<GameEvent> events,string abilityId)
        {
            if(index<0||index>=state.Tiles.Count) return false;
            var tile=state.Tiles[index]; if(tile.Visibility!=TileVisibility.Revealed || tile.Occupancy==OccupancyKind.Monster) return false;
            if(Manhattan(state.PlayerPosition,Position(index))>2) return false;
            int old=state.PlayerPosition.Row*RunState.BoardSize+state.PlayerPosition.Col; state.Tiles[old].Occupancy=OccupancyKind.None;
            tile.Occupancy=OccupancyKind.Player; state.PlayerPosition=Position(index); events.Add(new GameEvent("ability.shadowstep",index,abilityId)); return true;
        }

        private bool DisarmTrap(RunState state,int index,List<GameEvent> events,string abilityId)
        {
            if(index<0||index>=state.Tiles.Count) return false; var tile=state.Tiles[index];
            if(tile.Content!=TileContentKind.Trap || tile.Resolution!=TileResolution.Available || (tile.Visibility!=TileVisibility.Identified && tile.Visibility!=TileVisibility.Revealed)) return false;
            tile.Visibility=TileVisibility.Revealed; tile.Resolution=TileResolution.Resolved; tile.Occupancy=OccupancyKind.None; state.TilesResolved++;
            events.Add(new GameEvent("trap.disarmed",index,abilityId)); return true;
        }

        private bool Ambush(RunState state,int index,List<GameEvent> events,string abilityId)
        {
            if(!TryLivingMonster(state,index,out var tile)) return false; int bonus=tile.MonsterTurn==0?4:1; return DamageTarget(state,index,bonus,events,abilityId);
        }

        private bool AreaDamage(RunState state,int center,int baseDamage,int radius,List<GameEvent> events,string abilityId)
        {
            if(center<0||center>=state.Tiles.Count) return false; int hit=0; var pos=Position(center);
            for(int i=0;i<state.Tiles.Count;i++)
            {
                if(Manhattan(pos,Position(i))>radius || !TryLivingMonster(state,i,out var tile)) continue;
                int damage=Math.Max(1,baseDamage-tile.MonsterDefense/2); tile.MonsterHp=Math.Max(0,tile.MonsterHp-damage); hit++; events.Add(new GameEvent("ability.area_damage",i,abilityId,damage)); ResolveDeath(state,tile,events);
            }
            return hit>0;
        }

        private bool FrostNova(RunState state,List<GameEvent> events,string abilityId)
        {
            int hit=0; for(int i=0;i<state.Tiles.Count;i++) if(TryLivingMonster(state,i,out var tile) && Manhattan(state.PlayerPosition,Position(i))<=2) { tile.MonsterRootActions=Math.Max(tile.MonsterRootActions,2); hit++; events.Add(new GameEvent("ability.frozen",i,abilityId,2)); }
            return hit>0;
        }

        private bool ChainLightning(RunState state,List<GameEvent> events,string abilityId)
        {
            var targets=Enumerable.Range(0,state.Tiles.Count).Where(i=>TryLivingMonster(state,i,out _)).OrderBy(i=>Manhattan(state.PlayerPosition,Position(i))).ThenBy(i=>i).Take(3).ToArray();
            if(targets.Length==0) return false; int damage=4; foreach(int i in targets) { var tile=state.Tiles[i]; int dealt=Math.Max(1,damage-tile.MonsterDefense/2); tile.MonsterHp=Math.Max(0,tile.MonsterHp-dealt); events.Add(new GameEvent("ability.chain_damage",i,abilityId,dealt)); ResolveDeath(state,tile,events); damage=Math.Max(2,damage-1); } return true;
        }

        private bool Meteor(RunState state,List<GameEvent> events,string abilityId)
        {
            int hit=0; for(int i=0;i<state.Tiles.Count;i++) if(TryLivingMonster(state,i,out var tile)) { int damage=Math.Max(2,6-tile.MonsterDefense/2); tile.MonsterHp=Math.Max(0,tile.MonsterHp-damage); hit++; events.Add(new GameEvent("ability.meteor_damage",i,abilityId,damage)); ResolveDeath(state,tile,events); } return hit>0;
        }

        private static bool TryLivingMonster(RunState state,int index,out TileState tile)
        {
            tile=null; if(index<0||index>=state.Tiles.Count) return false; tile=state.Tiles[index]; return (tile.Content==TileContentKind.Monster||tile.Content==TileContentKind.Boss)&&tile.Visibility==TileVisibility.Revealed&&tile.Resolution==TileResolution.Available&&tile.MonsterHp>0;
        }

        private static void ResolveDeath(RunState state,TileState tile,List<GameEvent> events)
        {
            if(tile.MonsterHp>0){BossResolver.AfterDamage(state,tile,events);return;} tile.Occupancy=OccupancyKind.None; tile.Resolution=TileResolution.Resolved; state.MonstersDefeated++; state.TilesResolved++; if(tile.Content==TileContentKind.Boss) state.BossDefeated=true; events.Add(new GameEvent(tile.Content==TileContentKind.Boss?"boss.defeated":"monster.defeated",tile.Index,tile.ContentId));
        }
        private static GridPosition Position(int index)=>new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);
        private static int Manhattan(GridPosition a,GridPosition b)=>Math.Abs(a.Row-b.Row)+Math.Abs(a.Col-b.Col);
    }
}
