using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Rules;
using ClickDungeon.Presentation.Assets;

namespace ClickDungeon.Presentation.UI
{
    /// <summary>
    /// Runtime-first UI used by the vertical slice. It deliberately submits commands to the
    /// simulation rather than calculating gameplay. The same board adapts to portrait mobile and
    /// landscape desktop without stretching a phone layout across a PC window.
    /// </summary>
    public sealed class RuntimeGameUI : MonoBehaviour
    {
        private GameSession _session;
        private GameContent _content;
        private readonly List<Button> _tileButtons=new List<Button>();
        private readonly List<TMP_Text> _tileLabels=new List<TMP_Text>();
        private readonly List<Image> _tileIcons=new List<Image>();
        private TMP_Text _hud;
        private Image _heroPortrait;
        private TMP_Text _status;
        private TMP_Text _intent;
        private RectTransform _root;
        private RectTransform _infoPanel;
        private RectTransform _board;
        private RectTransform _controlPanel;
        private RectTransform _abilityBar;
        private RectTransform _choicePanel;
        private Image _biomeBackdrop;
        private PresentationAssetDatabase _assets;
        private GridLayoutGroup _boardGrid;
        private string _pendingAbilityId;
        private string _pendingItemId;
        private bool _lastLandscape;
        private Rect _lastSafeArea;

        public event Action StateChanged;
        public event Action<CommandResult> CommandResolved;
        public event Action<GameCommand,CommandResult> CommandExecuted;
        public event Action ReturnToMenuRequested;

        public void Initialize(GameSession session,GameContent content)
        {
            _session=session??throw new ArgumentNullException(nameof(session));_content=content??throw new ArgumentNullException(nameof(content));
            _assets=Resources.Load<PresentationAssetDatabase>("ClickDungeonPresentationAssets");EnsureEventSystem();BuildUi();ApplyAdaptiveLayout(true);Refresh();
        }

        private void Update()
        {
            bool landscape=Screen.width>Screen.height;
            if(landscape!=_lastLandscape||Screen.safeArea!=_lastSafeArea)ApplyAdaptiveLayout(false);
            UpdateBoardCellSize();
        }

        private void BuildUi()
        {
            var canvasGo=new GameObject("ClickDungeonCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasGo.transform.SetParent(transform,false);
            var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
            _root=CreateRect("SafeRoot",canvasGo.transform);Stretch(_root);
            var backdrop=CreateRect("BiomeBackdrop",_root);Stretch(backdrop);_biomeBackdrop=backdrop.gameObject.AddComponent<Image>();_biomeBackdrop.raycastTarget=false;_biomeBackdrop.color=new Color(1f,1f,1f,.14f);_biomeBackdrop.preserveAspect=true;

            _infoPanel=CreateRect("InfoPanel",_root);var infoLayout=_infoPanel.gameObject.AddComponent<VerticalLayoutGroup>();infoLayout.padding=new RectOffset(12,12,12,12);infoLayout.spacing=8;infoLayout.childControlHeight=true;infoLayout.childForceExpandHeight=false;
            var portraitRt=CreateRect("HeroPortrait",_infoPanel);_heroPortrait=portraitRt.gameObject.AddComponent<Image>();_heroPortrait.preserveAspect=true;_heroPortrait.raycastTarget=false;AddLayout(portraitRt.gameObject,86);
            _hud=CreateText("HUD",_infoPanel,"ClickDungeon",34,TextAlignmentOptions.Center);AddLayout(_hud.gameObject,92);
            _status=CreateText("Status",_infoPanel,"Read the dungeon.",22,TextAlignmentOptions.Center);AddLayout(_status.gameObject,76);
            _intent=CreateText("Intent",_infoPanel,"No immediate threat.",18,TextAlignmentOptions.Center);AddLayout(_intent.gameObject,120);

            _board=CreateRect("Board",_root);_boardGrid=_board.gameObject.AddComponent<GridLayoutGroup>();_boardGrid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;_boardGrid.constraintCount=5;_boardGrid.spacing=new Vector2(8,8);_boardGrid.childAlignment=TextAnchor.MiddleCenter;
            for(int i=0;i<25;i++){int captured=i;var button=CreateButton($"Tile_{i}",_board,"?",28);var iconRt=CreateRect("Icon",button.transform);Stretch(iconRt);var icon=iconRt.gameObject.AddComponent<Image>();icon.preserveAspect=true;icon.raycastTarget=false;icon.color=new Color(1f,1f,1f,.65f);iconRt.SetSiblingIndex(0);button.onClick.AddListener(()=>OnTilePressed(captured));_tileButtons.Add(button);_tileLabels.Add(button.GetComponentInChildren<TMP_Text>());_tileIcons.Add(icon);}

            _controlPanel=CreateRect("ControlPanel",_root);var controlLayout=_controlPanel.gameObject.AddComponent<VerticalLayoutGroup>();controlLayout.padding=new RectOffset(12,12,12,12);controlLayout.spacing=10;controlLayout.childControlHeight=true;controlLayout.childForceExpandHeight=false;
            _abilityBar=CreateRect("AbilityBar",_controlPanel);var abilities=_abilityBar.gameObject.AddComponent<HorizontalLayoutGroup>();abilities.spacing=6;abilities.childControlWidth=true;abilities.childForceExpandWidth=true;AddLayout(_abilityBar.gameObject,110);
            _choicePanel=CreateRect("ChoicePanel",_controlPanel);var choices=_choicePanel.gameObject.AddComponent<HorizontalLayoutGroup>();choices.spacing=6;choices.childControlWidth=true;choices.childForceExpandWidth=true;AddLayout(_choicePanel.gameObject,100);_choicePanel.gameObject.SetActive(false);
            var footer=CreateText("Footer",_controlPanel,"Read clues. Control threats. Big Keys open vaults or Forbidden Descents.",18,TextAlignmentOptions.Center);AddLayout(footer.gameObject,70);
            var menu=CreateButton("Menu",_controlPanel,"Return to Menu",18);AddLayout(menu.gameObject,62);menu.onClick.AddListener(()=>ReturnToMenuRequested?.Invoke());
        }

        private void ApplyAdaptiveLayout(bool force)
        {
            if(_root==null)return;bool landscape=Screen.width>Screen.height;Rect safe=Screen.safeArea;if(!force&&landscape==_lastLandscape&&safe==_lastSafeArea)return;_lastLandscape=landscape;_lastSafeArea=safe;
            var canvas=_root.GetComponentInParent<Canvas>();RectTransform canvasRect=canvas.GetComponent<RectTransform>();Vector2 min=safe.position;Vector2 max=safe.position+safe.size;min.x/=Screen.width;min.y/=Screen.height;max.x/=Screen.width;max.y/=Screen.height;_root.anchorMin=min;_root.anchorMax=max;_root.offsetMin=Vector2.zero;_root.offsetMax=Vector2.zero;
            if(landscape)
            {
                SetAnchors(_infoPanel,new Vector2(0f,0f),new Vector2(.22f,1f),16);
                SetAnchors(_board,new Vector2(.22f,.04f),new Vector2(.76f,.96f),10);
                SetAnchors(_controlPanel,new Vector2(.76f,0f),new Vector2(1f,1f),16);
            }
            else
            {
                SetAnchors(_infoPanel,new Vector2(0f,.86f),new Vector2(1f,1f),14);
                SetAnchors(_board,new Vector2(.04f,.30f),new Vector2(.96f,.86f),8);
                SetAnchors(_controlPanel,new Vector2(0f,0f),new Vector2(1f,.30f),14);
            }
            UpdateBoardCellSize();
        }

        private void UpdateBoardCellSize()
        {
            if(_boardGrid==null||_board==null)return;float usable=Mathf.Max(100,Mathf.Min(_board.rect.width,_board.rect.height)-32);float cell=(usable-_boardGrid.spacing.x*4)/5f;if(cell>1)_boardGrid.cellSize=new Vector2(cell,cell);
        }

        private void Refresh()
        {
            if(_session==null)return;var s=_session.State;
            string depthLabel=s.Mode==RunMode.Abyss?$"Abyss Depth {s.AbyssDepth}":$"Floor {s.Floor}";string heroName=_content.Hero(s.HeroClass).DisplayName;if(string.IsNullOrEmpty(heroName))heroName=s.HeroClass.ToString();string biomeName=_content.Biome(s.BiomeId).DisplayName;if(string.IsNullOrEmpty(biomeName))biomeName=ShortId(s.BiomeId);
            if(_heroPortrait!=null)_heroPortrait.sprite=_assets?.SpriteFor("hero."+s.HeroClass.ToString().ToLowerInvariant());
            _hud.text=$"{heroName}  HP {s.Hp}/{s.MaxHp}  ATK {s.Attack}  DEF {s.Defense}\n{depthLabel}  {biomeName}  Gold {s.Gold}  Keys {s.SmallKeys}/{s.BigKeys}";
            if(_biomeBackdrop!=null)_biomeBackdrop.sprite=_assets?.SpriteFor(s.BiomeId);
            for(int i=0;i<25;i++)RefreshTile(i);RefreshIntent();RebuildAbilities();
            if(s.GameOver)_status.text="The dungeon claimed this run.";else if(s.CampaignCompleted)_status.text="Campaign complete. The Abyss is now available from the main menu.";
        }

        private void RefreshTile(int index)
        {
            var tile=_session.State.Tiles[index];var label=_tileLabels[index];var button=_tileButtons[index];bool threatened=ThreatResolver.IsThreatened(_session.State,index);string text;
            if(tile.Visibility==TileVisibility.Hidden)text="?";else if(tile.Visibility==TileVisibility.Clued)text=ClueText(tile.Clue);else if(tile.Visibility==TileVisibility.Identified)text=IdentifiedText(tile);else text=RevealedText(tile);
            if(threatened&&tile.Occupancy!=OccupancyKind.Monster)text="⚠\n"+text;if(tile.Terrain!=TerrainKind.Normal)text=TerrainMark(tile.Terrain)+"\n"+text;if(index==Index(_session.State.PlayerPosition))text="◆\n"+text;label.text=text;var icon=_tileIcons[index];string assetId=AssetIdFor(tile);icon.sprite=_assets?.SpriteFor(assetId);icon.enabled=icon.sprite!=null;button.GetComponent<Image>().color=TileColor(tile,threatened,index==Index(_session.State.PlayerPosition));button.interactable=!_session.State.GameOver&&!_session.State.CampaignCompleted;
        }



        private void RefreshIntent()
        {
            var nearby=new List<string>();
            for(int i=0;i<_session.State.Tiles.Count;i++)
            {
                var t=_session.State.Tiles[i];if((t.Content!=TileContentKind.Monster&&t.Content!=TileContentKind.Boss)||t.Visibility!=TileVisibility.Revealed||t.Resolution!=TileResolution.Available||t.MonsterHp<=0)continue;
                if(!_session.State.PlayerPosition.IsOrthogonallyAdjacent(new GridPosition(i/RunState.BoardSize,i%RunState.BoardSize)))continue;
                string monsterName=_content.Monster(t.ContentId).DisplayName;nearby.Add($"{(string.IsNullOrEmpty(monsterName)?ShortId(t.ContentId):monsterName)}: {t.IntentKind} {t.IntentPower}");
            }
            string statuses=_session.State.Statuses.Count==0?string.Empty:"\nStatus: "+string.Join(", ",_session.State.Statuses.Select(x=>ShortId(x.StatusId)+" "+x.RemainingActions));
            _intent.text=(nearby.Count==0?"No adjacent enemy intent.":string.Join("\n",nearby))+statuses;
        }

        private static Color TileColor(TileState tile,bool threatened,bool player)
        {
            if(player)return new Color(.18f,.36f,.28f,.98f);if(threatened)return new Color(.38f,.14f,.16f,.98f);
            if(tile.Visibility==TileVisibility.Clued)return tile.Clue==ClueFamily.Danger?new Color(.31f,.18f,.18f,.98f):tile.Clue==ClueFamily.Opportunity?new Color(.30f,.27f,.15f,.98f):new Color(.18f,.22f,.34f,.98f);
            if(tile.Visibility==TileVisibility.Identified)return new Color(.20f,.27f,.32f,.98f);if(tile.Visibility==TileVisibility.Hidden)return new Color(.09f,.10f,.13f,.98f);return new Color(.15f,.16f,.20f,.95f);
        }
        private static string AssetIdFor(TileState tile)
        {
            if(tile.Visibility==TileVisibility.Hidden)return string.Empty;
            if(tile.Visibility==TileVisibility.Clued)return tile.Clue==ClueFamily.Danger?"clue.danger":tile.Clue==ClueFamily.Opportunity?"clue.opportunity":tile.Clue==ClueFamily.PassageArcane?"clue.passage":string.Empty;
            if(tile.Content==TileContentKind.BigKey)return "key.big";if(tile.Content==TileContentKind.SmallKey)return "key.small";if(tile.Content==TileContentKind.Gold)return "currency.gold";
            return tile.ContentId;
        }
        private void OnTilePressed(int index)
        {
            if(!string.IsNullOrEmpty(_pendingItemId)){string id=_pendingItemId;_pendingItemId=null;Apply(new UseItemCommand(id,index));return;}
            if(!string.IsNullOrEmpty(_pendingAbilityId)){string id=_pendingAbilityId;_pendingAbilityId=null;Apply(new UseAbilityCommand(id,index));return;}
            var tile=_session.State.Tiles[index];GameCommand command;
            if(tile.Visibility!=TileVisibility.Revealed)command=new RevealTileCommand(index);else if(tile.Content==TileContentKind.Monster||tile.Content==TileContentKind.Boss)command=new AttackCommand(index);else if(tile.Content==TileContentKind.SafeExit)command=new TakeSafeExitCommand(index);else if(tile.Content==TileContentKind.ForbiddenExit)command=new TakeForbiddenExitCommand(index);else if(tile.Content==TileContentKind.SealedVault)command=new UnlockVaultCommand(index);else if(tile.Content==TileContentKind.Shrine){ShowShrineChoices(index);return;}else if(tile.Content==TileContentKind.Merchant){ShowMerchant(index);return;}else if(tile.Resolution==TileResolution.Available&&tile.Content!=TileContentKind.Empty)command=new InteractCommand(index);else command=new MoveCommand(index);Apply(command);
        }

        private void RebuildAbilities()
        {
            foreach(Transform child in _abilityBar)Destroy(child.gameObject);
            foreach(var state in _session.State.AbilityStates){var def=_content.Ability(state.AbilityId);string shortName=string.IsNullOrEmpty(def.DisplayName)?ShortAbility(state.AbilityId):def.DisplayName;var button=CreateButton(shortName,_abilityBar,$"{shortName}\n{state.Charges}/{def.MaxCharges}",16);string id=state.AbilityId;button.interactable=state.Charges>0;button.onClick.AddListener(()=>BeginAbility(id));}
            var defend=CreateButton("Defend",_abilityBar,"Defend",16);defend.onClick.AddListener(()=>Apply(new DefendCommand()));var potion=CreateButton("Potion",_abilityBar,"Potion",16);potion.interactable=_session.State.InventoryItemIds.Contains("item.healing_potion");potion.onClick.AddListener(()=>Apply(new UseItemCommand("item.healing_potion")));var trapKit=CreateButton("Trap Kit",_abilityBar,"Trap Kit",16);trapKit.interactable=_session.State.InventoryItemIds.Contains("item.trap_disarm_kit");trapKit.onClick.AddListener(()=>BeginItem("item.trap_disarm_kit"));var gear=CreateButton("Gear",_abilityBar,"Gear",16);gear.onClick.AddListener(ShowInventory);
        }

        private void BeginItem(string id)
        {
            _pendingItemId=id;
            _pendingAbilityId=null;
            _status.text=id=="item.trap_disarm_kit"?"Select an adjacent identified or revealed trap to disarm.":"Select a target.";
        }

        private void BeginAbility(string id)
        {
            if(id.EndsWith("shield_wall")||id.EndsWith("fortify")||id.EndsWith("guardians_oath")||id.EndsWith("camouflage")||id.EndsWith("trap_scan")||id.EndsWith("veil_of_smoke")||id.EndsWith("frost_nova")||id.EndsWith("chain_lightning")||id.EndsWith("arcane_shield")||id.EndsWith("meteor")){Apply(new UseAbilityCommand(id));return;}_pendingAbilityId=id;_status.text=$"Select a target for {ShortAbility(id)}.";
        }


        private void ShowInventory()
        {
            ClearChoicePanel();_choicePanel.gameObject.SetActive(true);
            string weapon=string.IsNullOrEmpty(_session.State.EquippedWeaponId)?"None":ShortId(_session.State.EquippedWeaponId)+(string.IsNullOrEmpty(_session.State.EquippedWeaponAffixId)?"":" + "+ShortId(_session.State.EquippedWeaponAffixId));
            string armor=string.IsNullOrEmpty(_session.State.EquippedArmorId)?"None":ShortId(_session.State.EquippedArmorId)+(string.IsNullOrEmpty(_session.State.EquippedArmorAffixId)?"":" + "+ShortId(_session.State.EquippedArmorAffixId));
            _status.text=$"Weapon: {weapon} | Armor: {armor}";
            foreach(var instance in _session.State.ItemInstances)
            {
                var def=_content.Item(instance.BaseItemId);if(def.Kind!="weapon"&&def.Kind!="armor")continue;
                string baseName=string.IsNullOrEmpty(def.DisplayName)?ShortId(instance.BaseItemId):def.DisplayName;string affixName=string.Empty;if(!string.IsNullOrEmpty(instance.AffixId)&&_content.TryAffix(instance.AffixId,out var affix))affixName=string.IsNullOrEmpty(affix.DisplayName)?ShortId(instance.AffixId):affix.DisplayName;string label=baseName+(string.IsNullOrEmpty(affixName)?"":" + "+affixName);string itemId=instance.BaseItemId;string instanceId=instance.InstanceId;AddChoice(label,()=>ApplyAndClose(new EquipItemCommand(itemId,instanceId)));
            }
            if(_session.State.ItemInstances.Count==0)AddChoice("No equipment yet",()=>_choicePanel.gameObject.SetActive(false));
        }
        private void ShowShrineChoices(int tileIndex){ClearChoicePanel();_choicePanel.gameObject.SetActive(true);AddChoice("Blood +HP",()=>ApplyAndClose(new ChooseShrineCommand(tileIndex,ShrineChoice.MaxHp)));AddChoice("Steel +ATK",()=>ApplyAndClose(new ChooseShrineCommand(tileIndex,ShrineChoice.Attack)));AddChoice("Stone +DEF",()=>ApplyAndClose(new ChooseShrineCommand(tileIndex,ShrineChoice.Defense)));}
        private void ShowMerchant(int tileIndex)
        {
            ClearChoicePanel();_choicePanel.gameObject.SetActive(true);
            var tile=_session.State.Tiles[tileIndex];string shopId=tile.ContentId=="merchant.standard"?"shop.standard":tile.ContentId.Replace("merchant.","shop.");var shop=_content.Shop(shopId);
            foreach(var stockId in shop.StockItemIds){var def=_content.Item(stockId);string id=stockId;string display=string.IsNullOrEmpty(def.DisplayName)?ShortId(stockId):def.DisplayName;AddChoice($"{display} {def.Price}g",()=>ApplyAndClose(new BuyItemCommand(tileIndex,id)));}
        }
        private void AddChoice(string text,UnityEngine.Events.UnityAction action){var b=CreateButton(text,_choicePanel,text,16);b.onClick.AddListener(action);}private void ApplyAndClose(GameCommand command){Apply(command);_choicePanel.gameObject.SetActive(false);}private void ClearChoicePanel(){foreach(Transform child in _choicePanel)Destroy(child.gameObject);}

        private void Apply(GameCommand command)
        {
            var result=_session.Apply(command);CommandExecuted?.Invoke(command,result);if(!result.Accepted){_status.text=RejectionMessage(result.RejectionReason);Refresh();return;}_status.text=result.Events.Count==0?"Action resolved.":Describe(result.Events[result.Events.Count-1]);CommandResolved?.Invoke(result);StateChanged?.Invoke();Refresh();
        }

        private static string RejectionMessage(string reason)
        {
            switch(reason)
            {
                case "entitlement.full_game_required":return "The free introduction ends at Floor 5. Unlock the full game from the main menu to descend farther.";
                case "tile.not_adjacent":case "monster.not_adjacent":return "Move next to that target first.";
                case "tile.threatened":return "A monster's threat zone blocks that tile. Defeat, control, or bypass the threat first.";
                case "tile.not_revealed":return "Reveal that tile before moving onto it.";
                case "tile.blocked_by_monster":return "A living monster blocks that tile.";
                case "player.rooted":return "You are rooted and cannot move yet.";
                case "key.small.required":return "A Small Key is required.";
                case "key.big.required":return "A Big Key is required.";
                case "key.big.carry_limit":return "You cannot carry another Big Key. Spend one on a vault or Forbidden Descent first.";
                case "boss.must_be_defeated":return "The boss still controls the exit.";
                case "combat.no_adjacent_enemy":return "There is no adjacent enemy to defend against.";
                case "gold.insufficient":return "You do not have enough Gold.";
                case "item.not_owned":return "That item is not in this run's inventory.";
                case "item.target_required":return "Select a valid target for that item.";
                case "trap.not_disarmable":return "The Trap Disarm Kit only works on an adjacent identified or revealed unresolved trap.";
                case "monster.not_attackable":return "That monster cannot be attacked from the current state.";
                case "exit.not_available":return "That exit is not available yet.";
                case "vault.not_available":return "That vault cannot be opened from here.";
                case "run.game_over":return "This run has ended.";
                case "run.completed":return "The campaign is complete.";
                default:return "That action is not currently available.";
            }
        }

        private static string Describe(GameEvent evt)
        {
            switch(evt.Type){case "entitlement.full_game_required":return "The free introduction ends at Floor 5. Unlock the full game to descend farther.";case "tile.revealed":return $"Revealed {ShortId(evt.Id)}.";case "monster.encountered":return $"{ShortId(evt.Id)} blocks the path.";case "boss.encountered":return $"Boss: {ShortId(evt.Id)}.";case "monster.defeated":return $"Defeated {ShortId(evt.Id)}.";case "boss.defeated":return "Boss defeated. The descent is open.";case "trap.triggered":return $"Trap! Lost {evt.Amount} HP.";case "trap.disarmed":return "Trap safely disarmed.";case "gold.collected":return $"Collected {evt.Amount} gold.";case "key.big.collected":return "Big Key acquired: vault or Forbidden Descent?";case "vault.opened":return "Sealed Vault opened.";case "floor.entered.forbidden":return "Forbidden route entered. Danger and rewards increased.";case "campaign.completed":return "You survived the campaign.";default:return evt.Type.Replace('.',' ');}
        }

        private static string ClueText(ClueFamily clue){switch(clue){case ClueFamily.Danger:return "!\nDanger";case ClueFamily.Opportunity:return "✦\nOpportunity";case ClueFamily.PassageArcane:return "◇\nPassage";default:return "?";}}
        private static string IdentifiedText(TileState tile)=>tile.Content==TileContentKind.Trap?"TRAP\nidentified":$"ID\n{ShortId(tile.ContentId)}";
        private static string RevealedText(TileState tile){if(tile.Resolution==TileResolution.Resolved&&tile.Content!=TileContentKind.Empty)return "✓\n"+ShortId(tile.ContentId);switch(tile.Content){case TileContentKind.Empty:return "·";case TileContentKind.Gold:return "$";case TileContentKind.Monster:return $"{MonsterLetter(tile.ContentId)}\n{tile.MonsterHp}/{tile.MonsterMaxHp}";case TileContentKind.Boss:return $"BOSS\n{tile.MonsterHp}/{tile.MonsterMaxHp}";case TileContentKind.Trap:return "TRAP";case TileContentKind.Chest:return "CHEST";case TileContentKind.Shrine:return "SHRINE";case TileContentKind.SmallKey:return "small\nKEY";case TileContentKind.BigKey:return "BIG\nKEY";case TileContentKind.SafeExit:return "SAFE\nEXIT";case TileContentKind.ForbiddenExit:return "FORBIDDEN\nEXIT";case TileContentKind.SealedVault:return "VAULT";case TileContentKind.Merchant:return "SHOP";default:return ShortId(tile.ContentId);}}
        private static string TerrainMark(TerrainKind terrain){switch(terrain){case TerrainKind.Grave:return "☠";case TerrainKind.Flooded:return "≈";case TerrainKind.Thorn:return "♯";case TerrainKind.Mire:return "~";case TerrainKind.Ice:return "◇";case TerrainKind.Charged:return "ϟ";case TerrainKind.Lava:return "▲";case TerrainKind.Arcane:return "✧";case TerrainKind.Ash:return "░";default:return string.Empty;}}
        private static string MonsterLetter(string id){string s=ShortId(id);return string.IsNullOrEmpty(s)?"M":s.Substring(0,1).ToUpperInvariant();}private static string ShortAbility(string id){var value=ShortId(id).Replace('_',' ');return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value);}private static string ShortId(string id){if(string.IsNullOrEmpty(id))return string.Empty;int i=id.LastIndexOf('.');return i>=0?id.Substring(i+1).Replace('_',' '):id;}private static int Index(GridPosition p)=>p.Row*RunState.BoardSize+p.Col;

        private static RectTransform CreateRect(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();}private static TMP_Text CreateText(string name,Transform parent,string text,float size,TextAlignmentOptions alignment){var rt=CreateRect(name,parent);var label=rt.gameObject.AddComponent<TextMeshProUGUI>();label.text=text;label.fontSize=size;label.alignment=alignment;label.enableWordWrapping=true;label.color=Color.white;return label;}private static Button CreateButton(string name,Transform parent,string text,float size){var rt=CreateRect(name,parent);var image=rt.gameObject.AddComponent<Image>();image.color=new Color(.15f,.16f,.2f,.95f);var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;var label=CreateText("Label",rt,text,size,TextAlignmentOptions.Center);Stretch(label.rectTransform);return button;}private static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;}private static void SetAnchors(RectTransform rt,Vector2 min,Vector2 max,float margin){rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=new Vector2(margin,margin);rt.offsetMax=new Vector2(-margin,-margin);}private static void AddLayout(GameObject go,float preferredHeight){var e=go.AddComponent<LayoutElement>();e.preferredHeight=preferredHeight;e.flexibleWidth=1;}private static void EnsureEventSystem(){if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));}
    }
}
