# ClickDungeon2 5x5 Vertical-Slice Production Art Design

Date: 2026-09-15
Status: Written design awaiting user review
Scope: Complete polished 5x5 playable vertical slice only
Branch: `superpowers/vertical-slice-production-art-spec`

## 1. Purpose

This design defines how ClickDungeon2 will complete the missing production art required for one polished, end-to-end 5x5 vertical slice while preserving verified existing assets and Unity metadata wherever possible.

The approved playable path is:

`title/start presentation -> 5x5 dungeon run -> encounter/boss resolution -> exit/resolution -> victory/reward/end-of-run presentation`

The supplied reference images are the visual bible. They define character identity, material language, palette, painterly/chibi treatment, UI tone, dungeon mood, and presentation hierarchy. They are not automatically shippable runtime sprites. The runtime set is built first as isolated transparent assets, validated in Unity, and only then used to produce polished reference sheets.

This design intentionally does not expand the vertical slice into the full release game, later heroes, later enemies, extra biomes, or unrelated progression systems.

## 2. Approved Decisions

The following decisions are fixed for this design:

- Deliverable order: **A then B**.
  - **Phase A:** implementation-ready runtime art.
  - **Phase B:** polished composite/reference sheets derived from the validated Phase A assets.
- Animation strategy: **hybrid**.
  - Key-pose sprites for most states.
  - Full frame sequences only for high-value motion where transforms alone would look inadequate.
- Fidelity rule: **production-normalized fidelity**.
  - Preserve reference designs and style.
  - Normalize runtime-facing scale, canvas bounds, pivots, transparency, lighting direction, outline/readability treatment, and VFX intensity so the set behaves coherently in-game.
- Encounter scope: **core slice only**.
  - Sir Clickington
  - Goblin Raider
  - Slime
  - Mimic Chest
  - Lord Blobert
- Existing-art rule: **preserve-and-fill**.
  - Keep every existing runtime asset that already meets the approved visual and technical standard.
  - Preserve existing paths and Unity `.meta` GUIDs whenever technically safe.
  - Generate or repair only assets that are missing, incomplete, or visibly below the approved quality bar.
- Playable-path scope: **complete slice path**, not only the board.
- Production approach: **contract-first preserve-and-fill**.

## 3. Source of Truth and Architecture

The art pass is a production asset system, not a collection of disconnected images.

The authoritative flow is:

`reference images -> runtime asset contract -> existing-asset audit -> KEEP/REPAIR/CREATE manifest -> implementation-ready assets -> Unity validation -> Phase B reference sheets`

The runtime asset set is the source of truth. Composite boards, mockups, and encyclopedia-style sheets are documentation outputs and may not override or drift from the validated runtime files.

The work must follow existing repository boundaries rather than inventing a parallel presentation system. Relevant existing contracts include:

- `Assets/ClickDungeon/Presentation/Assets/HeroPresentationAssets.cs`
- `Assets/ClickDungeon/Presentation/Assets/MonsterPresentationAssets.cs`
- `Assets/ClickDungeon/Presentation/Assets/DungeonRoomPresentationLayout.cs`
- `Assets/ClickDungeon/Presentation/Assets/GameplayScreenPresentationLayout.cs`
- `Assets/ClickDungeon/Presentation/Assets/PresentationAssetDatabase.cs`
- `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`
- `Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs`
- `Assets/ClickDungeon/Presentation/Menu/MenuOverlayFactory.cs`
- `Assets/ClickDungeon/Art/Runtime/`

The implementation plan may add narrowly scoped presentation bindings or validation helpers when required, but it must not replace the existing presentation architecture simply to accommodate new art.

## 4. KEEP / REPAIR / CREATE Manifest

Before new production art is generated, every asset required by the approved vertical slice must receive exactly one disposition:

### KEEP

Use the existing asset unchanged when it already meets the visual and technical contract.

Requirements:

- preserve current file path;
- preserve current `.meta` GUID;
- preserve presentation ID mapping;
- do not regenerate merely for stylistic uniformity if the asset already works at runtime quality.

### REPAIR

Use the existing identity/content but correct a production defect such as:

- opaque or dirty background;
- poor crop or padding;
- incorrect scale;
- inconsistent pivot/contact point;
- weak board-scale readability;
- missing required state coverage;
- import configuration that prevents correct use.

For a repaired asset, preserve the existing path and `.meta` GUID whenever technically safe. If a repair requires replacement that cannot safely retain the metadata relationship, the implementation must explicitly document the reason before changing the GUID.

### CREATE

Create a new asset only when no adequate runtime asset exists for a required state, interaction, UI role, VFX role, or presentation slot.

### Manifest fields

Each required asset entry must record at minimum:

- canonical runtime asset ID;
- file path or planned file path;
- category;
- intended runtime role;
- reference identity/source;
- `KEEP`, `REPAIR`, or `CREATE`;
- exact canvas dimensions after the family contract is locked;
- pivot/contact convention;
- animation sequence membership when applicable;
- expected Unity import settings;
- expected presentation/database binding;
- validation status.

The manifest is the completeness checklist. No required asset may remain unclassified when Phase A begins production generation.

## 5. Runtime File Contract

### 5.1 Runtime files

Implementation-ready source assets are isolated transparent PNG files wherever transparency is appropriate.

Runtime PNGs must not contain:

- sheet backgrounds;
- captions;
- filenames rendered into the image;
- decorative presentation frames;
- callout text;
- reference-sheet chrome.

Composite atlases may be produced later for optimization, but the stable semantic runtime files and IDs remain authoritative.

### 5.2 Static states

A static state is represented by one transparent PNG unless the existing runtime pipeline already uses an equivalent stable representation.

### 5.3 Animated states

Frame sequences use deterministic numbered filenames and one fixed canvas/pivot contract for the entire sequence, for example:

- `attack_01`
- `attack_02`
- `attack_03`

No frame in one sequence may silently change canvas dimensions, pivot, character scale, or contact point.

### 5.4 Character canvas rules

Each scale class uses a shared normalized character canvas rather than arbitrary pose-by-pose cropping.

For grounded characters:

- the foot/contact point remains stable across idle, attack, hit, and defeat transitions where the pose allows it;
- visual state swaps may not cause unintended tile-position jumping;
- the silhouette must remain readable at actual gameplay size.

Large bosses may use a larger scale class, but must still fit the board presentation without obscuring critical tile information.

### 5.5 Effects separation

Reusable effects should remain separate from character art wherever practical. Examples include:

- sword slash arc;
- damage spark;
- shield flash;
- selection ring;
- heal glow;
- pickup sparkle;
- reward burst;
- boss attack effect;
- trap warning overlay.

Painted frame animation is reserved for silhouette-changing motion or interactions where a transform-only solution would visibly fail.

## 6. Fidelity and Style Contract

The approved style rule is production-normalized fidelity.

The following reference characteristics must be preserved:

- character identity;
- costume and equipment identity;
- major palette relationships;
- silhouette and recognizable shape language;
- dark-fantasy chibi/painterly rendering direction;
- gold-trimmed, jewel-like UI language where appropriate;
- dungeon material identity;
- overall product tone.

Normalization is allowed only to improve runtime cohesion and usability, including:

- consistent board-scale character proportions;
- stable canvas padding;
- stable pivots/contact points;
- coherent lighting direction;
- coherent outline/readability treatment;
- consistent transparency cleanup;
- controlled VFX brightness and footprint;
- sufficient silhouette separation from floor art.

Normalization must not redesign a supplied identity into a different character or substitute another hero/monster's art because it shares mechanics.

## 7. Approved Runtime Character and Encounter Set

No additional encounter roster is introduced by this art pass.

### 7.1 Sir Clickington

Required coverage:

- master/showcase art where the current presentation uses it;
- gameplay art;
- portrait;
- roster/card art where the current presentation uses it;
- idle;
- hit;
- victory;
- defeat;
- short frame-animated primary sword attack.

Additional interaction or shield/block poses are added only if the current approved slice visibly invokes them and existing presentation cannot communicate them cleanly with the base states plus VFX.

Identity requirements include the supplied lucky crown, adventurer scarf, sword, royal shield, palette, and overall mascot silhouette.

### 7.2 Goblin Raider

Required coverage:

- gameplay art;
- portrait/master only where a current presentation slot requires it;
- spawn where visible;
- idle;
- attack;
- hit;
- defeat;
- alert treatment through a reusable overlay/VFX when possible.

The attack defaults to a key pose plus Unity motion/VFX unless runtime review proves that a short painted sequence is necessary.

### 7.3 Slime

Required coverage:

- spawn where visible;
- idle;
- attack/body-impact pose;
- hit;
- defeat.

Simple bounce and squash/stretch should be driven by Unity transforms where that produces an acceptable result rather than multiplying painted frames.

### 7.4 Mimic Chest

Required coverage:

- chest disguise;
- suspicious/transition state if needed for timing;
- frame-animated reveal;
- idle monster state;
- frame-animated bite/tongue attack;
- hit;
- defeat/opened-remains state.

The reveal and bite are mandatory high-value hybrid-animation sequences because silhouette transformation is central to the encounter's readability.

### 7.5 Lord Blobert

Required coverage:

- master/showcase or boss portrait where the current boss presentation requires it;
- gameplay art;
- spawn/entrance if visible;
- idle;
- hit;
- defeat;
- at least one signature frame-animated attack sequence;
- separate signature VFX where practical.

Boss scale may be larger than standard enemies, but cannot hide the information needed to understand the 5x5 board.

## 8. Dungeon and Interactable Contract

`DungeonRoomPresentationLayout` already defines the canonical 5x5 tile vocabulary. The art pass preserves that vocabulary rather than adding new simulation concepts merely to create more art.

The canonical environment set includes the existing identities for:

- stone floor;
- cracked floor;
- moss floor;
- water;
- lava;
- shadow;
- pit trap;
- bomb trap;
- spike trap;
- pressure plate;
- teleport;
- healing fountain;
- stair up;
- locked stair up;
- stair down;
- locked stair down;
- wall;
- wall corner;
- key;
- closed chest;
- open chest;
- locked door;
- open door;
- torch.

Existing approved tile art should be kept. New art is created only when a required identity is missing, visibly inadequate, or when the current gameplay exposes a distinct visual state that the existing asset set cannot communicate.

Required interaction/presentation states for the slice include, where actually exercised by the current flow:

- normal chest closed/open;
- Mimic disguise/reveal states;
- key/inventory/use presentation;
- locked/unlocked door or stair state;
- exit/resolution presentation;
- switch/pressure-plate state when used;
- trap telegraph and triggered state when used;
- selected/target/danger overlays.

No additional environmental gameplay system is added solely because a supplied reference board depicts one.

## 9. UI, HUD, and Flow-Screen Contract

The current normalized gameplay layout remains authoritative. New art must fit the existing HUD, board, action-bar, and footer hierarchy in supported orientations rather than forcing a screen-layout rewrite.

### 9.1 Title/start presentation

The complete slice includes production-quality art needed for the existing title/start flow, including as actually used:

- ClickDungeon title treatment;
- dungeon backdrop;
- Sir Clickington showcase/master presentation;
- hero portrait/card treatment;
- Continue/Play presentation;
- Daily Reward presentation;
- bottom-navigation icons/panels;
- modal/panel chrome;
- button visual states needed by current controls.

The art pass skins and completes the existing menu architecture. It does not invent a separate menu navigation system.

### 9.2 Gameplay HUD and actions

Required gameplay-facing coverage includes, where present in the approved slice:

- attack/slash;
- movement/interact;
- shield/block if active in the current flow;
- potion/heal;
- key/open/interact;
- hero health;
- monster/boss health;
- coins;
- gems if displayed;
- Special Key if displayed;
- selected/hovered/targetable state;
- blocked/disabled state;
- tile selection;
- danger/target highlight;
- enemy alert.

No UI icon is generated simply because it appeared in an earlier concept board if the current playable slice does not expose that action or resource.

### 9.3 Victory/reward/end-of-run presentation

The approved complete slice includes:

- victory title/treatment;
- Sir Clickington victory pose;
- reward chest/container states where used;
- coin/gem/key/loot icons actually awarded or displayed;
- reward burst;
- continue/return controls;
- panel frame/background needed by the current reward flow.

## 10. Reusable VFX and Overlays

Phase A must provide or preserve the effects required to make the slice readable and polished at runtime.

Expected categories include:

- sword slash arc;
- attack impact;
- hit flash/damage burst;
- shield/block flash if used;
- heal glow;
- pickup sparkle;
- unlock pulse;
- chest-opening burst;
- Mimic reveal effect;
- Lord Blobert signature effect;
- defeat/fade support;
- tile selection highlight;
- enemy alert marker;
- trap warning/trigger effect;
- victory/reward burst.

Effects must remain subordinate to board information. They may not obscure critical tile state or make selection/target information unreadable.

## 11. Hybrid Animation Rules

Key poses are the default. Full painted sequences are used only where they materially improve the approved slice.

Mandatory sequence candidates are:

- Sir Clickington primary sword attack;
- Mimic reveal;
- Mimic bite/tongue attack;
- Lord Blobert signature attack;
- chest/reward opening where a static state change plus VFX is visibly insufficient.

A sequence is accepted only if:

- every frame uses one stable canvas;
- every frame uses one stable pivot/contact convention;
- character identity does not drift across frames;
- anticipation, impact, and recovery are readable at gameplay scale;
- transparent edges are clean;
- reusable VFX are not unnecessarily baked into character frames.

Everything else should prefer key poses plus Unity-driven motion, timing, squash/stretch, fades, flashes, or reusable VFX.

## 12. Phase A Production Order

Phase A proceeds in small validated batches.

### Batch 1: Audit and manifest

- enumerate the full approved slice asset contract;
- map existing runtime files and presentation IDs;
- assign exactly one of `KEEP`, `REPAIR`, or `CREATE`;
- record current `.meta` GUIDs for every existing asset that may be touched;
- lock exact canvas, pivot, naming, frame-count, and import rules per asset family before generation for that family begins.

### Batch 2: Sir Clickington

- complete required character states;
- complete primary attack sequence;
- complete any required character-specific VFX;
- validate at actual board scale and menu/reward presentation scale.

### Batch 3: Core encounters

- Goblin Raider;
- Slime;
- Mimic;
- Lord Blobert.

Validate each encounter in the actual 5x5 composition before downstream UI polish is considered final.

### Batch 4: Interactables and board overlays

- fill only missing/inadequate canonical environment and interaction art;
- complete chest/key/lock/exit presentation required by the slice;
- complete selection, target, danger, trap, and interaction overlays.

### Batch 5: HUD and reusable VFX

- action/resource icons actually used;
- health/status presentation;
- alert and targeting feedback;
- reusable combat and interaction VFX.

### Batch 6: Title/start and victory/reward presentation

- complete the start-side production art;
- complete end-of-run production art;
- verify that both screens visibly belong to the same product as the 5x5 dungeon board.

### Batch 7: Integrated validation

Run the complete slice and all technical gates before Phase B begins.

## 13. Phase A Validation and Failure Handling

Phase A is accepted only when it is visually correct, contract-complete, deterministic, and safe for the existing Unity project.

### 13.1 Manifest completeness

Hard failures include:

- missing required asset ID;
- duplicate aliases for one semantic identity;
- unresolved manifest entries;
- orphaned animation frames;
- inconsistent sequence dimensions;
- inconsistent pivot conventions;
- accidental opaque background where transparency is required;
- missing binding for a required runtime role.

### 13.2 Existing asset protection

Any asset marked `KEEP` must retain:

- file path;
- `.meta` GUID;
- presentation identity;
- current valid import behavior.

Uncertain generated art must not overwrite verified art. If a proposed replacement is weaker, inconsistent, misframed, incorrectly transparent, or does not preserve the reference identity, it stays out of the runtime set and the manifest remains unresolved until corrected.

### 13.3 Deterministic Unity import

After import and validation, the working tree may contain only expected asset, metadata, manifest, test, and presentation-binding changes.

A second import/validation pass must produce no unexpected tracked-file mutations.

Unexpected Unity mutation is a failure that must be understood and repaired rather than normalized as acceptable noise.

### 13.4 Runtime slice verification

The complete approved flow must be exercised:

`title/start -> 5x5 run -> Goblin Raider/Slime/Mimic encounters -> Lord Blobert -> exit/resolution -> victory/reward`

Verify that:

- state changes do not cause unintended position jumps;
- animation frames do not clip or scale inconsistently;
- characters do not obscure critical neighboring tiles;
- selection/target/danger overlays remain legible;
- hazards/interactables are distinguishable at a glance after they are meant to be visible;
- VFX do not overpower board information;
- title, gameplay, boss, and reward presentation read as one coherent product;
- supported orientations preserve the existing HUD/board/action/footer hierarchy.

### 13.5 Visual identity acceptance

The slice must preserve:

- Sir Clickington as the unmistakable supplied mascot identity;
- Goblin Raider as the approved goblin identity;
- the approved Slime identity;
- the Mimic's deceptive chest-to-monster identity;
- Lord Blobert as the approved boss identity.

Production normalization may not become redesign.

## 14. Existing Test and Verification Boundaries

The implementation should extend existing verification rather than create a disconnected test harness. Relevant existing coverage includes presentation tests around:

- canonical dungeon tile registry;
- layered tile presentation;
- gameplay screen layout;
- hero presentation contracts;
- menu routing.

The implementation plan should add focused tests only where needed to prove new asset-contract behavior, manifest completeness, presentation IDs, or deterministic import assumptions.

The final implementation must also pass the repository's normal compile/test/Unity validation gates that are applicable to the changed files.

## 15. Phase B Reference-Sheet Deliverables

Phase B may start only after Phase A is green.

Phase B produces five polished documentation/reference outputs derived from the validated runtime set:

1. Sir Clickington character sheet.
2. Core enemy/boss sheet for Goblin Raider, Slime, Mimic, and Lord Blobert.
3. Dungeon/interactable sheet.
4. UI/VFX sheet.
5. Complete vertical-slice presentation sheet showing the coherent start-to-reward product language.

These sheets are documentation and presentation artifacts. They are not runtime dependencies and may not introduce new gameplay scope or undocumented production identities.

The previously created composite concept sheets for special chest/key, common encounters, and core ability/consumable UI remain concept/reference material only. They do not count as implementation-ready assets because they are composite boards rather than isolated transparent runtime sprites.

## 16. Explicit Non-Goals

This art pass does not include:

- Goblin Bomber;
- Goblin Key Warden;
- Skeleton Warrior;
- Fire Imp;
- Cave Spider;
- Crowned Slime;
- Armored Boar;
- Bat Swarm Leader;
- Spooky Spellbook;
- Theater Curtain Demon;
- later boss/monster encyclopedia content;
- later hero identities such as Ironheart, Emberwisp, Shadowcut, Lightbringer, Dawnward, Rageclaw, Gearspark, or Windsong;
- new biomes;
- a full-game art refresh;
- unrelated gameplay-system redesign;
- replacement of verified runtime art solely to make everything newly generated;
- a new menu/navigation architecture;
- composite reference boards as runtime sprite sources.

Future work may cover those areas in separate specs after this vertical slice is complete.

## 17. Completion Criteria

The production-art effort is complete only when all of the following are true:

- the approved complete slice path is visually production-ready from title/start through reward/end-of-run;
- the required Sir Clickington, Goblin Raider, Slime, Mimic, and Lord Blobert art is complete;
- required environment, interactable, UI, HUD, overlay, and VFX assets are complete;
- every required asset has a resolved manifest status;
- existing good assets and GUIDs have been preserved wherever technically safe;
- runtime assets are isolated implementation-ready files, not composite boards;
- hybrid sequences pass fixed-canvas/fixed-pivot checks;
- Unity import is deterministic with no unexplained tracked-file mutation;
- existing presentation contracts remain intact or are extended narrowly and explicitly;
- actual board-scale visual review passes;
- the full approved runtime path passes validation;
- only after that, the five Phase B reference sheets are produced from the shipping asset set.

The governing quality rules are: **faithful to references, preserve-first, implementation-ready, deterministic in Unity, and complete for the approved playable slice without expanding into later-game content.**
