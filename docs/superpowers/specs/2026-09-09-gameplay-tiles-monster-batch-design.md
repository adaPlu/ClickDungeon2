# ClickDungeon Gameplay Screen, Dungeon Tiles, and Monster Batch 1 Design

Date: 2026-09-09
Status: Approved architectural direction; written spec awaiting final review
Branch: `feature/visual-remaster-production`
Companion spec: `2026-09-09-hero-identity-class-remaster-design.md`

## 1. Goal

Extend the approved visual remaster so the live gameplay screen, the usable dungeon tile vocabulary, and the first ten canonical monster identities all match the newly uploaded ClickDungeon reference sheets.

This is a real runtime/presentation integration, not a screenshot reskin. The third-to-last uploaded image (`main(1).png`) is the canonical gameplay-layout reference. The last two uploaded tile sheets are the canonical dungeon-tile visual reference. The ten uploaded monster encyclopedia sheets plus the combined roster are the canonical art/identity reference for Monster Batch 1.

No new image generation is part of this design phase. Reference-sheet artwork must not be used as a giant background or runtime atlas with labels/borders attached.

## 2. Approved rollout policy: additive compatibility

Use the user-approved **Option A**.

Existing unremastered monsters and bosses remain valid and encounterable. Monster Batch 1 is added or safely remapped into the existing content model without deleting the legacy roster. Only the ten identities in this spec are required to meet the new production-art fidelity gate now. Remaining legacy monsters are intentionally deferred to later remaster batches.

Do not globally restrict normal floor generation to these ten identities.

## 3. Canonical gameplay-screen target

The landscape gameplay presentation must visually follow `main(1).png` while preserving the current deterministic simulation and command flow.

Required hierarchy:

1. **Top brand/HUD strip**
   - ClickDungeon2 branding at upper left.
   - Selected hero portrait with a compact level/progression badge.
   - Prominent red HP bar.
   - Blue class resource/energy bar when the active rules expose such a resource; otherwise use the existing ability/recharge resource presentation rather than inventing a second simulation currency.
   - Gold and gem/currency counters grouped across the upper area.
   - Settings/menu controls at upper right.

2. **Floor placard and dungeon framing**
   - Parchment-style floor/depth placard near the upper-left side of the board.
   - Stone dungeon frame, warm torch lighting, dark recesses, and banner/ornamental framing consistent with the reference.
   - Decorative framing must never obscure playable cells or tap targets.

3. **Central board**
   - Preserve the current authoritative 5×5 board.
   - Make it the dominant screen element in landscape instead of the current three-column information/board/control split.
   - Each board cell displays a true top-down dungeon tile as its base layer, then content/occupant/state overlays.
   - Hero, monster, chest, key, trap, door/exit and other gameplay sprites must be centered and clearly readable at gameplay scale.
   - Threat, clue, selection, damage and targeting states remain overlays and may not permanently recolor the source art.

4. **Primary action bar**
   - Large, high-contrast action buttons directly below the board, matching the visual weight of the reference `MOVE / SLASH / SHIELD / DASH / POTION` row.
   - Actual labels/actions are driven by the selected class, equipped/available abilities and current simulation state. The reference labels are visual examples, not a mandate to give every class Knight actions.
   - Consumable count badges remain visible on consumable actions.

5. **Persistent footer navigation**
   - Inventory, Talents, and Shop form the main bottom navigation row.
   - Current choice/interaction panels open without covering the entire board unless modal interaction is required.

Portrait/mobile layout may reflow vertically, but it must keep the same visual hierarchy: HUD → board → primary actions → footer. Landscape is the fidelity reference.

## 4. Runtime UI architecture

Refactor `RuntimeGameUI` presentation structure without moving game rules into UI code.

The current `RuntimeGameUI` already owns a programmatic 5×5 `GridLayoutGroup` and submits commands to the simulation. Preserve that boundary. Replace the current landscape 22% info / 54% board / 24% control-column arrangement with the reference-driven top HUD + dominant center board + bottom action/footer composition.

Keep `DungeonRoomPresentationLayout` as the engine-free presentation contract for board dimensions and visual placement. Extend it for canonical tile IDs and tile-layer decisions rather than hard-coding decorative cracked-floor cells as the only floor variation.

The UI must continue to render from `RunState`/content data. The uploaded gameplay mockup is never used as a flattened background image.

## 5. Canonical usable dungeon tile set

Normalize the union of the final two tile sheets into one canonical runtime vocabulary. Prefer the final sheet's `tile_*` file naming convention.

Required files/visual identities:

- `tile_floor_stone.png`
- `tile_floor_cracked.png`
- `tile_floor_moss.png`
- `tile_water.png`
- `tile_lava.png`
- `tile_shadow.png`
- `tile_trap_pit.png`
- `tile_trap_bomb.png`
- `tile_trap_spike.png`
- `tile_pressure_plate.png`
- `tile_teleport.png`
- `tile_fountain_heal.png`
- `tile_stair_up.png`
- `tile_stair_up_locked.png`
- `tile_stair_down.png`
- `tile_stair_down_locked.png`
- `tile_wall.png`
- `tile_wall_corner.png`
- `tile_key.png`
- `tile_chest_closed.png`
- `tile_chest_open.png`
- `tile_door_locked.png`
- `tile_door_open.png`
- `tile_torch.png`

All are square top-down production sprites, grid-aligned and suitable for Unity import. Where an equivalent runtime asset already exists, replace/reconcile it in place and preserve its `.meta`/GUID. Do not create duplicate aliases merely because an older sheet used a different filename.

## 6. Tile semantics and layering

Use a layered cell model:

**base floor/terrain → structural tile → content/occupant → state/telegraph overlay**.

Existing simulation semantics remain authoritative. Map new visuals onto existing semantics wherever possible:

- normal floor → stone, with cracked/moss as deterministic visual variants where they do not change rules;
- flooded/water terrain → water tile;
- lava terrain → lava tile;
- dark/void-compatible biome or special terrain → shadow tile only where the simulation marks that state; never infer lethal void behavior from art alone;
- `trap.pitfall` → pit tile;
- existing chest available/resolved → closed/open chest;
- keys → key tile/icon presentation;
- exits/routes → appropriate stair/door locked/unlocked presentation based on actual accessibility/state;
- walls/corners/torches → room framing/decorative structure unless backed by an actual board-state object.

For visual identities with no current deterministic mechanic, add the smallest simulation primitive necessary before allowing the generator to place them as gameplay-affecting tiles:

- `trap.bomb`: direct deterministic damage on resolution/trigger;
- `trap.spikes`: direct deterministic damage on resolution/trigger;
- pressure plate: a deterministic one-shot trigger object whose first implementation toggles/unlocks a linked generated route/door state selected during floor generation;
- teleport: a deterministic paired teleport interaction; destination pairing is established during floor generation and stored in state rather than chosen ad hoc by UI;
- healing fountain: a one-shot interactable that restores a bounded amount of HP and then resolves.

If an implementation discovers an existing primitive that already provides one of these semantics, reuse it rather than adding a duplicate system.

## 7. Monster Batch 1

The first canonical remastered monster set contains exactly these ten identities:

| Identity | Runtime classification | Canonical presentation family |
| --- | --- | --- |
| Goblin Brute King | Boss / Brute | `goblin_brute_king` |
| Crowned Slime | Regular / Brute | `crowned_slime` |
| Skeleton Warrior | Regular / Undead | `skeleton_warrior` |
| Bat Swarm Leader | Boss / Swarm | `bat_swarm_leader` |
| Mimic Chest | Regular / Trickster | `mimic_chest` |
| Fire Imp | Regular / Magic | `fire_imp` |
| Armored Boar | Regular / Beast | `armored_boar` |
| Spooky Spellbook | Regular / Magic | `spooky_spellbook` |
| Cave Spider | Regular / Beast | `cave_spider` |
| Theater Curtain Demon | Boss | `theater_curtain_demon` |

The existing `ProductionArtManifest` classification is authoritative for filename prefix: Goblin Brute King, Bat Swarm Leader, and Theater Curtain Demon use `boss_`; the other seven use `monster_`.

Each family keeps the existing ten-variant runtime contract:

`master`, `portrait`, `roster`, `gameplay`, `spawn`, `idle`, `attack`, `hit`, `victory`, `defeat`.

That is exactly 100 required Batch-1 monster/boss PNGs. Existing `.meta`/GUIDs are preserved when replacing already-tracked files.

## 8. Content-ID compatibility mapping

Do not rename legacy content IDs merely to make them match art filenames. Add a small explicit content-ID → presentation-family resolver where needed.

Treat these as direct identity refinements of existing content IDs:

- `monster.slime` → display identity **Crowned Slime** → presentation family `crowned_slime`;
- `monster.skeleton` → display identity **Skeleton Warrior** → presentation family `skeleton_warrior`;
- `monster.spider` → display identity **Cave Spider** → presentation family `cave_spider`.

Keeping these content IDs preserves saved-floor/content compatibility while allowing their visuals and descriptive identity to match the approved sheets.

Add new regular content IDs for identities that are not safe refinements of an existing generic monster:

- `monster.mimic_chest`
- `monster.fire_imp`
- `monster.armored_boar`
- `monster.spooky_spellbook`

Keep existing generic `monster.goblin`, `monster.bat`, `monster.demon`, and every other legacy monster. The boss identities are distinct and use new IDs:

- `boss.goblin_brute_king`
- `boss.bat_swarm_leader`
- `boss.theater_curtain_demon`

## 9. Batch-1 behavior identity

Use the uploaded encyclopedia sheets as the behavioral design reference but fit behavior into the current deterministic intent/threat system first. Do not create ten bespoke combat engines.

- **Goblin Brute King:** durable heavy melee boss; club/ground-slam pressure, greed/gold flavor, later-phase enrage/defend-the-crown behavior.
- **Crowned Slime:** durable brute; greed/treasure identity, body/slime attacks; splitting is implemented only if it can be deterministic and validated without destabilizing board occupancy.
- **Skeleton Warrior:** defensive undead; sword/shield pressure and guard/block identity.
- **Bat Swarm Leader:** high-mobility swarm boss; swarm/summon pressure and dive-style attack identity.
- **Mimic Chest:** disguised surprise attacker; bite/chomp pressure and greed-punisher identity.
- **Fire Imp:** fast low-health fire threat; ranged/fire pressure with a deterministic death-burst only if supported by the damage pipeline.
- **Armored Boar:** durable physical charger; heavy telegraphed charge/trample behavior.
- **Spooky Spellbook:** ranged magical summoner; arcane/shadow attack and page-minion pressure.
- **Cave Spider:** web/poison control; movement denial plus venom identity.
- **Theater Curtain Demon:** theatrical control boss; magic pressure, summons, reposition/teleport and stage-control flavor using existing deterministic primitives where possible.

Exact balance numbers are set during implementation against the current power envelope and verified by deterministic simulation tests; the sheets determine identity and relative role, not unchecked stat inflation.

## 10. Encounter availability under Option A

Regular Batch-1 monsters participate in the existing biome-driven pool alongside legacy monsters. Existing monsters are not removed to make room for them.

Use these first-pass biome memberships:

- Crowned Slime (`monster.slime`): keep its existing memberships; its identity/art is upgraded in place.
- Skeleton Warrior (`monster.skeleton`): keep existing crypt/frozen-ruins membership.
- Cave Spider (`monster.spider`): keep existing sunken-temple/thorn-wilds/mire memberships and add cavern eligibility to reflect the approved cave identity.
- Mimic Chest: all biomes; it is an uncommon monster encounter selected through deterministic weighting/gating rather than appearing at the same frequency as baseline enemies.
- Fire Imp: lava field and ash wastes.
- Armored Boar: thorn wilds and cavern.
- Spooky Spellbook: crypt, storm plateau, and arcane nexus.

The current `MonsterIdsForBiome` path is preserved and extended only as needed for explicit encounter weighting/rarity. Any weighting addition must be deterministic from the existing floor RNG.

## 11. Boss availability without deleting legacy bosses

The current content layer allows only one `BossDefinition` per floor because adding a boss removes another definition with the same floor. Option A therefore requires a small boss-pool extension instead of overwriting the five existing campaign bosses.

Use deterministic candidate pools at milestone floors:

- Floor 10: existing Lich Sovereign + Goblin Brute King.
- Floor 20: existing Rootbound Leviathan + Bat Swarm Leader.
- Floor 30: existing Frostbog Colossus + Theater Curtain Demon.
- Floor 40: existing Archdemon Overlord remains the sole candidate for this batch.
- Floor 50: existing Primal Ancient Wyrm remains the sole candidate for this batch.

Select from a milestone's candidates using a seed derived from the run/floor so save/replay determinism is preserved. Abyss boss cycling must use the same deterministic candidate mechanism rather than silently collapsing back to only the old five IDs.

This is a Batch-1 distribution, not a permanent declaration that those are the only bosses that may ever share those milestones; later monster batches may extend the pools.

## 12. Presentation mapping and fallbacks

`MonsterPresentationAssets` remains presentation-only. Extend it or add a focused resolver so a content ID can resolve to its canonical art family before `master/gameplay/portrait/...` variant lookup.

For the ten Batch-1 identities, missing production art is a validation failure; do not silently substitute unrelated legacy art.

For legacy monsters not yet remastered, retain the current core/legacy fallback behavior so Option A remains playable while later batches are pending.

## 13. Validation and testing

Add these gates to the companion hero-remaster gates:

1. Landscape `RuntimeGameUI` hierarchy has top HUD, dominant 5×5 board, primary action row, and Inventory/Talents/Shop footer; simulation command ownership remains unchanged.
2. The canonical dungeon tile registry resolves all 24 tile visual identities listed in section 5.
3. Tile resolver tests prove correct base/content/state layering and closed/open or locked/unlocked transitions.
4. Bomb, spike, pressure-plate, teleport, and healing-fountain mechanics receive deterministic simulation tests before generator placement is enabled.
5. Monster content validation proves all ten Batch-1 identities resolve to the correct content ID, classification, and presentation family.
6. Production-art validation still requires exactly the existing ten variants for each of the ten Batch-1 families: 100 monster/boss PNGs.
7. Compatibility tests prove `monster.slime`, `monster.skeleton`, and `monster.spider` still load by their legacy IDs while rendering Crowned Slime, Skeleton Warrior, and Cave Spider respectively.
8. Encounter-generation tests prove legacy monsters remain in eligible pools and Batch-1 regular monsters are added deterministically.
9. Boss-pool tests prove legacy bosses remain selectable and the three new bosses are also deterministically selectable at their assigned milestone pools.
10. Visual verification compares the live landscape gameplay screen against `main(1).png` for hierarchy, board dominance, HUD/action/footer placement, tile readability, and dungeon framing.
11. Visual verification checks all ten monster identities for silhouette, palette, major equipment/props, gameplay-scale readability, and animation-state continuity.
12. Existing unexpected-mutation protections remain strict; no validator is weakened to accept missing art or duplicate tile aliases.
13. Normal CI, required Unity validation, `/Gaudit`, and `/graphRepair` must be green/clean before PR #5 merges.

## 14. Explicit non-goals for this batch

- Do not generate replacement images during the design/planning phase.
- Do not remaster the remaining legacy monsters yet.
- Do not delete legacy monsters or legacy bosses.
- Do not replace the actual interactive board with the uploaded gameplay screenshot.
- Do not turn decorative sheet labels/borders into runtime textures.
- Do not introduce a parallel tile, monster, ability, or combat engine when the existing deterministic simulation can be extended.
- Do not merge PR #5 until this companion scope and the approved hero/class remaster complete their shared final validation gates.

## 15. Definition of done

This companion scope is complete when the live gameplay screen follows the approved `main(1).png` hierarchy, the 5×5 board uses the canonical tile vocabulary from the final two tile sheets, all ten Batch-1 monster identities are present with correct art-family mapping and deterministic encounter behavior, the old monster/boss roster remains available under Option A, the three new bosses coexist with the legacy milestone bosses through deterministic boss pools, no unapproved image generation or flattened screenshot substitution has occurred, and all relevant simulation, presentation, Unity, CI, `/Gaudit`, `/graphRepair`, and final visual gates are clean.