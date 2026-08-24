using System;
using System.Collections.Generic;
using ClickDungeon.Application.State;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Content
{
    public static class ContentMigrationService
    {
        public static void Apply(SlotSavePayload payload,GameContent content)
        {
            if(payload==null||content==null)return;
            if(payload.Meta!=null)
            {
                for(int i=0;i<payload.Meta.UnlockedAbilityIds.Count;i++)payload.Meta.UnlockedAbilityIds[i]=content.MigrateContentId(payload.Meta.UnlockedAbilityIds[i]);
                Deduplicate(payload.Meta.UnlockedAbilityIds);
            }
            var state=payload.ActiveRun;if(state==null)return;
            state.BiomeId=content.MigrateContentId(state.BiomeId);state.ArchetypeId=content.MigrateContentId(state.ArchetypeId);
            state.EquippedWeaponId=content.MigrateContentId(state.EquippedWeaponId);state.EquippedWeaponAffixId=content.MigrateContentId(state.EquippedWeaponAffixId);state.EquippedArmorId=content.MigrateContentId(state.EquippedArmorId);state.EquippedArmorAffixId=content.MigrateContentId(state.EquippedArmorAffixId);
            for(int i=0;i<state.InventoryItemIds.Count;i++)state.InventoryItemIds[i]=content.MigrateContentId(state.InventoryItemIds[i]);
            for(int i=0;i<state.ItemInstances.Count;i++){state.ItemInstances[i].BaseItemId=content.MigrateContentId(state.ItemInstances[i].BaseItemId);state.ItemInstances[i].AffixId=content.MigrateContentId(state.ItemInstances[i].AffixId);}
            for(int i=0;i<state.AbilityStates.Count;i++)state.AbilityStates[i].AbilityId=content.MigrateContentId(state.AbilityStates[i].AbilityId);
            for(int i=0;i<state.Statuses.Count;i++)state.Statuses[i].StatusId=content.MigrateContentId(state.Statuses[i].StatusId);
            for(int i=0;i<state.Tiles.Count;i++){state.Tiles[i].ContentId=content.MigrateContentId(state.Tiles[i].ContentId);state.Tiles[i].VariantId=content.MigrateContentId(state.Tiles[i].VariantId);}
        }

        private static void Deduplicate(List<string> values)
        {
            var seen=new HashSet<string>(StringComparer.Ordinal);for(int i=values.Count-1;i>=0;i--)if(string.IsNullOrEmpty(values[i])||!seen.Add(values[i]))values.RemoveAt(i);
        }
    }
}
