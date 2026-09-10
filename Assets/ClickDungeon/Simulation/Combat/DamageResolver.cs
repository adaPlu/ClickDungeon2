using System;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Status;

namespace ClickDungeon.Simulation.Combat
{
    public static class DamageResolver
    {
        public static int PlayerAttackDamage(RunState state, TileState monster, GameContent content, int bonus = 0)
        {
            int weapon = 0;
            if (!string.IsNullOrEmpty(state.EquippedWeaponId) && content.TryItem(state.EquippedWeaponId, out var item)) weapon = item.Attack;
            int affixBonus = 0;
            if (state.EquippedWeaponAffixId == "affix.keen" && monster.MonsterHp == monster.MonsterMaxHp) affixBonus = 1;
            int temporaryAttack=(state.TemporaryAttackActionsRemaining>0||state.TemporaryAttackResponsesRemaining>0)?state.TemporaryAttackBonus:0;
            int berserkerPassive=state.HeroClass==HeroClassId.Berserker&&state.Hp*2<=state.MaxHp?1:0;
            int raw = Math.Max(1, state.Attack + temporaryAttack + berserkerPassive + weapon + bonus + affixBonus - monster.MonsterDefense - (monster.MonsterGuarding ? 1 : 0));
            monster.MonsterGuarding = false;
            return raw;
        }

        public static int ApplyIncoming(RunState state, int rawDamage, GameContent content)
        {
            int armor = 0;
            if (!string.IsNullOrEmpty(state.EquippedArmorId) && content.TryItem(state.EquippedArmorId, out var item)) armor = item.Defense;
            int affixDefense = state.EquippedArmorAffixId == "affix.vital" ? 1 : 0;
            int temporaryDefense=state.TemporaryDefenseResponsesRemaining>0?state.TemporaryDefenseBonus:0;
            int defense = state.Defense + temporaryDefense + armor + affixDefense + (state.FortifyActions > 0 ? 1 : 0) + (state.Defending ? Math.Max(1, state.Defense) : 0);
            bool vulnerable=StatusResolver.HasEffect(state,content,"incoming_damage_plus_one");
            int damage = Math.Max(1, rawDamage + (vulnerable?1:0) - defense);
            if (state.ShieldPoints > 0)
            {
                int absorbed = Math.Min(state.ShieldPoints, damage);
                state.ShieldPoints -= absorbed;
                damage -= absorbed;
            }
            int hpBefore=state.Hp;
            if (damage > 0) state.Hp = Math.Max(0, state.Hp - damage);
            if(state.HeroClass==HeroClassId.Paladin&&!state.PaladinHalfHpPassiveTriggered&&damage>0&&hpBefore*2>state.MaxHp&&state.Hp*2<=state.MaxHp)
            {
                state.PaladinHalfHpPassiveTriggered=true;
                state.ShieldPoints+=3;
            }
            if(vulnerable)StatusResolver.Remove(state,"status.vulnerable");
            state.Defending = false;
            if (state.FortifyActions > 0) state.FortifyActions--;
            if (state.Hp <= 0) state.GameOver = true;
            return damage;
        }
    }
}
