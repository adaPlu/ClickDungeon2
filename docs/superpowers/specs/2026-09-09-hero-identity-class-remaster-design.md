# ClickDungeon Hero Identity & Mechanical Class Remaster Design

Date: 2026-09-09
Status: Approved design captured for review before implementation planning
Branch: `feature/visual-remaster-production`

## 1. Goal

Remaster the selectable hero roster so the uploaded hero identity sheets are the canonical visual specification and the game exposes nine selectable hero identities across eight mechanical classes.

The implementation must update art, class mechanics, identity registration, selection UI, presentation mapping, persistence, and validation as one coherent feature. It must not treat the uploaded sheets as giant runtime textures. The sheets are reference material from which isolated runtime assets are produced.

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

Sir Clickington remains mechanically `Knight`. The `classId: clickington` wording on his visual sheet is an identity/archetype label only and must never become a ninth `HeroClassId`. His persistent hero id remains `clickington`, his special campaign remains distinct, and selecting him must never silently resolve to Ironheart.

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

The remaster must preserve recognizable identity across every runtime variant. A pose can simplify for small sprites, but equipment, silhouette, palette, and face/hair must remain consistent enough that the same hero is immediately recognizable.

## 4. Runtime art contract

Keep the existing nine-variant runtime contract for every hero:

`master`, `portrait`, `roster`, `gameplay`, `idle`, `attack`, `hit`, `victory`, `defeat`.

With nine hero identities, the hero production set remains 81 PNG assets. Existing canonical filenames remain unchanged:

`hero_<heroId>_<variant>.png`

The current `.meta` files and Unity GUIDs must be preserved wherever an existing runtime PNG is being replaced. Replace PNG content in place rather than deleting/recreating the asset pair.

Runtime images must be isolated production art, not screenshots or crops that include the sheet frame, labels, typography, dungeon panel borders, or unrelated neighboring poses. Character art should use transparent backgrounds where the existing presentation contract expects isolated sprites. Equipment and VFX that are part of the action may be included in the relevant pose.

The uploaded full sheets themselves are reference material and are not added to Resources as runtime textures.

## 5. Animation-state intent

Each hero must have coherent state continuity:

- `idle`: readable neutral combat stance.
- `attack`: class-defining offensive action using the correct weapon/tool/magic.
- `hit`: clear impact/recoil without changing identity or equipment.
- `victory`: class-appropriate celebratory pose.
- `defeat`: unambiguously defeated/exhausted pose without gore.
- `gameplay`: compact chibi/small-runtime representation suitable for normal play.
- `portrait` and `roster`: face/silhouette-forward crops optimized for UI legibility.
- `master`: clean high-detail identity illustration used for hero selection/presentation where appropriate.

Sir Clickington's states should retain the humorous mascot tone, while his underlying gameplay class remains Knight.

## 6. Mechanical class architecture

`HeroClassId` must preserve existing serialized ordinals. Convert the enum to explicit values rather than inserting new entries between existing ones:

- `Knight = 0`
- `Ranger = 1`
- `Thief = 2`
- `Wizard = 3`
- `Paladin = 4`
- `Berserker = 5`
- `Engineer = 6`
- `Cleric = 7`

Display order must therefore be defined separately from enum numeric order. The hero/class selector should present:

Knight → Paladin → Berserker → Engineer → Ranger → Cleric → Wizard → Thief.

Knight exposes two hero identities, Ironheart and Sir Clickington. Every other class has one default identity in this remaster.

## 7. Class data and gameplay identity

The four existing classes remain valid and are revalidated, not reinvented. Add four new real class definitions to content data:

### Paladin / Dawnward

Base envelope: HP 17, Attack 2, Defense 2.

Identity: frontline protection plus sustain. Paladin must feel more supportive and restorative than Knight rather than becoming a duplicate shield tank.

Ability kit intent:

1. Radiant Strike — adjacent physical/holy strike that also grants a small shield.
2. Lay on Hands — restore player HP.
3. Consecration — short-radius holy area effect that damages threats and/or improves survival.
4. Aegis of Dawn — strong temporary shield/defense support.
5. Divine Bulwark — ultimate combining substantial protection with recovery.

Board passive: the first critical-health event on a floor grants a small emergency shield. It triggers at most once per floor.

### Berserker / Rageclaw

Base envelope: HP 16, Attack 4, Defense 0.

Identity: high-risk melee momentum. Berserker should reward staying in the fight and trading durability for pressure.

Ability kit intent:

1. Cleaving Blow — heavy adjacent attack.
2. Bloodrush — temporary offensive boost with an explicit survivability cost or risk.
3. War Cry — disrupt or weaken an enemy intent.
4. Frenzy — rapid multi-hit melee attack.
5. Ragequake — ultimate area attack centered on the Berserker.

Board passive: while at or below half HP, Berserker gains a small deterministic attack bonus. The bonus must not stack recursively.

### Engineer / Gearspark

Base envelope: HP 14, Attack 2, Defense 1.

Identity: utility/control through gadgets rather than raw spell damage.

Ability kit intent:

1. Shock Wrench — attack with a short control effect.
2. Barrier Drone — grants shield/protection.
3. Snare Mine — roots or otherwise delays a revealed threat.
4. Overclock — temporary tactical boost that improves the next useful action/ability sequence.
5. Clockwork Barrage — ultimate multi-target gadget attack.

Board passive: once per floor, a nearby/relevant threat or hazard receives an additional deterministic piece of useful information from Gearspark's instrumentation. This must not duplicate the Thief's trap-specialist passive.

### Cleric / Lightbringer

Base envelope: HP 15, Attack 2, Defense 1.

Identity: healing, protection, and momentum recovery.

Ability kit intent:

1. Smite — basic holy/radiant attack.
2. Mend — direct HP restoration.
3. Sanctuary — defensive protection for the next enemy-response window.
4. Blessing — temporary support buff that improves survivability and/or offense without replacing another class's identity.
5. Radiant Renewal — ultimate major recovery plus protection.

Board passive: the first completed shrine interaction on a floor restores a small amount of HP, capped at max HP.

The implementation must first map these semantic behaviors onto existing deterministic simulation primitives. If a behavior cannot be represented by the current effect vocabulary, add only the smallest new deterministic primitive needed for that behavior and cover it with simulation tests. Do not create a second parallel ability engine.

## 8. Hero identity catalog

The canonical class-to-default-hero mapping is:

- Knight → `ironheart`
- Paladin → `dawnward`
- Berserker → `rageclaw`
- Engineer → `gearspark`
- Ranger → `windsong`
- Cleric → `lightbringer`
- Wizard → `emberwisp`
- Thief → `shadowcut`

`clickington` remains a second Knight identity and retains `clickington_campaign`.

The non-Clickington heroes use the normal/default run path unless an already-existing campaign mapping explicitly says otherwise. This remaster does not invent separate story campaigns for the other eight identities.

Identity resolution rules:

1. If persisted/requested hero id exists and belongs to the persisted/requested class, keep it exactly.
2. Otherwise resolve to that class's default hero.
3. Unknown class values must fail through the existing safe compatibility path rather than silently selecting an unrelated new class.
4. `Knight + clickington` must always resolve to `clickington`.

## 9. Presentation architecture

Presentation lookup should remain hero-identity driven, not class driven. Runtime art keys use `hero.<heroId>.<variant>` / canonical resource filenames.

Where current code uses a finite switch for only the original identities, update it so all nine approved hero IDs resolve consistently. Prefer a catalog-validated generic resource path over another growing switch when that can be done without weakening fallback behavior.

No class may accidentally borrow Ironheart's presentation assets merely because a mapping is missing. Missing/invalid art should surface through validation and explicit fallback rules, not silent identity substitution.

## 10. Selection UI

The class selector exposes all eight mechanical classes in the explicit display order from section 6.

The hero selector is identity-within-class:

- Knight: Ironheart ↔ Sir Clickington.
- All other classes: one identity in this remaster, so hero cycling is effectively stable/no-op while still using the same catalog path.

UI labels should show the selected identity name and actual mechanical class. Sir Clickington may additionally indicate his story campaign, but must still display `Knight` as the mechanical class.

## 11. Persistence compatibility

Existing saves created with the four-class enum must remain readable. Explicit enum numeric values preserve the existing Knight/Ranger/Thief/Wizard values.

Persist hero identity and class together using the existing save model. On load, resolve the identity against its class. Old saves without a new-class identity continue to resolve exactly as before.

Required regression case: a save with `HeroClassId.Knight` and hero id `clickington` reloads as Sir Clickington, not Ironheart.

## 12. Validation and tests

The implementation is not complete until all of these gates are green:

1. Enum/persistence tests prove the original four numeric values did not change.
2. Content validation finds exactly eight supported class definitions and all referenced abilities.
3. Hero catalog tests prove nine unique identities, eight classes, correct default mapping, and two Knight identities.
4. Sir Clickington regression tests prove explicit selection/persistence never collapses to Ironheart.
5. Presentation tests resolve every required variant for every hero identity.
6. Production-art validation still requires all 81 hero PNGs and rejects missing/misnamed runtime art.
7. Existing unexpected-mutation protections remain strict.
8. Existing normal CI remains green.
9. Unity EditMode/import validation is run only when required by changed Unity assets/code and must be green before merge.
10. Final runtime/presentation visual verification checks character fidelity, no sheet-text/border contamination, UI crop legibility, and animation-state continuity.
11. `/Gaudit` and `/graphRepair` run after implementation and before PR #5 merges.

## 13. Change isolation and merge safety

This work stays on PR #5's visual-remaster branch unless the implementation plan intentionally creates a fresh worktree/child branch from its current head for TDD isolation.

Do not merge PR #5 while this hero/class remaster is partially implemented. Do not weaken validators to make new assets pass. Do not regenerate unrelated monster/environment art.

Art replacement should preserve existing hero filenames and `.meta` GUIDs. Class/content changes should be narrowly scoped to the enum, class/ability data, hero identity catalog, selection/presentation plumbing, required simulation primitives, persistence compatibility, and their tests.

## 14. Definition of done

The remaster is done when the game presents nine visually faithful hero identities across eight real mechanical classes, Sir Clickington remains a distinct Knight identity with his unique campaign, all 81 runtime hero assets match the approved sheets without sheet UI contamination, old saves remain compatible, every class is selectable and mechanically valid, all relevant Unity and CI validation is green, and the final `/Gaudit` + `/graphRepair` + visual verification gates are clean before merge.