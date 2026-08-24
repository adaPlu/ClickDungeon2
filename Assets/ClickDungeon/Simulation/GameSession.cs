using System;
using System.Collections.Generic;
using ClickDungeon.Simulation.Abilities;
using ClickDungeon.Simulation.Biome;
using ClickDungeon.Simulation.Boss;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Combat;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Loot;
using ClickDungeon.Simulation.Rules;
using ClickDungeon.Simulation.Status;

namespace ClickDungeon.Simulation
{
    public sealed class GameSession
    {
        private readonly FloorGenerator _generator;
        private readonly GameContent _content;
        private readonly AbilityResolver _abilities;
        private readonly LootResolver _loot;
        public RunState State { get; }

        public GameSession(RunState state, FloorGenerator generator, GameContent content=null)
        {
            State=state??throw new ArgumentNullException(nameof(state)); _generator=generator??throw new ArgumentNullException(nameof(generator)); _content=content??generator.Content; _abilities=new AbilityResolver(_content); _loot=new LootResolver(_content);
        }

        public CommandResult Apply(GameCommand command)
        {
            if(command==null)return CommandResult.Reject("command.null");
            if(State.GameOver)return CommandResult.Reject("run.game_over");
            if(State.CampaignCompleted)return CommandResult.Reject("run.completed");
            var events=new List<GameEvent>(); CommandResult result;
            if(command is RevealTileCommand reveal) result=Reveal(reveal.TileIndex,events);
            else if(command is MoveCommand move) result=Move(move.TileIndex,events);
            else if(command is InteractCommand interact) result=Interact(interact.TileIndex,events);
            else if(command is AttackCommand attack) result=Attack(attack.TileIndex,events);
            else if(command is DefendCommand) result=Defend(events);
            else if(command is UseAbilityCommand ability) result=UseAbility(ability,events);
            else if(command is UseItemCommand item) result=UseItem(item.ItemId,item.TargetTileIndex,events);
            else if(command is ChooseShrineCommand shrine) result=ChooseShrine(shrine,events);
            else if(command is BuyItemCommand buy) result=BuyItem(buy,events);
            else if(command is EquipItemCommand equip) result=EquipItem(equip.ItemId,equip.InstanceId,events);
            else if(command is TakeSafeExitCommand safe) result=Exit(safe.TileIndex,false,events);
            else if(command is TakeForbiddenExitCommand hard) result=Exit(hard.TileIndex,true,events);
            else if(command is UnlockVaultCommand vault) result=UnlockVault(vault.TileIndex,events);
            else result=CommandResult.Reject("command.unsupported");

            if(result.Accepted)
            {
                State.CommandNumber++;
                if(ConsumesMeaningfulAction(command) && !State.CampaignCompleted && !State.GameOver) StatusResolver.AdvanceMeaningfulAction(State,_content,events);
            }
            return result;
        }

        private CommandResult Reveal(int index,List<GameEvent> events)
        {
            if(!TryTile(index,out var tile))return CommandResult.Reject("tile.out_of_range");
            if(tile.Visibility==TileVisibility.Revealed)return CommandResult.Reject("tile.already_revealed");
            if(!IsAdjacent(index))return CommandResult.Reject("tile.not_adjacent");
            if(State.CamouflageActions<=0 && ThreatResolver.IsThreatened(State,index))return CommandResult.Reject("tile.threatened");
            bool wasIdentified=tile.Visibility==TileVisibility.Identified; tile.Visibility=TileVisibility.Revealed;
            tile.Occupancy=(tile.Content==TileContentKind.Monster||tile.Content==TileContentKind.Boss)?OccupancyKind.Monster:tile.Content==TileContentKind.Trap?OccupancyKind.Hazard:OccupancyKind.None;
            events.Add(new GameEvent("tile.revealed",index,tile.ContentId));
            if(tile.Content==TileContentKind.Monster||tile.Content==TileContentKind.Boss) events.Add(new GameEvent(tile.Content==TileContentKind.Boss?"boss.encountered":"monster.encountered",index,tile.ContentId,tile.IntentPower));
            else if(tile.Content==TileContentKind.Trap)
            {
                if(wasIdentified){events.Add(new GameEvent("trap.exposed_safe",index,tile.ContentId));}
                else{var trap=_content.Trap(tile.ContentId);tile.Resolution=TileResolution.Resolved;tile.Occupancy=OccupancyKind.None;State.Hp=Math.Max(0,State.Hp-trap.Damage);State.TilesResolved++;if(!string.IsNullOrEmpty(trap.StatusId)){StatusResolver.AddOrRefresh(State,_content,trap.StatusId,trap.StatusDuration);if(trap.StatusId=="status.root")State.RootedActions=Math.Max(State.RootedActions,Math.Max(1,trap.StatusDuration));}events.Add(new GameEvent("trap.triggered",index,tile.ContentId,trap.Damage));if(State.Hp<=0){State.GameOver=true;events.Add(new GameEvent("run.game_over"));}}
            }
            GainRecharge(1,events); return CommandResult.Accept(events);
        }

        private CommandResult Move(int index,List<GameEvent> events)
        {
            if(!TryTile(index,out var tile))return CommandResult.Reject("tile.out_of_range");
            if(State.RootedActions>0){State.RootedActions--;StatusResolver.Consume(State,_content,"status.root");return CommandResult.Reject("player.rooted");}
            if(!IsAdjacent(index))return CommandResult.Reject("tile.not_adjacent");
            if(tile.Visibility!=TileVisibility.Revealed)return CommandResult.Reject("tile.not_revealed");
            if(tile.Occupancy==OccupancyKind.Monster)return CommandResult.Reject("tile.blocked_by_monster");
            if(State.CamouflageActions<=0&&State.ShieldPoints<=0&&ThreatResolver.IsThreatened(State,index))return CommandResult.Reject("tile.threatened");
            int old=Index(State.PlayerPosition);State.Tiles[old].Occupancy=OccupancyKind.None;tile.Occupancy=OccupancyKind.Player;State.PlayerPosition=Position(index);events.Add(new GameEvent("player.moved",index));TerrainResolver.ResolveEntry(State,old,index,_content,events);
            if(tile.Terrain==TerrainKind.Arcane&&tile.TerrainTriggered)GainRecharge(1,events);
            int slide=TerrainResolver.TryIceSlideTarget(State,old,index);if(slide>=0){tile.Occupancy=OccupancyKind.None;State.Tiles[slide].Occupancy=OccupancyKind.Player;State.PlayerPosition=Position(slide);events.Add(new GameEvent("terrain.ice.slide",slide));TerrainResolver.ResolveEntry(State,index,slide,_content,events);}
            return CommandResult.Accept(events);
        }

        private CommandResult Interact(int index,List<GameEvent> events)
        {
            if(!TryTile(index,out var tile))return CommandResult.Reject("tile.out_of_range");
            if(!IsAdjacent(index)&&index!=Index(State.PlayerPosition))return CommandResult.Reject("tile.not_adjacent");
            if(tile.Visibility!=TileVisibility.Revealed||tile.Resolution!=TileResolution.Available)return CommandResult.Reject("tile.not_interactable");
            switch(tile.Content)
            {
                case TileContentKind.Gold:State.Gold+=Math.Max(1,tile.Amount);ResolveTile(tile);events.Add(new GameEvent("gold.collected",index,tile.ContentId,tile.Amount));break;
                case TileContentKind.SmallKey:State.SmallKeys++;ResolveTile(tile);events.Add(new GameEvent("key.small.collected",index));break;
                case TileContentKind.BigKey:if(State.BigKeys>=_content.Balance.BigKeyMaxCarry)return CommandResult.Reject("key.big.carry_limit");State.BigKeys++;ResolveTile(tile);events.Add(new GameEvent("key.big.collected",index));break;
                case TileContentKind.Chest:if(State.SmallKeys<=0)return CommandResult.Reject("key.small.required");State.SmallKeys--;AwardLoot("loot.chest.standard",index,events);ResolveTile(tile);events.Add(new GameEvent("chest.opened",index,tile.ContentId));break;
                case TileContentKind.Consumable:AddOwnedItem(tile.ContentId,string.Empty);ResolveTile(tile);events.Add(new GameEvent("item.collected",index,tile.ContentId));break;
                case TileContentKind.Equipment:AddOwnedItem(tile.ContentId,string.Empty);ResolveTile(tile);events.Add(new GameEvent("item.collected",index,tile.ContentId));break;
                case TileContentKind.Merchant:events.Add(new GameEvent("merchant.opened",index,tile.ContentId));return CommandResult.Accept(events);
                case TileContentKind.Shrine:events.Add(new GameEvent("shrine.choice_required",index,tile.ContentId));return CommandResult.Accept(events);
                default:return CommandResult.Reject("tile.requires_specific_command");
            }
            GainRecharge(1,events);return CommandResult.Accept(events);
        }

        private CommandResult Attack(int index,List<GameEvent> events)
        {
            if(!TryLivingMonster(index,out var tile))return CommandResult.Reject("monster.not_attackable");
            if(!IsAdjacent(index))return CommandResult.Reject("monster.not_adjacent");
            int damage=DamageResolver.PlayerAttackDamage(State,tile,_content);tile.MonsterHp=Math.Max(0,tile.MonsterHp-damage);events.Add(new GameEvent("monster.damaged",index,tile.ContentId,damage));ApplyWeaponAffixOnHit(tile,events);
            if(tile.MonsterHp==0)DefeatMonster(tile,events);else{BossResolver.AfterDamage(State,tile,events);MonsterIntentResolver.Resolve(State,tile,_content,events);}
            return CommandResult.Accept(events);
        }

        private CommandResult Defend(List<GameEvent> events)
        {
            var enemy=FirstAdjacentMonster(); if(enemy<0)return CommandResult.Reject("combat.no_adjacent_enemy"); State.Defending=true;events.Add(new GameEvent("player.defending"));MonsterIntentResolver.Resolve(State,State.Tiles[enemy],_content,events);return CommandResult.Accept(events);
        }

        private CommandResult UseAbility(UseAbilityCommand command,List<GameEvent> events)
        {
            if(!_abilities.TryUse(State,command.AbilityId,command.TargetTileIndex,events,out string rejection))return CommandResult.Reject(rejection);
            int killProgress=0;for(int i=0;i<events.Count;i++){if(events[i].Type=="monster.defeated")killProgress+=2;else if(events[i].Type=="boss.defeated")killProgress+=4;}if(killProgress>0)GainRecharge(killProgress,events);
            if(command.TargetTileIndex>=0&&TryLivingMonster(command.TargetTileIndex,out var tile)&&tile.MonsterHp>0&&IsAdjacent(command.TargetTileIndex))MonsterIntentResolver.Resolve(State,tile,_content,events);
            return CommandResult.Accept(events);
        }

        private CommandResult UseItem(string itemId,int targetTileIndex,List<GameEvent> events)
        {
            int inventoryIndex=State.InventoryItemIds.IndexOf(itemId);if(inventoryIndex<0)return CommandResult.Reject("item.not_owned");if(!_content.TryItem(itemId,out var item))return CommandResult.Reject("item.unknown");
            if(item.Kind!="consumable")return CommandResult.Reject("item.not_consumable");
            if(itemId=="item.trap_disarm_kit")
            {
                if(targetTileIndex<0)return CommandResult.Reject("item.target_required");
                if(!TryTile(targetTileIndex,out var trapTile))return CommandResult.Reject("tile.out_of_range");
                if(!IsAdjacent(targetTileIndex)&&targetTileIndex!=Index(State.PlayerPosition))return CommandResult.Reject("tile.not_adjacent");
                if(trapTile.Content!=TileContentKind.Trap||trapTile.Resolution!=TileResolution.Available||(trapTile.Visibility!=TileVisibility.Identified&&trapTile.Visibility!=TileVisibility.Revealed))return CommandResult.Reject("trap.not_disarmable");
                State.InventoryItemIds.RemoveAt(inventoryIndex);ResolveTile(trapTile);events.Add(new GameEvent("trap.disarmed",targetTileIndex,trapTile.ContentId));GainRecharge(1,events);var reactingEnemy=FirstAdjacentMonster();if(reactingEnemy>=0)MonsterIntentResolver.Resolve(State,State.Tiles[reactingEnemy],_content,events);return CommandResult.Accept(events);
            }
            if(item.Heal<=0)return CommandResult.Reject("item.effect_not_implemented");
            State.InventoryItemIds.RemoveAt(inventoryIndex);int before=State.Hp;int heal=item.Heal+(State.EquippedArmorAffixId=="affix.vital"?2:0);State.Hp=Math.Min(State.MaxHp,State.Hp+heal);events.Add(new GameEvent("item.healed",-1,itemId,State.Hp-before));
            var enemy=FirstAdjacentMonster();if(enemy>=0)MonsterIntentResolver.Resolve(State,State.Tiles[enemy],_content,events);return CommandResult.Accept(events);
        }

        private CommandResult ChooseShrine(ChooseShrineCommand command,List<GameEvent> events)
        {
            if(!IsAdjacent(command.TileIndex)&&command.TileIndex!=Index(State.PlayerPosition))return CommandResult.Reject("tile.not_adjacent");
            if(!TryTile(command.TileIndex,out var tile)||tile.Content!=TileContentKind.Shrine||tile.Visibility!=TileVisibility.Revealed||tile.Resolution!=TileResolution.Available)return CommandResult.Reject("shrine.not_available");
            switch(command.Choice){case ShrineChoice.MaxHp:State.MaxHp+=3;State.Hp+=3;break;case ShrineChoice.Attack:State.Attack+=1;break;case ShrineChoice.Defense:State.Defense+=1;break;}
            ResolveTile(tile);events.Add(new GameEvent("shrine.chosen",command.TileIndex,command.Choice.ToString()));GainRecharge(1,events);return CommandResult.Accept(events);
        }

        private CommandResult BuyItem(BuyItemCommand command,List<GameEvent> events)
        {
            if(!IsAdjacent(command.MerchantTileIndex)&&command.MerchantTileIndex!=Index(State.PlayerPosition))return CommandResult.Reject("tile.not_adjacent");
            if(!TryTile(command.MerchantTileIndex,out var merchant)||merchant.Content!=TileContentKind.Merchant||merchant.Visibility!=TileVisibility.Revealed)return CommandResult.Reject("merchant.not_available");
            if(!_content.TryItem(command.ItemId,out var item))return CommandResult.Reject("item.unknown");var shop=_content.Shop(merchant.ContentId=="merchant.standard"?"shop.standard":merchant.ContentId.Replace("merchant.","shop."));if(Array.IndexOf(shop.StockItemIds,item.Id)<0)return CommandResult.Reject("merchant.item_not_stocked");if(State.Gold<item.Price)return CommandResult.Reject("gold.insufficient");State.Gold-=item.Price;AddOwnedItem(item.Id,string.Empty);events.Add(new GameEvent("merchant.item_bought",command.MerchantTileIndex,item.Id,item.Price));return CommandResult.Accept(events);
        }

        private CommandResult EquipItem(string itemId,string instanceId,List<GameEvent> events)
        {
            if(!_content.TryItem(itemId,out var item))return CommandResult.Reject("item.unknown");var instance=FindOwnedEquipment(itemId,instanceId);if(instance==null&&!State.InventoryItemIds.Contains(itemId))return CommandResult.Reject("item.not_owned");string affix=instance?.AffixId??string.Empty;
            if(item.Kind=="weapon"){State.EquippedWeaponId=itemId;State.EquippedWeaponAffixId=affix;}else if(item.Kind=="armor"){State.EquippedArmorId=itemId;State.EquippedArmorAffixId=affix;}else return CommandResult.Reject("item.not_equipment");events.Add(new GameEvent("item.equipped",-1,string.IsNullOrEmpty(affix)?itemId:itemId+"+"+affix));return CommandResult.Accept(events);
        }

        private CommandResult UnlockVault(int index,List<GameEvent> events)
        {
            if(!IsAdjacent(index)&&index!=Index(State.PlayerPosition))return CommandResult.Reject("tile.not_adjacent");
            if(!TryTile(index,out var tile)||tile.Content!=TileContentKind.SealedVault||tile.Visibility!=TileVisibility.Revealed||tile.Resolution!=TileResolution.Available)return CommandResult.Reject("vault.not_available");if(State.BigKeys<=0)return CommandResult.Reject("key.big.required");State.BigKeys--;AwardLoot("loot.vault.sealed",index,events);ResolveTile(tile);events.Add(new GameEvent("vault.opened",index,tile.ContentId));GainRecharge(2,events);return CommandResult.Accept(events);
        }

        private CommandResult Exit(int index,bool forbidden,List<GameEvent> events)
        {
            if(!TryTile(index,out var tile))return CommandResult.Reject("tile.out_of_range");if(!IsAdjacent(index)&&index!=Index(State.PlayerPosition))return CommandResult.Reject("tile.not_adjacent");var required=forbidden?TileContentKind.ForbiddenExit:TileContentKind.SafeExit;if(tile.Content!=required||tile.Visibility!=TileVisibility.Revealed)return CommandResult.Reject("exit.not_available");if(State.BossRequired&&!State.BossDefeated)return CommandResult.Reject("boss.must_be_defeated");if(forbidden&&State.BigKeys<=0)return CommandResult.Reject("key.big.required");if(forbidden)State.BigKeys--;
            int nextFloor=State.Floor+1;
            int campaignLimit=State.CampaignFloorLimit>0?Math.Min(State.CampaignFloorLimit,_content.Balance.CampaignFloors):_content.Balance.CampaignFloors;
            if(State.Mode==RunMode.Campaign&&nextFloor>campaignLimit&&campaignLimit<_content.Balance.CampaignFloors)return CommandResult.Reject("entitlement.full_game_required");
            if(State.Mode==RunMode.Campaign&&nextFloor>_content.Balance.CampaignFloors){State.CampaignCompleted=true;events.Add(new GameEvent("campaign.completed"));return CommandResult.Accept(events);}
            if(State.Mode==RunMode.Abyss){State.AbyssDepth=Math.Max(1,State.AbyssDepth+1);nextFloor=_content.Balance.CampaignFloors+State.AbyssDepth;}
            _generator.GenerateFloor(State,nextFloor,forbidden?RouteModifier.Forbidden:RouteModifier.Standard);events.Add(new GameEvent(State.Mode==RunMode.Abyss?"abyss.depth.entered":forbidden?"floor.entered.forbidden":"floor.entered.safe",-1,State.BiomeId,State.Mode==RunMode.Abyss?State.AbyssDepth:nextFloor));GainRecharge(2,events);return CommandResult.Accept(events);
        }

        private void DefeatMonster(TileState tile,List<GameEvent> events)
        {
            tile.Occupancy=OccupancyKind.None;tile.Resolution=TileResolution.Resolved;State.MonstersDefeated++;State.TilesResolved++;if(tile.Content==TileContentKind.Boss){State.BossDefeated=true;events.Add(new GameEvent("boss.defeated",tile.Index,tile.ContentId));}else events.Add(new GameEvent("monster.defeated",tile.Index,tile.ContentId));GainRecharge(tile.Content==TileContentKind.Boss?4:2,events);
        }

        private void GainRecharge(int amount,List<GameEvent> events)
        {
            foreach(var charge in State.AbilityStates){var def=_content.Ability(charge.AbilityId);int before=charge.Charges;charge.GainProgress(amount,def.RechargeProgressRequired,def.MaxCharges);if(charge.Charges>before)events.Add(new GameEvent("ability.charge_restored",-1,charge.AbilityId,charge.Charges-before));}events.Add(new GameEvent("ability.recharge_progress",-1,"",amount));
        }

        private void ApplyWeaponAffixOnHit(TileState target,List<GameEvent> events)
        {
            switch(State.EquippedWeaponAffixId)
            {
                case "affix.flaming":
                    events.Add(new GameEvent("affix.burn.applied",target.Index,target.ContentId,1));
                    target.MonsterHp=Math.Max(0,target.MonsterHp-1);
                    break;
                case "affix.frost":
                    target.MonsterRootActions=Math.Max(target.MonsterRootActions,1);events.Add(new GameEvent("affix.intent_delayed",target.Index,target.ContentId,1));
                    break;
                case "affix.storm":
                    foreach(int adjacent in LivingMonstersAdjacentTo(target.Index))
                    {
                        var chained=State.Tiles[adjacent];
                        chained.MonsterHp=Math.Max(0,chained.MonsterHp-1);
                        events.Add(new GameEvent("affix.chain",adjacent,chained.ContentId,1));
                        if(chained.MonsterHp==0)DefeatMonster(chained,events);else BossResolver.AfterDamage(State,chained,events);
                        break;
                    }
                    break;
            }
        }

        private void AwardLoot(string tableId,int tileIndex,List<GameEvent> events)
        {
            var reward=_loot.Roll(State,tableId,tileIndex);
            if(reward.Gold>0){State.Gold+=reward.Gold;events.Add(new GameEvent("loot.gold",tileIndex,"currency.gold",reward.Gold));}
            if(reward.Item!=null){State.ItemInstances.Add(reward.Item);if(_content.TryItem(reward.Item.BaseItemId,out var def)&&def.Kind=="consumable")State.InventoryItemIds.Add(reward.Item.BaseItemId);events.Add(new GameEvent("loot.item",tileIndex,string.IsNullOrEmpty(reward.Item.AffixId)?reward.Item.BaseItemId:reward.Item.BaseItemId+"+"+reward.Item.AffixId));}
        }

        private void AddOwnedItem(string itemId,string affixId)
        {
            if(_content.TryItem(itemId,out var def)&&(def.Kind=="weapon"||def.Kind=="armor"))State.ItemInstances.Add(new ItemInstanceState{InstanceId=$"shop-{State.Floor}-{State.CommandNumber}-{State.ItemInstances.Count}",BaseItemId=itemId,AffixId=affixId});
            else State.InventoryItemIds.Add(itemId);
        }

        private ItemInstanceState FindOwnedEquipment(string itemId,string instanceId="")
        {
            if(!string.IsNullOrEmpty(instanceId))for(int i=State.ItemInstances.Count-1;i>=0;i--)if(State.ItemInstances[i].InstanceId==instanceId&&State.ItemInstances[i].BaseItemId==itemId)return State.ItemInstances[i];
            for(int i=State.ItemInstances.Count-1;i>=0;i--)if(State.ItemInstances[i].BaseItemId==itemId)return State.ItemInstances[i];
            return null;
        }

        private IEnumerable<int> LivingMonstersAdjacentTo(int centerIndex)
        {
            var center=Position(centerIndex);for(int i=0;i<State.Tiles.Count;i++){if(i==centerIndex)continue;var pos=Position(i);if(center.IsOrthogonallyAdjacent(pos)&&TryLivingMonster(i,out _))yield return i;}
        }

        private void ResolveTile(TileState tile){tile.Resolution=TileResolution.Resolved;State.TilesResolved++;if(tile.Occupancy!=OccupancyKind.Player)tile.Occupancy=OccupancyKind.None;}
        private IEnumerable<int> AdjacentLivingMonsters(){for(int i=0;i<State.Tiles.Count;i++)if(IsAdjacent(i)&&TryLivingMonster(i,out _))yield return i;}
        private int FirstAdjacentMonster(){foreach(int i in AdjacentLivingMonsters())return i;return -1;}
        private bool TryLivingMonster(int index,out TileState tile){tile=null;if(!TryTile(index,out tile))return false;return(tile.Content==TileContentKind.Monster||tile.Content==TileContentKind.Boss)&&tile.Visibility==TileVisibility.Revealed&&tile.Resolution==TileResolution.Available&&tile.MonsterHp>0;}
        private bool TryTile(int index,out TileState tile){tile=null;if(index<0||index>=State.Tiles.Count)return false;tile=State.Tiles[index];return true;}
        private bool IsAdjacent(int index)=>State.PlayerPosition.IsOrthogonallyAdjacent(Position(index));
        private static GridPosition Position(int index)=>new GridPosition(index/RunState.BoardSize,index%RunState.BoardSize);
        private static int Index(GridPosition p)=>p.Row*RunState.BoardSize+p.Col;
        private static bool ConsumesMeaningfulAction(GameCommand command)=>command is RevealTileCommand||command is AttackCommand||command is DefendCommand||command is UseAbilityCommand||command is UseItemCommand||command is ChooseShrineCommand||command is UnlockVaultCommand;
    }
}
