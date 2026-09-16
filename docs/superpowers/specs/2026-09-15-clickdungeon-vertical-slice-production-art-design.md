# ClickDungeon2 5x5 Vertical-Slice Production Art Design

Date: 2026-09-15
Status: Written design awaiting user review
Scope: Polished 5x5 vertical slice only

## 1. Purpose

This design defines the gameplay-facing rules, visual constraints, production-art coverage, and generation order required to complete a polished ClickDungeon2 5x5 vertical slice.

The supplied ClickDungeon2 reference boards remain the visual bible. Existing presentation boards are references, not automatically shippable Unity sprites. The production workflow will create clean gameplay assets first, validate them in the 5x5 board context, and only then build matching presentation and encyclopedia sheets from the approved production assets.

This design intentionally does not expand into the full release game's later biomes, complete hero roster, complete monster encyclopedia, or long-form metagame systems.

## 2. Approved Vertical-Slice Content

### 2.1 Playable hero

- Sir Clickington

### 2.2 Encounter roster

The slice uses this exact roster:

- Goblin Raider
- Skeleton
- Fire Imp
- Cave Spider
- Crowned Slime
- Mimic Chest
- Goblin King

The roster provides baseline melee, defensive melee, ranged pressure, mobility, swarm/control, deception, and boss behavior without requiring the full monster catalog.

### 2.3 Run length

- Five floors total.
- Floors 1-4 use normal room generation and may contain pits if a lower floor exists.
- Floor 5 is the final Goblin King floor and may never contain pits.
- All five floors belong to one evolving stone-dungeon biome.

## 3. Core Board Model

The game board is a 5x5 grid. Tile contents are hidden by a cover tile until selected and revealed.

### 3.1 Concealment tiles

A wall-looking tile is not physical collision geometry. It is a concealment cap used to hide the tile's contents.

Rules:

- Nothing on the board blocks movement.
- Covered tiles may hide empty floor, enemies, chests, traps, pits, stairs, loot, or other supported encounter content.
- All covered tiles on the same floor must look identical before reveal.
- There must be no visual tell for chest rarity, Mimics, traps, enemies, pits, stairs, or loot before reveal.
- The cover style may evolve cosmetically from floor to floor, but all 25 covered tiles on a given floor share the same hidden-state appearance.

Required cover states:

- covered
- selected
- cracking/opening
- revealed

## 4. Movement Modes

Movement mode is chosen in Options before a run begins. It cannot be changed during an active run.

### 4.1 Free-click movement

This is the default mode and the normal out-of-box behavior.

- The player may click any legal tile on the 5x5 board.
- Destination distance does not matter.
- Intervening tiles do not block movement.
- Revealed monsters, chests, traps, walls/covers, and other board contents do not act as path blockers.
- All other reveal, combat, chest, trap, and enemy-response rules are unchanged.

### 4.2 Tactical movement

This is the selectable optional mode.

- Move reaches one adjacent tile.
- Adjacent includes up, down, left, right, and all four diagonals.
- Dash reaches up to two tiles.
- Nothing acts as path-blocking geometry; the movement rule only limits legal destination distance.

### 4.3 Implementation architecture constraint

The intended gameplay architecture is one shared game loop with pluggable movement validation rather than duplicated game modes.

Conceptually:

- `FreeClickMovement` validates unrestricted 5x5 destinations.
- `TacticalMovement` validates one-tile Move and two-tile Dash destinations.
- Combat, reveal, enemy turns, loot, traps, floor state, and HUD behavior remain shared.

## 5. Turn and Enemy-Response Rules

Every successful player action advances the enemy-response phase once.

Examples of player actions include:

- Move
- Dash
- Slash
- Shield
- Potion
- chest tap
- premium chest key use
- door or object interaction
- other successful gameplay interactions that consume an action

Resolution order:

1. Validate the chosen player action.
2. Execute the action.
3. Reveal and resolve any selected tile content or interaction result.
4. Add any newly revealed enemy to the active revealed-enemy set.
5. Resolve one response opportunity for every revealed enemy that is able to act.
6. Resolve damage, status, defeat, victory, or floor transition.
7. Refresh board and HUD state and return control to the player.

A monster revealed by the current player action is active during that same enemy-response phase unless a future enemy-specific rule explicitly says otherwise.

Position still matters for attack legality and telegraphing even though it does not block movement. For example, a Goblin Raider must satisfy its melee range while a Fire Imp may use a ranged attack.

## 6. Pit Trap Rules

A pit is a revealed trap that causes immediate damage and downward floor transition.

Rules:

- Clicking/revealing a pit triggers the pit.
- The hero immediately takes exactly 20 HP of fixed damage.
- The hero then drops exactly one floor lower.
- Remaining HP, resources, and status effects carry to the lower floor.
- The lower floor begins in its normal newly generated starting state; there is no special pit-arrival room.
- If no valid floor exists below, the generator must not place a pit on the current floor.
- Therefore Floor 5 contains no pits.
- Pit tiles never function as movement blockers.

Required pit presentation:

- hidden through the normal identical cover tile
- reveal effect
- open-pit asset
- 20 HP damage feedback
- fall animation/VFX
- one-floor descent transition

## 7. Chest, Key, Loot, and Mimic Rules

### 7.1 Regular chest quality

Regular chest opening requires repeated taps. Each tap is a complete player action and therefore triggers the full revealed-enemy response phase.

Tap counts are fixed by chest quality:

- Common Chest: 2 taps
- Rare Chest: 3 taps
- Epic Chest: 4 taps

Example Rare Chest sequence:

1. Tap 1 advances progress to 1/3, then revealed enemies respond.
2. Tap 2 advances progress to 2/3, then revealed enemies respond.
3. Tap 3 opens the chest and resolves loot, then revealed enemies respond.

Opening a chest while enemies are active is intentionally a risk/reward choice.

### 7.2 Premium/Special chest

- Requires one valid Special Key.
- Consumes the key.
- Opens immediately once the valid key is used.
- Does not use repeated tap buildup after key validation.
- The key use itself is a player action and therefore participates in the standard enemy-response flow.

### 7.3 Mimic

- Initially presents as a normal chest.
- Before first interaction it must not have a reliable visual tell.
- The first tap reveals the Mimic immediately instead of advancing regular chest progress.
- That first tap is the player's action.
- The newly revealed Mimic joins the same enemy-response phase and may act immediately if its attack rules allow it.

### 7.4 Loot presentation

The vertical slice needs gameplay-ready art for:

- coins
- gems
- potion
- equipment pickup
- Special Key
- reward burst
- pickup sparkle/confirmation

## 8. Stairs and Floor Completion

- Stairs begin hidden under the same cover system as other tile contents.
- Revealing stairs does not force an immediate exit.
- The player may continue exploring the current 5x5 room.
- Choosing to use the stairs advances exactly one floor.
- A pit is the involuntary one-floor descent alternative.
- Floor 5 resolves to the slice victory state after the Goblin King encounter and required completion conditions.

## 9. Vertical-Slice Enemy Roles

### 9.1 Goblin Raider

Baseline melee enemy.

Production needs:

- idle
- movement/reposition
- alert
- melee attack
- hit
- defeat

### 9.2 Skeleton

Durable guard-style melee enemy.

Production needs:

- idle
- movement/reposition
- alert
- melee attack
- guard/block
- guard-break feedback if used
- hit
- defeat

### 9.3 Fire Imp

Ranged pressure enemy.

Production needs:

- idle
- movement/reposition
- alert
- cast
- projectile
- projectile impact
- hit
- defeat

### 9.4 Cave Spider

Fast mobility/pounce enemy.

Production needs:

- idle
- scuttle/reposition
- alert
- pounce telegraph
- pounce attack
- hit
- defeat

### 9.5 Crowned Slime

Simple swarm/control pressure enemy.

Production needs:

- idle
- bounce/reposition
- alert
- attack
- hit/squash
- defeat/splat

### 9.6 Mimic Chest

Deception encounter.

Production needs:

- chest disguise
- first-tap reveal transition
- alert
- attack
- hit
- defeat

### 9.7 Goblin King

Final-floor boss with a larger health pool and at least two visually distinct attack patterns.

Production needs:

- idle
- movement/reposition
- encounter introduction/alert
- attack pattern 1
- attack pattern 2
- attack telegraphs
- hit
- optional stagger if implemented
- defeat
- boss portrait
- boss health frame and fill
- boss warning treatment
- victory presentation

## 10. Sir Clickington Production Set

Gameplay character art uses a fixed 3/4 top-down camera.

Direction coverage:

- front
- back
- left
- right

Diagonal movement reuses the nearest directional animation. The slice does not require an eight-direction art set.

Required hero states:

- idle
- move
- dash
- slash
- shield
- potion/heal
- hurt
- defeat
- pit fall

Required separate VFX:

- slash arc
- shield/block impact
- dash trail
- healing/potion effect
- hero hit feedback

## 11. Art Direction and Technical Standards

### 11.1 Camera and readability

- Fixed 3/4 top-down gameplay view matching the 5x5 board.
- Chibi/stylized proportions remain consistent with existing ClickDungeon2 references.
- Silhouettes must remain readable at actual board scale, not only at encyclopedia-sheet scale.

### 11.2 Sprite consistency

- Consistent feet position and pivot across character animation frames.
- Consistent scale for all standard enemies relative to Sir Clickington.
- Boss scale may be larger but must remain compatible with the 5x5 presentation.
- Transparent backgrounds for gameplay sprites, VFX, icons, and UI elements where applicable.

### 11.3 Lighting

- Character sprites should not bake in floor-specific environmental lighting strongly enough to prevent reuse.
- Torch/light overlays and floor mood should primarily come from environment presentation.

### 11.4 Effects separation

Where practical, telegraphs, slash trails, projectile effects, hit flashes, healing effects, and similar transient effects are separate assets rather than permanently baked into character sprites.

### 11.5 Master and export sizes

- Create large master art at approximately 1024x1024 where appropriate.
- Typical Unity gameplay derivatives should target 256x256 or 512x512 depending on on-screen size and animation needs.
- Large boss or full-screen VFX assets may use 512x512 or 1024x1024 exports.
- Final import settings are determined during implementation based on actual Unity board-scale validation.

### 11.6 Naming convention

Use lowercase snake_case with a system or character prefix.

Examples:

- `hero_clickington_attack_front_01.png`
- `enemy_goblin_raider_idle_left_01.png`
- `chest_rare_progress_02.png`
- `tile_cover_floor_03.png`
- `vfx_fire_imp_projectile_impact_01.png`

## 12. Five-Floor Visual Progression

All floors stay within one stone-dungeon biome.

### Floor 1

- cleanest stonework
- warm torches
- simple banners
- sparse debris

### Floor 2

- more cracks
- chains
- broken masonry
- bones
- slightly darker lighting

### Floor 3

- heavier wear
- more rubble
- cobwebs
- darker torch pools
- stronger hazard dressing

### Floor 4

- oppressive lighting
- damaged walls
- more skulls and chains
- stronger red/purple accents
- pre-boss tension

### Floor 5

- Goblin King boss presentation
- throne/arena motifs
- larger banners
- stronger gold/red accents
- dramatic lighting
- no pits

The biome changes in intensity, not identity. This slice does not introduce multiple independent biomes.

## 13. Environment Production Set

Required environment assets include:

- base revealed floor tiles and variants
- floor-specific cover tiles for Floors 1-5
- selected-cover state
- crack/reveal states
- reveal burst
- Free-click target highlight
- Tactical legal-move highlight
- Tactical invalid-move feedback
- Dash destination highlight
- stairs idle/selected/activated
- pit open/reveal/fall presentation
- rubble
- bones
- chains
- banners
- cobwebs
- torch/light overlays
- Floor 5 boss-room dressing

## 14. HUD and Options Production Set

### 14.1 Pre-run Options

The movement option must clearly communicate:

Free-click, default:

- click any board tile
- distance and intervening contents do not block movement

Tactical, optional:

- Move one tile in any of eight directions
- Dash up to two tiles

The option is locked for the duration of the active run.

### 14.2 Gameplay HUD

Required gameplay-facing UI art includes:

- hero HP
- potion
- coins
- gems
- Special Keys
- floor indicator from Floor 1/5 through Floor 5/5
- enemy HP bars
- Goblin King boss HP treatment
- enemy-response/turn feedback
- invalid-action feedback
- chest progress meter
- movement target highlights
- damage numbers
- 20 HP pit-damage presentation
- attack telegraphs

## 15. Combat and Readability VFX

The slice requires:

- melee slash arc
- shield/block impact
- dash trail
- heal/potion effect
- player and enemy hit flash
- damage numbers
- enemy alert marker
- attack-range/telegraph overlays
- Fire Imp projectile and impact
- Cave Spider pounce cue
- Skeleton guard effect
- Goblin King boss telegraphs and attack VFX
- defeat effects
- tile reveal burst

Any critical-hit or strong-hit treatment is only required if the current gameplay implementation uses that mechanic.

## 16. Production Generation Batches

Large-scale art generation should proceed in dependency order rather than as one blind batch.

### Batch 1: Board and reveal system

Generate and validate:

- base floor
- cover tiles
- cover selection/reveal
- movement highlights
- stairs
- pit presentation
- environment dressing

This batch establishes scale, camera, color, tile readability, and hidden-information integrity.

### Batch 2: Sir Clickington

Generate four-direction gameplay states and separate core VFX.

### Batch 3: Baseline enemy proof

Generate Goblin Raider first and validate it in the real 5x5 composition with Sir Clickington before expanding the roster.

### Batch 4: Chest and reveal interactions

Generate:

- Common 2-tap chest
- Rare 3-tap chest
- Epic 4-tap chest
- Premium chest
- Special Key
- loot and reward effects

### Batch 5: 5x5 composition checkpoint

Before generating the full remaining roster, confirm:

- character scale
- direction readability
- cover-tile secrecy
- move and Dash highlight readability
- chest-progress readability
- enemy telegraphs
- visual hierarchy under actual gameplay conditions

Any style, scale, or perspective problem discovered here is corrected before downstream asset expansion.

### Batch 6: Remaining common enemies

Generate Skeleton, Fire Imp, Cave Spider, and Crowned Slime.

### Batch 7: Mimic

Generate the disguise-to-reveal interaction and verify that its unrevealed state does not provide a reliable tell.

### Batch 8: Goblin King

Generate boss gameplay art, boss telegraphs, boss HUD, and final-floor dressing.

### Batch 9: HUD, VFX, and flow screens

Generate remaining gameplay UI, action feedback, floor descent, pit fall, defeat, and Floor 5 victory presentation.

### Batch 10: Presentation sheets

Only after production gameplay art is accepted, build matching documentation boards:

1. 5x5 Dungeon Environment Production Sheet
2. Sir Clickington Gameplay Animation Sheet
3. Core Enemy Production Sheet
4. Chest / Loot / Special Key System Sheet
5. Mimic Encounter Sheet
6. Goblin King Boss Sheet
7. Combat VFX and Telegraph Sheet
8. HUD + Movement Modes Sheet
9. Five-Floor Visual Progression Sheet
10. Complete Vertical Slice Art Bible

Presentation sheets must document the approved gameplay assets rather than introducing conflicting replacement designs.

## 17. Flow Screens in Scope

The slice requires only the flow art necessary to prove the run:

- run-start transition
- normal floor-descent transition
- pit-fall transition
- defeat presentation
- Floor 5 victory presentation
- return-to-menu result treatment

Full-release progression screens outside the vertical slice are not part of this production-art pass.

## 18. Validation Requirements

The production-art pass is not complete merely because individual assets look polished.

Validation must confirm:

- all concealed tiles on a floor are visually indistinguishable before reveal
- no Mimic tell exists before first interaction
- Sir Clickington and every enemy remain readable at gameplay scale
- all four directional character states maintain consistent scale and pivots
- Free-click and Tactical destination states are visually distinct and understandable
- Tactical mode communicates 1-tile Move and up-to-2-tile Dash reach correctly
- movement art never implies physical collision where the rules have none
- every regular chest quality has a clearly readable 2/3/4-tap progression
- each chest tap can be visually understood as a discrete player action
- premium chest art clearly communicates Special Key gating and immediate keyed opening
- pit art clearly communicates 20 HP damage plus one-floor descent
- Floor 5 contains no pit presentation in generation or authored layouts
- newly revealed enemies can visually enter the same response phase cleanly
- attack telegraphs distinguish melee, ranged, pounce, guard, and boss behaviors
- the evolving floor treatment remains one coherent biome
- final presentation sheets match the approved production assets

## 19. Explicit Non-Goals

This design does not require:

- the complete hero roster
- every monster from the encyclopedia/reference collection
- multiple biomes
- eight-direction sprites
- full release-game progression
- full late-game equipment content
- unrelated menu redesigns
- expansion of the five-floor slice into a complete campaign

## 20. Definition of Done for the Art Design

The vertical-slice production-art program is design-complete when:

- the above gameplay rules are represented without contradiction
- every required gameplay-facing asset category has an explicit place in the batch plan
- the 5x5 composition checkpoint is passed before broad asset expansion
- gameplay production art is approved before presentation sheets are generated
- presentation sheets accurately document final production assets
- no unapproved full-game scope has been pulled into the slice

Implementation planning begins only after the user reviews and approves this written design.
