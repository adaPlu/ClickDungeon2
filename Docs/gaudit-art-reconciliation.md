# ClickDungeon art reconciliation — repository pass

Branch: `gaudit-asset-reconciliation`

This file records the repository-side `/Gaudit` decisions for the production art integration pass. It is deliberately conservative: approved sources are reused, existing canonical assets are preserved, and new files are not created when a current runtime asset already satisfies the same logical role.

## A–I classification

| Class | Meaning | Current examples / action |
| --- | --- | --- |
| A | Canonical runtime production asset already present and registered | Existing biome masters, hero core sheets, monster/boss core sheets, keys, chests, exits, gold, merchant, healing potion, trap-disarm kit and current trap icons. Preserve paths/IDs unless a migration is explicitly tested. |
| B | Canonical runtime asset exists but presentation/gameplay wiring still needs verification | Ironheart/Sir Clickington derived visuals, chest open/closed sequence, key/exit presentation, trap presentation. Verify in scenes/UI before replacement. |
| C | Approved source/master exists outside the current runtime tree and requires controlled extraction/import | Approved hero masters, item source sheets and item-manifest outputs supplied for this reconciliation pass. Import only after duplicate/hash reconciliation. |
| D | Approved reference/key-pose master, not a literal sprite atlas | Supplied monster encyclopedia sheets. Reconstruct production frames/clips from the approved poses; do not slice the whole encyclopedia image as an animation sheet. |
| E | Deliberate reuse of an existing canonical asset | Existing chest/key/gold/healing-potion/trap-kit assets and manifest-declared duplicate/reuse decisions. Reuse; do not redraw or duplicate. |
| F | Duplicate/stale candidate that must not become a second canonical asset | Any newly extracted file whose logical ID or payload duplicates A/E. Remove or redirect references rather than registering a second ID. |
| G | Genuine runtime art gap with approved visual direction available | Modular 5×5 stone room set: floor variants, walls, corners, torch/lighting/shadow layer, locked-door/lock presentation, dedicated spikes and bomb presentation. `PLAN.png` is reference only, never a giant room background. |
| H | Visual source unresolved; search prior approved material before generating | Player-facing Thief/Shadowcut master art. The procedural `hero_thief_core.png` remains a valid existing runtime fallback, but it is not evidence of the approved final master. |
| I | Deferred expansion / later production phase | Rageclaw, Gearspark, Lightbringer as roster-expansion candidates; cloud saves, leaderboards, seasons, store and Google Play purchase work. |

## Canonical runtime contract discovered in the repo

- Runtime art root: `Assets/ClickDungeon/Art/Runtime`.
- Provenance/canonical source manifest: `Assets/ClickDungeon/Art/Source/asset_manifest.json`.
- Presentation database generation is centralized in `PresentationAssetGenerator`; do not create a competing registry.
- Existing `_core` procedural hero/monster/boss sheets may continue using the current importer/animation slicing rules.
- Approved monster encyclopedia/reference masters added in this pass must **not** be treated as `_core` atlases.
- The current 5×5 board uses a five-column UI grid with content icons over colored cells; modular floor/wall/corner art is not currently present in `Art/Runtime`.
- Player-facing branding remains **ClickDungeon**. `ClickDungeon2` is repository/internal history only.

## Stable modular room presentation IDs

The asset mapper now reserves the following filename-to-ID convention so production art can be swapped without changing gameplay references:

| Filename convention | Presentation ID |
| --- | --- |
| `dungeon_floor_<variant>.png` | `dungeon.floor.<variant>` |
| `dungeon_wall_<side>.png` | `dungeon.wall.<side>` |
| `dungeon_corner_<corner>.png` | `dungeon.corner.<corner>` |
| `dungeon_torch.png` | `dungeon.torch` |
| `dungeon_door_locked.png` | `dungeon.door.locked` |
| `dungeon_lock.png` | `dungeon.lock` |
| `dungeon_shadow.png` | `dungeon.shadow` |
| `trap_spikes.png` | `trap.spikes` via the existing trap convention |

No bomb ID is frozen in this contract yet because the repository has no current bomb content/runtime identifier. The supplied item manifest classifies a bomb as item content while the room plan also depicts bomb-like hazard presentation; that semantic distinction must be resolved against gameplay content before creating a canonical runtime ID.

## No-duplicate decisions carried forward

1. Keep rarity frames independent from item textures; rarity is a UI layer, not baked item art.
2. Reuse current `chest_closed`, `chest_open`, `small_key`, `big_key`, `gold`, `healing_potion`, and `trap_disarm_kit` assets where their canonical logical role matches.
3. Do not re-register Crown of Kings or other manifest-declared reused assets under second IDs.
4. Do not generate final Thief/Shadowcut art until the prior approved source search is exhausted.
5. Do not convert `PLAN.png` into a room-sized background texture.
6. Do not register supplied monster reference sheets as runtime animation atlases.

## Dependency order after this contract

1. Produce/import the modular 5×5 stone-room runtime set using the stable IDs above.
2. Layer the room art into `RuntimeGameUI` without changing deterministic simulation state.
3. Verify/reuse chest, key, lock, pit and trap assets; resolve bomb semantics before import.
4. Verify Ironheart and Sir Clickington production visuals end-to-end.
5. Reconstruct and integrate one approved monster completely (anchors, idle/attack/hit/victory/defeat, VFX timing), then scale to the remaining approved monster masters.
6. Reconcile the supplied individual item manifest into Unity definitions/inventory/equipment/reward/save paths.
7. Run full core-loop QA and final `/graphRepair` before any deferred live-service work.
