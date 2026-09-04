# ClickDungeon Visual Remaster Asset Manifest

Status legend: `KEEP`, `REWORK`, `REPLACE`, `ADD`, `LOCKED`, `INTEGRATED`, `TESTED`.

> Player-facing product name is **ClickDungeon**. The repository may remain `ClickDungeon2` internally, but no new player-facing asset or string may use sequel branding.

## Canonical repository baseline

Audited from `Assets/ClickDungeon/Content/Json` on the remaster branch.

| Category | Canonical count | Source | Remaster target |
|---|---:|---|---|
| Existing playable classes | 4 | `classes.json` | Preserve stable IDs; remaster identities/art |
| Existing class abilities | 20 | `abilities.json` | Preserve behavior; remaster icons/VFX |
| Base monsters | 23 | `monsters.json` | Preserve valid content; reconcile approved reference identities |
| Canonical bosses | 5 | `bosses.json` | Preserve; remaster presentation; add approved reference bosses only when not duplicates |
| Biomes | 10 | `biomes.json` | 10/10 distinct gameplay presentations |
| Canonical items | 8 | `items.json` | Preserve; replace placeholder imagery; equipment board is visual vocabulary, not automatic content expansion |

## Existing class identity lock

Stable technical IDs remain authoritative for saves, abilities, achievements and encounter logic.

| Stable class ID | Existing class | Approved hero identity | Player-facing class/archetype | Status |
|---|---|---|---|---|
| `class.knight` | Knight | **Ironheart** | Knight / Tank / Melee | LOCKED |
| `class.thief` | Thief | **Shadowcut** | Rogue / Thief / Agile Melee | LOCKED |
| `class.wizard` | Wizard | **Emberwisp** | Wizard / Mage / Magic Control | LOCKED |
| `class.ranger` | Ranger | **Windsong** | Ranger / Archer / Ranged Control | LOCKED |

Rules:
- Do not create duplicate classes named Ironheart, Shadowcut, Emberwisp or Windsong.
- These names are named hero identities layered over the stable existing class IDs.
- Existing saves must retain class ID, level, XP, equipment, mastery, unlocks and ability state.

### Existing hero production assets required

For each of Ironheart, Shadowcut, Emberwisp and Windsong:
- master/full-body art
- gameplay/chibi art
- portrait/HUD art
- roster icon
- class/select presentation
- locked/unavailable state where used
- equipment affinity presentation where used
- animation states required by runtime: idle, attack, hit, victory, defeat
- class-specific state where gameplay uses it (defend/block, dodge/burst, cast/special, ranged shot)

No hero is considered complete until the asset resolves through the presentation pipeline and the runtime visibly triggers its state changes.

## New playable classes

These are genuinely new gameplay classes and must be added only after the existing four-class remaster is stable.

| Proposed stable ID | Hero identity | Class | Role | Status |
|---|---|---|---|---|
| `class.cleric` | **Lightbringer** | Cleric | Healer / Support | ADD |
| `class.berserker` | **Rageclaw** | Berserker | Risk-reward melee damage | ADD |
| `class.engineer` | **Gearspark** | Engineer | Utility / devices / battlefield control | ADD |
| `class.paladin` | **Dawnward** | Paladin | Tank / support / holy defense | ADD |

Each new class requires gameplay definition, five-ability progression compatible with the existing content model, balance, unlock/mastery rules, save support, art, iconography, animation, VFX, UI integration and regression tests.

## Approved monster reference reconciliation

The uploaded monster boards are identity-locked visual references. They do not automatically imply new gameplay entities. Reconcile against the existing 23 monsters and 5 bosses first.

| Approved identity | Closest canonical content | Confidence | Initial classification | Notes |
|---|---|---:|---|---|
| Goblin Brute King | `monster.goblin` | medium | BOSS/ELITE CANDIDATE | Preserve goblin identity; no canonical goblin boss currently exists. Do not overwrite base goblin mechanics without explicit mapping. |
| Crowned Slime | `monster.slime` | high | VARIANT/BOSS CANDIDATE | Strong visual/base-species match; preserve base slime unless a new boss/elite definition is justified. |
| Skeleton Warrior | `monster.skeleton` | high | EXISTING MATCH | Remaster canonical skeleton presentation. |
| Bat Swarm Leader | `monster.bat` | high | VARIANT/ELITE CANDIDATE | Preserve base bat; leader/swarm presentation may be a variant or new encounter identity. |
| Mimic Chest | none | high | NEW CONTENT CANDIDATE | No current canonical mimic entry. |
| Fire Imp | `monster.demon` | low | NEW CONTENT CANDIDATE | Do not collapse into Demon automatically; silhouette/role differs. |
| Armored Boar | none | high | NEW CONTENT CANDIDATE | No current canonical boar entry. |
| Spooky Spellbook | none | high | NEW CONTENT CANDIDATE | No current canonical animated-book entry. |
| Cave Spider | `monster.spider` | high | EXISTING MATCH | Remaster canonical spider presentation. |
| Theater Curtain Demon | `monster.demon` / bosses | low | BOSS CANDIDATE | Preserve approved identity; do not replace canonical Archdemon Overlord unless mechanics/placement are intentionally reconciled. |

Unlisted canonical monsters are preserved by default. Deletion requires an explicit design reason and migration plan.

## Canonical bosses to preserve

| ID | Display name | Floor | Status |
|---|---|---:|---|
| `boss.lich_sovereign` | Lich Sovereign | 10 | KEEP + REWORK PRESENTATION |
| `boss.rootbound_leviathan` | Rootbound Leviathan | 20 | KEEP + REWORK PRESENTATION |
| `boss.frostbog_colossus` | Frostbog Colossus | 30 | KEEP + REWORK PRESENTATION |
| `boss.archdemon_overlord` | Archdemon Overlord | 40 | KEEP + REWORK PRESENTATION |
| `boss.primal_ancient_wyrm` | Primal Ancient Wyrm | 50 | KEEP + REWORK PRESENTATION |

Reference-board bosses may be added or mapped only after duplication analysis.

## Ten biome gameplay presentations

All ten canonical biome IDs remain. Each must visibly alter actual tactical gameplay, not only a splash/background image.

| Biome ID | Display name | Floors | Visual integration target | Status |
|---|---|---|---|---|
| `biome.cavern` | Cavern | 1-5 | stone, warm torchlight, dust/tracks | REWORK |
| `biome.crypt` | Crypt | 6-10 | grave stonework, cold shadow, curse motifs | REWORK |
| `biome.sunken_temple` | Sunken Temple | 11-15 | wet stone, reflective water, ripples | REWORK |
| `biome.thorn_wilds` | Thorn Wilds | 16-20 | roots, vines, thorn routes | REWORK |
| `biome.mire` | Mire | 21-25 | poison pools, bubbles, fumes | REWORK |
| `biome.frozen_ruins` | Frozen Ruins | 26-30 | frost, ice cracks, cold haze | REWORK |
| `biome.storm_plateau` | Storm Plateau | 31-35 | charged tiles, lightning/static | REWORK |
| `biome.lava_field` | Lava Field | 36-40 | heat, embers, burning cells | REWORK |
| `biome.arcane_nexus` | Arcane Nexus | 41-45 | runes, purple distortion, teleport motifs | REWORK |
| `biome.ash_wastes` | Ash Wastes | 46-50 | ash currents, red glow, elite/endgame treatment | REWORK |

Existing `Assets/ClickDungeon/Art/Runtime/biome_*_master.png` files are inputs to the production pipeline. Completion requires the biome identity to be visible in the 5x5 dungeon board through floor/wall treatment, props, lighting, ambient VFX and hazard presentation while retaining tile readability.

## Canonical item baseline

| ID | Display name | Type | Rarity | Status |
|---|---|---|---|---|
| `item.healing_potion` | Healing Potion | consumable | common | REPLACE ART |
| `item.trap_disarm_kit` | Trap Disarm Kit | consumable | common | REPLACE ART |
| `item.rusty_sword` | Rusty Sword | weapon | common | REPLACE ART |
| `item.iron_sword` | Iron Sword | weapon | uncommon | REPLACE ART |
| `item.steel_blade` | Steel Blade | weapon | rare | REPLACE ART |
| `item.leather_armor` | Leather Armor | armor | common | REPLACE ART |
| `item.chainmail` | Chainmail | armor | uncommon | REPLACE ART |
| `item.plate_mail` | Plate Mail | armor | rare | REPLACE ART |

The uploaded equipment board defines the visual language and future equipment vocabulary. It is not permission to silently add every illustrated item to gameplay.

## Player-facing UI/presentation audit targets

Required review/remaster coverage:
- boot/loading/branding
- main menu
- four save slots / continue/load presentation
- class/hero selection
- tactical 5x5 gameplay HUD
- inventory/equipment
- merchant/shop
- progression/mastery/abilities
- boss presentation
- victory/defeat
- rewards/chest reveal
- pause/settings
- achievements
- dialogs/tooltips/confirmations
- loading/error/empty states that actually exist in the application

Visual target: approved premium dark-fantasy cartoon language, gold/orange bevels, navy/charcoal/purple surfaces, high-readability mobile composition, expressive comedy through character reaction rather than meme text.

## Animation/VFX completion rules

Hero baseline: idle, attack, hit, victory, defeat + gameplay-required class special.

Monster baseline: spawn, idle, attack, hit, defeat. Bosses additionally require entrance/taunt, special/phase presentation when supported, and distinctive defeat.

Animation is presentation only: simulation state and deterministic combat timing remain authoritative.

VFX families required where gameplay uses them: melee impact, magic impact, projectile, critical, shield/heal/buff/debuff, trap, pickup/unlock, chest/reward, boss action, floor clear, victory/defeat and biome ambience/hazards.

## Placeholder / provenance gate

Search and eliminate player-facing:
- temp/placeholder images
- emoji as final icons
- generic avatars
- tiny low-resolution stand-ins
- inconsistent old class/monster/item art
- broken asset references
- visible `ClickDungeon2` / sequel branding

Each shipped binary production asset must satisfy the repository's provenance/hash requirements where applicable.

## Required completion report

Do not mark this remaster complete until the repository can report and verify:

- Classes: **8 / 8 playable**
- Existing hero identity mappings: **4 / 4**
- New classes: **4 / 4**
- Hero production art: **8 / 8**
- Hero animation sets: **8 / 8**
- Canonical base monsters reconciled: **23 / 23**
- Canonical bosses preserved/remastered: **5 / 5**
- Approved reference monsters/bosses accounted for: **10 / 10**
- Monster/boss required animation sets: **all active runtime entities**
- Biome gameplay presentations: **10 / 10**
- Canonical items visually remastered: **8 / 8**
- Reachable player-facing screens reviewed: **all**
- Player-facing placeholder assets remaining: **0**
- Visible sequel branding remaining: **0**
- Save/regression suite: **green**
- Windows runtime smoke: **green**
- Android APK: **green**
- Android AAB signer verification: **green**
- WebGL: **green**

Only after these gates pass may `develop` be merged to `main` for the final release-candidate CI pass.
