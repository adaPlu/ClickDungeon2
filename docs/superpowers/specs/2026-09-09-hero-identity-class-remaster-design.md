# ClickDungeon Hero Identity & Mechanical Class Remaster Design

Date: 2026-09-09
Status: Approved design captured for review before implementation planning
Branch: `feature/visual-remaster-production`

## 1. Goal

Remaster the selectable hero roster so the uploaded hero identity sheets are the canonical visual specification and the game exposes nine selectable hero identities across eight mechanical classes.

The implementation updates art, class mechanics, identity registration, selection UI, presentation mapping, persistence, and validation as one coherent feature. The uploaded sheets are reference material, not giant runtime textures.

## 2. Approved hero/class model

| Hero identity | Mechanical class | Presentation role |
| --- | --- | --- |
| Ironheart | Knight | Tank / Melee |
| Sir Clickington | Knight | Versatile / Durable / Support presentation; unique identity and campaign |
| Dawnward | Paladin | Tank / Support |
| Rageclaw | Berserker | Damage / Melee |
| Gearspark | Engineer | Utility / Support |
| Windsong | Ranger | Damage / Ranged |
| Lightbringer | Cleric | Healer / Support |
| Emberwisp | Wizard | Damage / Magic |
| Shadowcut | Thief | Damage / Melee |

There are exactly eight gameplay classes: Knight, Paladin, Berserker, Engineer, Ranger, Cleric, Wizard, and Thief.

There are exactly nine hero identities because Knight has two identities: Ironheart and Sir Clickington.

Sir Clickington remains mechanically `Knight`. The `classId: clickington` wording on his visual sheet is an identity/archetype label only and must never become a ninth `HeroClassId`. His persistent hero id remains `clickington`, his special campaign remains `clickington_campaign`, and selecting him must never silently resolve to Ironheart.

## 3. Canonical visual identity

The uploaded sheets are the visual source of truth for character silhouette, face, hair, armor/clothing, primary equipment, accent colors, VFX language, and pose intent.

- Ironheart: compact blue-and-gold armored knight, brown spiked hair, sword and blue/gold shield, confident heroic expression.
- Sir Clickington: small cheerful knight, red plume and scarf/cape, silver armor, sword, blue/gold crown shield, intentionally comedic heroic energy.
- Dawnward: white-and-gold paladin plate, blue scarf, cross shield, warhammer, radiant gold holy effects.
- Rageclaw: large red-haired/red-bearded berserker, fur and rugged dark armor, skull motifs, oversized double axe, red/orange rage effects.
- Gearspark: young brown-haired engineer, bright blue goggles, wrench, bronze mechanical companion/core, cyan electrical effects.
- Windsong: elven ranger in green hood/leathers/cloak, longbow and quiver, leaf motifs, green nature effects.
- Lightbringer: blond cleric in white/gold vestments, radiant staff and holy tome, warm gold/white healing effects.
- Emberwisp: enthusiastic brown-haired wizard in red/dark robes, staff and spellbook, orange flame magic.
- Shadowcut: brown-haired thief in black/purple light armor and scarf, dual daggers, violet shadow effects.

Every runtime variant must preserve equipment, silhouette, palette, face/hair, and class VFX strongly enough that the same hero is immediately recognizable.

## 4. Runtime art contract

Keep the existing nine-variant runtime contract for every hero:

`master`, `portrait`, `roster`, `gameplay`, `idle`, `attack`, `hit`, `victory`, `defeat`.

With nine hero identities, the hero production set remains exactly 81 PNG assets. Canonical filenames remain `hero_<heroId>_<variant>.png`.

Existing `.meta` files and Unity GUIDs must be preserved when replacing an existing runtime PNG. Replace PNG content in place rather than deleting/recreating the asset pair.

Runtime images must be isolated production art, not screenshots or crops containing the sheet frame, labels, typography, panel borders, or neighboring poses. Transparent isolated character art is required for sprite/pose variants. Equipment and VFX that belong to the action remain part of the relevant pose. The uploaded full sheets are not committed to Resources as runtime textures.

## 5. Animation-state intent

- `idle`: neutral combat stance.
- `attack`: class-defining offensive action with correct weapon/tool/magic.
- `hit`: impact/recoil without equipment or identity drift.
- `victory`: class-appropriate celebration.
- `defeat`: defeated/exhausted, non-gory.
- `gameplay`: compact chibi/small-runtime representation.
- `portrait` and `roster`: face/silhouette-forward UI crops.
- `master`: high-detail hero selection/presentation illustration.

Sir Clickington retains the humorous mascot tone in every state while remaining a Knight mechanically.

## 6. Mechanical class architecture and compatibility

`HeroClassId` must preserve existing serialized ordinals. Convert the enum to explicit values:

- `Knight = 0`
- `Ranger = 1`
- `Thief = 2`
- `Wizard = 3`
- `Paladin = 4`
- `Berserker = 5`
- `Engineer = 6`
- `Cleric = 7`

Do not reorder these numeric values for presentation. Define a separate UI display order:

Knight → Paladin → Berserker → Engineer → Ranger → Cleric → Wizard → Thief.

Knight exposes Ironheart and Sir Clickington. Every other class exposes one default identity in this remaster.

## 7. Class content and exact first-pass mechanics

Keep the four existing Knight/Ranger/Thief/Wizard definitions and abilities unchanged except for compatibility edits required by the expanded enum/catalog. Bump `classes.json` from revision 3 to 4 and `abilities.json` from revision 2 to 3 when the new records are added.

All new ability mastery thresholds use the existing progression sequence `0, 24, 56, 96, 150`. The deterministic simulation remains authoritative. If one exact behavior below cannot be expressed by the existing effect vocabulary, add the smallest new deterministic simulation primitive required; do not create a parallel ability engine.

### Paladin / Dawnward

Class id: `class.paladin`. Base HP 17, Attack 2, Defense 2. Identity: frontline protection plus sustain.

1. `ability.paladin.radiant_strike` — Radiant Strike. 3 charges, recharge 8. Adjacent attack at normal attack power; on successful hit gain 2 shield.
2. `ability.paladin.lay_on_hands` — Lay on Hands. 2 charges, recharge 10. Restore 5 HP, capped at max HP.
3. `ability.paladin.consecration` — Consecration. 2 charges, recharge 10. Deal 2 damage to each revealed monster within Manhattan distance 1 of the player, then gain 2 shield.
4. `ability.paladin.aegis_of_dawn` — Aegis of Dawn. 2 charges, recharge 11. Gain 6 shield and +1 temporary defense for the next 2 enemy responses.
5. `ability.paladin.divine_bulwark` — Divine Bulwark. 1 charge, recharge 14. Restore 6 HP and gain 8 shield.

Board passive: once per floor, after damage causes current HP to become less than or equal to half max HP, gain 3 shield. The trigger is condition-based and cannot fire more than once on that floor.

### Berserker / Rageclaw

Class id: `class.berserker`. Base HP 16, Attack 4, Defense 0. Identity: high-risk melee momentum.

1. `ability.berserker.cleaving_blow` — Cleaving Blow. 3 charges, recharge 8. Adjacent attack for normal attack power +2.
2. `ability.berserker.bloodrush` — Bloodrush. 2 charges, recharge 10. Lose 2 HP, never reducing HP below 1; gain +2 temporary attack for the next 2 offensive actions.
3. `ability.berserker.war_cry` — War Cry. 2 charges, recharge 10. One revealed target's next offensive intent is forced to its basic attack at reduced intent power, using the existing Knight intent-control semantics where possible.
4. `ability.berserker.frenzy` — Frenzy. 2 charges, recharge 10. Make two consecutive adjacent attacks at normal attack power against the same target, stopping if the target is defeated after the first hit.
5. `ability.berserker.ragequake` — Ragequake. 1 charge, recharge 14. Deal normal attack power +1 to each revealed monster within Manhattan distance 1 of the player.

Board passive: while current HP is less than or equal to half max HP, effective Attack is +1. This is a derived condition, not a stored stack, so it cannot accumulate recursively.

### Engineer / Gearspark

Class id: `class.engineer`. Base HP 14, Attack 2, Defense 1. Identity: gadget-based utility and control.

1. `ability.engineer.shock_wrench` — Shock Wrench. 3 charges, recharge 8. Adjacent attack at normal attack power and root the surviving target for 1 enemy response.
2. `ability.engineer.barrier_drone` — Barrier Drone. 2 charges, recharge 9. Gain 5 shield.
3. `ability.engineer.snare_mine` — Snare Mine. 2 charges, recharge 9. Root one revealed target for 2 enemy responses.
4. `ability.engineer.overclock` — Overclock. 2 charges, recharge 10. Gain +1 temporary Attack and +1 temporary Defense for the next 3 enemy responses; the buff refreshes rather than stacks if re-used while active.
5. `ability.engineer.clockwork_barrage` — Clockwork Barrage. 1 charge, recharge 14. Deal normal attack power to up to 3 revealed monsters selected by the same deterministic targeting/tie-break rules used by existing multi-target abilities.

Board passive: at floor start, identify one non-boss occupied threat cell that is currently hidden or merely clued, choosing the nearest eligible cell by Manhattan distance and then the board's existing stable coordinate order as the tie-break. If no eligible threat exists, the passive does nothing. It never identifies traps, preserving the Thief's trap-information niche.

### Cleric / Lightbringer

Class id: `class.cleric`. Base HP 15, Attack 2, Defense 1. Identity: healing, protection, and recovery.

1. `ability.cleric.smite` — Smite. 3 charges, recharge 8. Deal normal attack power +1 to one revealed monster within Manhattan distance 2.
2. `ability.cleric.mend` — Mend. 3 charges, recharge 8. Restore 5 HP, capped at max HP.
3. `ability.cleric.sanctuary` — Sanctuary. 2 charges, recharge 10. Gain 5 shield and +1 temporary Defense for the next 2 enemy responses.
4. `ability.cleric.blessing` — Blessing. 2 charges, recharge 10. Gain +1 temporary Attack and +1 temporary Defense for the next 3 enemy responses; re-use refreshes rather than stacks.
5. `ability.cleric.radiant_renewal` — Radiant Renewal. 1 charge, recharge 14. Restore 8 HP, capped at max HP, then gain 5 shield.

Board passive: once per floor, immediately after the first successfully completed shrine interaction, restore 3 HP, capped at max HP. If no shrine is completed, the passive does not trigger.

## 8. Hero identity catalog

Canonical class-to-default-hero mapping:

- Knight → `ironheart`
- Paladin → `dawnward`
- Berserker → `rageclaw`
- Engineer → `gearspark`
- Ranger → `windsong`
- Cleric → `lightbringer`
- Wizard → `emberwisp`
- Thief → `shadowcut`

`clickington` remains a second Knight identity and retains `clickington_campaign`.

All non-Clickington identities use the normal/default run path and receive no new story campaign in this remaster.

Resolution rules are deterministic: retain a requested hero when it exists and belongs to the requested class; otherwise use that class's default hero. Unknown class values use the existing safe compatibility/error path rather than selecting a different new class. `Knight + clickington` always resolves to `clickington`.

## 9. Presentation architecture

Presentation remains hero-identity driven, not class driven. Runtime keys remain `hero.<heroId>.<variant>` and canonical resource filenames.

Update finite identity switches so all nine approved hero IDs resolve. Prefer a catalog-validated generic resource path where safe. A missing mapping must be caught by validation; no new class may silently borrow Ironheart art because a mapping was omitted.

## 10. Selection UI

Expose all eight classes in the explicit display order from section 6. Hero selection is identity-within-class: Knight cycles Ironheart ↔ Sir Clickington; the other seven classes have one hero and remain stable when hero-cycle input is used.

UI labels show the selected identity and actual mechanical class. Sir Clickington may additionally show his story-campaign indicator, but the mechanical class label remains Knight.

## 11. Persistence compatibility

Existing saves created with the original four enum values remain readable because those numeric values are locked. Persist hero identity and class together through the existing save model and resolve identity against class on load.

Regression requirement: a save containing `HeroClassId.Knight` and hero id `clickington` reloads as Sir Clickington, never Ironheart.

## 12. Validation and tests

The implementation is complete only when all gates are green:

1. Enum/persistence tests prove Knight=0, Ranger=1, Thief=2, Wizard=3.
2. Content validation finds exactly eight supported classes and all referenced abilities.
3. Hero catalog tests prove nine unique identities, eight classes, correct defaults, and exactly two Knight identities.
4. Sir Clickington selection/save/load tests prove he never collapses to Ironheart.
5. Presentation tests resolve every required variant for all nine identities.
6. Production-art validation requires all 81 hero PNGs and rejects missing/misnamed runtime art.
7. New Paladin/Berserker/Engineer/Cleric mechanics have deterministic simulation tests for damage, healing, buffs, passives, charge usage, and expiration/refresh behavior.
8. Existing unexpected-mutation protections remain strict.
9. Existing normal CI is green.
10. Unity EditMode/import validation is run when required by changed Unity assets/code and is green before merge.
11. Final visual verification checks character fidelity, sheet-text/border contamination, crop legibility, transparency, and state continuity.
12. `/Gaudit` and `/graphRepair` run after implementation and before PR #5 merges.

## 13. Change isolation and merge safety

Keep work on PR #5's visual-remaster line, using a fresh child branch/worktree from its current head during implementation if required by the implementation plan/TDD workflow.

Do not merge PR #5 while this remaster is partial. Do not weaken validators. Do not regenerate unrelated monster/environment art.

Preserve hero filenames and existing `.meta` GUIDs. Keep code/content changes scoped to enum expansion, class/ability data, hero identity catalog, selection/presentation plumbing, the smallest required simulation primitives, persistence compatibility, and tests.

## 14. Definition of done

The remaster is done when ClickDungeon presents nine visually faithful hero identities across eight real mechanical classes; Sir Clickington remains a distinct Knight identity with `clickington_campaign`; all 81 runtime hero assets match the approved sheets without sheet UI contamination; old saves remain compatible; every class is selectable and its first-pass mechanics are deterministic and tested; all relevant Unity and CI validation is green; and the final `/Gaudit`, `/graphRepair`, and runtime visual verification gates are clean before merge.