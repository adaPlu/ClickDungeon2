# ClickDungeon2

Clean-room Unity/C# implementation of **ClickDungeon**.

## Product identity

**Read the dungeon. Reveal the danger. Risk the deeper path.**

ClickDungeon is a deterministic tactical tile-reveal roguelite built around a 5×5 board. Hidden tiles expose truthful but incomplete clues, monsters reshape spatial threat, classes read/manipulate the board differently, and scarce Big Keys force a choice between immediate vault rewards and more dangerous Forbidden Descents.

## Stack

- Unity 6.5 / **6000.5.9f1** (`b57deb96f08d`)
- URP 2D / URP 17.5
- C#
- deterministic discrete-action simulation
- canonical JSON content → validated generated runtime data
- Newtonsoft.Json versioned saves
- deterministic command recording/replay with final-state hashing
- GitHub Actions + GameCI v4
- PowerShell/Python validation and build tooling

## Current integrated scope

The `develop` branch contains the repaired graph integration:

- 50-floor / 10-biome campaign plus Endless Abyss
- Knight, Ranger, Thief and Wizard with 20 abilities
- 23 base monsters, deterministic biome variants and five bosses
- clues, spatial threat zones, traps, terrain, shrines, merchant, equipment/affixes and deterministic loot
- Safe Exit / Forbidden Descent / Sealed Vault Big-Key economy
- Trap Disarm Kit with targeted safe-disarm behavior and combat-turn reaction rules
- deterministic RNG, atomic saves, replay record/playback, state hashing and content migrations
- account / slot-meta / run-state ownership
- four save slots, class mastery and achievement persistence
- portrait + landscape runtime UI, canonical shop prices/stock, intent/status feedback and menu/game audio routing
- generated content and presentation databases
- editor tooling, animation slicing, content/asset validators, multi-policy balance harness and release gates

## Architecture rule

`ClickDungeon.Simulation` has no dependency on `UnityEngine`. Presentation submits commands and renders returned game events; it never owns gameplay outcomes.

Canonical gameplay/configuration data lives under `Assets/ClickDungeon/Content/Json`. Unknown hero-class, threat-pattern, and monster-intent values fail loading instead of silently coercing to fallback gameplay. Boss threat/intent data is canonical JSON rather than inferred from boss IDs.

## Automated verification status

Current engine-free evidence includes:

- Simulation assembly: real .NET/Roslyn compilation **PASS**
- Simulation tests: **40/40 PASS**, including 500-seed generation fuzzing, trap-kit economy/turn contracts and deterministic multi-policy balance cohorts
- Application assembly + persistence/replay layer: real .NET/Roslyn compilation **PASS**
- Application tests: **15/15 PASS**, including canonical JSON → complete 50-floor generation, save migration/recovery, replay record/playback/hash and future-version rejection
- canonical content validation: **PASS**
- replay contract validation: **PASS**
- static architecture/integration audit: **PASS (0 errors, 0 warnings)**

Unity-specific EditMode execution and player builds remain gated on GitHub Unity licensing secrets.

## Prototype presentation

Clean CI checkouts can generate deterministic DEVELOPMENT-only `_placeholder` runtime PNG/WAV media when binary assets are absent. This allows sprite slicing, animation generation, audio routing and player-build verification without weakening the commercial release gate. Existing runtime media is never overwritten.

Production release still requires final art/audio/store assets and provenance. Placeholder media is not considered shippable.

## Unity CI licensing

For Unity Personal, configure GitHub Actions repository secrets according to current GameCI guidance:

- `UNITY_LICENSE` — full contents of the locally activated Unity `.ulf` file
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Never commit or paste license files or credentials into chat. Unity Pro uses GameCI's serial-based configuration instead.

The build graph is:

`preflight → Unity EditMode → Windows → { Android AAB, WebGL }`

## Commercial release rule

Passing deterministic/unit/static CI does **not** mean the game is commercially ready. Release still requires licensed Unity import/player-build verification, stabilized Unity-generated `.meta`/ProjectSettings, production asset replacement/provenance, platform validation, human playtesting, balance review, store assets and the manual release gate.
