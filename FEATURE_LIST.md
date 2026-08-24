# ClickDungeon2 Feature List

This document tracks the feature surface currently represented on `develop`. It is an implementation-grounded list, not a store description or future roadmap.

## Status Legend

- **Implemented**: Code and canonical content exist in the repository.
- **Verified**: Covered by current content validation, static audit, .NET compilation/tests, or application tests.
- **Unity-gated**: Requires licensed Unity import, EditMode execution, or player-build verification.
- **Release-gated**: Requires production assets, metadata, platform checks, store materials, or manual approval before shipping.

## Core Game Loop

| Feature | Status | Notes |
| --- | --- | --- |
| 5x5 tactical tile board | Implemented / Verified | Board size is canonicalized in `balance.json`; commands resolve against tile indices and deterministic board state. |
| Hidden tile reveal flow | Implemented / Verified | Tiles support hidden, clued, identified, and revealed visibility states. |
| Truthful clue families | Implemented / Verified | Clue families include danger, opportunity, and passage/arcane clues. |
| Discrete command execution | Implemented / Verified | All gameplay changes flow through `GameCommand` and `GameSession.Apply`. |
| Deterministic RNG | Implemented / Verified | Uses `XorShift32` and semantic seed derivation instead of frame time or platform randomness. |
| Turn-based monster reactions | Implemented / Verified | Monster intent and threat logic runs as deterministic simulation behavior. |
| Command rejection reasons | Implemented / Verified | Invalid actions return structured `CommandResult` rejections instead of silently mutating state. |

## Modes And Routes

| Feature | Status | Notes |
| --- | --- | --- |
| Campaign mode | Implemented / Verified | 50-floor campaign target with deterministic content generation. |
| Endless Abyss mode | Implemented / Verified | Post-campaign endless mode with abyss depth progression. |
| Standard route | Implemented / Verified | Normal floor progression path. |
| Safe Exit | Implemented / Verified | Explicit `TakeSafeExitCommand`. |
| Forbidden Descent | Implemented / Verified | Explicit `TakeForbiddenExitCommand`; uses higher-risk route modifiers. |
| Sealed Vault | Implemented / Verified | Explicit `UnlockVaultCommand`; consumes Big-Key economy. |
| Prestige depth contract | Implemented / Verified | Progression validation requires prestige depth beyond campaign length. |

## Player Classes And Abilities

| Feature | Status | Notes |
| --- | --- | --- |
| Knight | Implemented / Verified | Defensive/control identity with shield-oriented board passive. |
| Ranger | Implemented / Verified | Distance/control identity with extra useful floor-start clue. |
| Thief | Implemented / Verified | Information/control identity with trap identification behavior. |
| Wizard | Implemented / Verified | Board-control identity with area ability behavior. |
| 20 class abilities | Implemented / Verified | Five abilities per class in canonical content. |
| Ability charges | Implemented / Verified | Ability definitions require positive max charges and recharge values. |
| Mastery unlock thresholds | Implemented / Verified | Content validation enforces monotonic unlock thresholds. |
| Ability command replay | Implemented / Verified | `UseAbilityCommand` is encoded and decoded for replay. |

## Board Content

| Feature | Status | Notes |
| --- | --- | --- |
| Gold tiles | Implemented / Verified | Supported by tile content kind and loot/economy flow. |
| Consumables | Implemented / Verified | Supported by `UseItemCommand` and canonical item definitions. |
| Equipment | Implemented / Verified | Supported by item instances, affixes, and `EquipItemCommand`. |
| Shrines | Implemented / Verified | Shrine choices include max HP, attack, and defense. |
| Chests | Implemented / Verified | Supported as tile content and deterministic reward surfaces. |
| Traps | Implemented / Verified | Five canonical traps with status/duration validation. |
| Monsters | Implemented / Verified | 23 base monsters in canonical content. |
| Bosses | Implemented / Verified | Five canonical bosses with explicit floor, threat, and intent data. |
| Small Keys | Implemented / Verified | Supported tile content for key economy. |
| Big Keys | Implemented / Verified | Carry cap is canonicalized at two keys. |
| Merchant | Implemented / Verified | One canonical shop with validated stock references. |
| Special events | Implemented | Supported tile content kind; concrete event use should remain content/test driven. |

## Combat And Threats

| Feature | Status | Notes |
| --- | --- | --- |
| Player attack resolution | Implemented / Verified | `AttackCommand` resolves through simulation combat rules. |
| Defend action | Implemented / Verified | `DefendCommand` participates in command/replay contracts. |
| Incoming damage mitigation | Implemented / Verified | Damage resolver applies defense/status effects through simulation state. |
| Threatened-cell detection | Implemented / Verified | `ThreatResolver` evaluates monster threat coverage. |
| Adjacent threat pattern | Implemented / Verified | Canonical threat pattern. |
| Cross-two threat pattern | Implemented / Verified | Canonical threat pattern. |
| Orthogonal-line threat pattern | Implemented / Verified | Canonical threat pattern. |
| Aura-two threat pattern | Implemented / Verified | Canonical threat pattern. |
| Monster attack intent | Implemented / Verified | Canonical intent. |
| Heavy attack intent | Implemented / Verified | Canonical intent. |
| Steal-gold intent | Implemented / Verified | Canonical intent. |
| Poison intent | Implemented / Verified | Canonical intent. |
| Guard intent | Implemented / Verified | Canonical intent. |
| Summon intent | Implemented / Verified | Canonical intent. |
| Hazard intent | Implemented / Verified | Canonical intent. |
| Boss phase reactions | Implemented / Verified | Boss tests cover phase behavior after damage. |

## Biomes And Terrain

| Feature | Status | Notes |
| --- | --- | --- |
| 10 canonical biomes | Implemented / Verified | Biome IDs are validated and referenced by monsters/floors. |
| Eight floor archetypes | Implemented / Verified | Archetypes drive floor composition. |
| Normal terrain | Implemented / Verified | Baseline tile terrain. |
| Grave terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Flooded terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Thorn terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Mire terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Ice terrain | Implemented / Verified | Includes slide-target behavior. |
| Charged terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Lava terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Arcane terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Ash terrain | Implemented / Verified | Terrain kind supported by simulation. |
| Biome variants | Implemented / Verified | Six monster variant definitions are canonicalized. |

## Items, Loot, Shops, And Economy

| Feature | Status | Notes |
| --- | --- | --- |
| Eight canonical items | Implemented / Verified | Items are referenced by shops, loot, equipment, and consumable flows. |
| Five affixes | Implemented / Verified | Equipment affixes are canonical content. |
| Two loot tables | Implemented / Verified | Loot entries validate positive weights and valid item/currency references. |
| Merchant shop | Implemented / Verified | `BuyItemCommand` references merchant tile and item ID. |
| Shop stock validation | Implemented / Verified | Content validation rejects missing item references. |
| Trap Disarm Kit | Implemented / Verified | Covered by economy and combat-turn contract tests. |
| Big-Key carry cap | Implemented / Verified | Canonical cap is two keys. |
| Forbidden route rewards | Implemented / Verified | Balance data includes higher risk/reward route modifiers. |
| Equipment instances | Implemented / Verified | Item instance state supports deterministic equipment ownership. |

## Status Effects And Achievements

| Feature | Status | Notes |
| --- | --- | --- |
| Seven canonical statuses | Implemented / Verified | Status definitions require effect, duration, and max stacks. |
| Status add/refresh | Implemented / Verified | Managed by `StatusResolver`. |
| Status duration advancement | Implemented / Verified | Meaningful actions advance status state. |
| Status consumption/removal | Implemented / Verified | Resolver supports stack consumption and explicit removal. |
| Five achievements | Implemented / Verified | Achievement content requires supported trigger types and display names. |
| Campaign-completion achievement trigger | Implemented / Verified | Supported trigger family. |
| Vault-opened achievement trigger | Implemented / Verified | Supported trigger family. |
| Forbidden-floor achievement trigger | Implemented / Verified | Supported trigger family. |
| Abyss-depth achievement trigger | Implemented / Verified | Supported trigger family. |

## Saves, Slots, And Progression

| Feature | Status | Notes |
| --- | --- | --- |
| Four save slots | Implemented / Verified | Runtime menu/game bootstrap supports multi-slot flow. |
| Account-level progression | Implemented / Verified | `AccountState` and `AccountRepository` own account progress. |
| Slot metadata | Implemented / Verified | `SlotMetaState` separates menu-visible slot state from active run payload. |
| Active run save payload | Implemented / Verified | `SlotSavePayload` stores deterministic run data. |
| Atomic save writes | Implemented / Verified | Repository uses temp/backup recovery behavior. |
| Save checksums | Implemented / Verified | `ChecksumUtility` supplies SHA-256 checksum support. |
| Save schema versioning | Implemented / Verified | Current save schema version is `2`. |
| Save migration | Implemented / Verified | Migration service applies compatible content ID migrations. |
| Future-version rejection | Implemented / Verified | Application tests cover incompatible future saves. |
| WebGL persistence flush | Implemented / Verified | C# extern and `.jslib` bridge call `FS.syncfs(false, ...)`. |

## Replay And Determinism

| Feature | Status | Notes |
| --- | --- | --- |
| Replay envelope | Implemented / Verified | Captures root seed, class, mode, campaign limit, unlocked abilities, command stream, final hash, and version metadata. |
| Replay command codec | Implemented / Verified | Covers reveal, move, interact, attack, defend, ability, item, shrine, buy, equip, safe exit, forbidden exit, and vault commands. |
| Replay compression/encoding | Implemented / Verified | Uses GZip and Base64 encoding. |
| Compatibility validation | Implemented / Verified | Rejects incompatible simulation/content revisions. |
| Replay recording | Implemented / Verified | Records command stream and hashes final state. |
| Replay playback | Implemented / Verified | Reconstructs deterministic session and checks for divergence. |
| Last replay repository | Implemented / Verified | Saves last replay with recovery files. |
| Final-state hashing | Implemented / Verified | `StateHasher` validates deterministic end state. |

## Presentation And UX

| Feature | Status | Notes |
| --- | --- | --- |
| Boot loader | Implemented / Unity-gated | Unity entry point exists; full behavior needs Unity runtime verification. |
| Main menu UI | Implemented / Unity-gated | Supports slots, class selection, progression display, and run entry. |
| Runtime game UI | Implemented / Unity-gated | Supports board rendering, inventory/equipment, intent/status feedback, and command dispatch. |
| Portrait orientation contract | Implemented / Unity-gated | Player settings target portrait as the default. |
| Landscape support | Implemented / Unity-gated | Runtime UI is intended to support portrait and landscape layouts. |
| Generated content database | Implemented / Unity-gated | Editor generator creates `ClickDungeonGeneratedContent.asset`. |
| Generated presentation database | Implemented / Unity-gated | Editor generator creates `ClickDungeonPresentationAssets.asset`. |
| Music and ambience controller | Implemented / Unity-gated | Routes menu/exploration/combat/boss/victory/defeat music categories. |
| Game event audio routing | Implemented / Unity-gated | Presentation maps simulation events to audio cues. |
| Development placeholder assets | Implemented / Release-gated | Generated placeholders support CI/development but are not shippable. |

## Platform And Services

| Feature | Status | Notes |
| --- | --- | --- |
| Analytics interface | Implemented | `IAnalytics` contract and null implementation exist. |
| Remote config interface | Implemented | Local fallback implementation exists. |
| Ads interface | Implemented | Null implementation exists; production service is not wired. |
| Store entitlement interface | Implemented | Local full-game entitlement store exists; production platform billing is not wired. |
| Cloud save interface | Implemented | Null implementation exists; production cloud save is not wired. |
| Platform capability interface | Implemented | Desktop, mobile, web, haptics, and export capabilities are abstracted. |
| Windows build automation | Implemented / Unity-gated | GameCI and local verifier target Windows builds. |
| Android AAB build automation | Implemented / Unity-gated | Build target exists; production signing/store workflow remains release work. |
| WebGL build automation | Implemented / Unity-gated | Includes persistent-data sync bridge. |
| iOS export automation | Implemented / Unity-gated | Export target exists; Xcode/App Store pipeline is not completed here. |

## Editor Tooling And CI

| Feature | Status | Notes |
| --- | --- | --- |
| Unity project version pin | Implemented / Verified | `ProjectVersion.txt` pins Unity `6000.5.9f1`. |
| Build settings automation | Implemented / Verified | Applies package/player target settings before builds. |
| Scene scaffolding | Implemented / Unity-gated | Editor tooling ensures core scenes. |
| TextMeshPro bootstrap | Implemented / Unity-gated | Editor tooling ensures TMP resources. |
| Content asset generation | Implemented / Unity-gated | Converts canonical JSON into runtime resources. |
| Presentation asset generation | Implemented / Unity-gated | Creates runtime presentation asset database. |
| Pixel asset import tooling | Implemented / Unity-gated | Supports imported runtime art pipeline. |
| Animation clip generation | Implemented / Unity-gated | Supports generated animation assets. |
| Content validation script | Implemented / Verified | `scripts/validate-content.py`. |
| Replay contract validation script | Implemented / Verified | `scripts/validate-replay.py`. |
| Static architecture audit | Implemented / Verified | `scripts/static-audit.py`. |
| Unity metadata audit | Implemented / Release-gated | Advisory in CI; strict in release gate. |
| Build artifact inspection | Implemented / Unity-gated | Validates Windows, Android, and WebGL output structure. |
| Release readiness gate | Implemented / Release-gated | Aggregates release blockers and intentionally fails until production gates are complete. |

## Current Release Blockers

- Canonical Unity `.meta` sidecars and essential ProjectSettings need to be generated by Unity `6000.5.9f1`, reviewed, and committed.
- Unity Platform CI still allows dirty builds during metadata stabilization.
- Licensed Unity EditMode and player-build evidence must be produced through GameCI or a local licensed Unity editor.
- Production art, audio, asset manifests, usage terms, and SHA-256 provenance must replace development placeholder media.
- Store assets must be completed without placeholder files.
- Platform-specific validation is still required for Windows, Android, WebGL, and iOS export targets.
- Production billing, ads, analytics, remote config, and cloud save providers are represented by service interfaces/local null implementations rather than final platform integrations.
- Manual playtesting, balance review, economy review, and release approval remain required.

## Out Of Scope For The Current Implementation

- Live backend services.
- Multiplayer or online leaderboards.
- Production cloud save provider.
- Production ad mediation.
- Production app-store billing integration.
- Final platform signing, store submission, and storefront metadata.
- Final commercial art/audio/store asset pack.
