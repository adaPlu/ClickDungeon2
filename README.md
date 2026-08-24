# ClickDungeon2

Clean-room Unity/C# implementation of **ClickDungeon**.

## Product identity

**Read the dungeon. Reveal the danger. Risk the deeper path.**

ClickDungeon is a deterministic tactical tile-reveal roguelite built around a 5×5 board. Hidden tiles expose truthful but incomplete clues, monsters reshape spatial threat, classes read/manipulate the board differently, and scarce Big Keys force a choice between immediate vault rewards and more dangerous Forbidden Descents.

## Stack

- Unity 6.5 / **6000.5.9f1** (`b57deb96f08d`)
- URP 2D
- C#
- deterministic discrete-action simulation
- canonical JSON content → validated generated runtime data
- Newtonsoft.Json versioned saves
- GitHub Actions + GameCI scaffolding
- PowerShell/Python validation and build tooling

## Current integrated scope

The local `develop` branch contains the repaired graph integration rather than only the original vertical-slice scaffold:

- 50-floor / 10-biome campaign plus Endless Abyss
- Knight, Ranger, Thief and Wizard with 20 abilities
- 23 base monsters, deterministic biome variants and five bosses
- clues, spatial threat zones, traps, terrain, shrines, merchant, equipment/affixes and deterministic loot
- Safe Exit / Forbidden Descent / Sealed Vault Big-Key economy
- deterministic RNG, saves, replay contracts and content migrations
- account / slot-meta / run-state ownership
- four save slots, class mastery and achievement persistence
- portrait + landscape runtime UI, canonical names, intent/status feedback and menu/game audio routing
- generated content and presentation databases
- editor tooling, animation slicing, content/asset validators, balance harness and release gates
- placeholder production footprint for 4 heroes, 23 monster sheets, 5 boss sheets, 10 biome masters, object art, store art and 37 audio files

## Architecture rule

`ClickDungeon.Simulation` has no dependency on `UnityEngine`. Presentation submits commands and renders returned game events; it never owns gameplay outcomes.

Canonical gameplay/configuration data lives under `Assets/ClickDungeon/Content/Json`. `scripts/static-audit.py` fails when a canonical JSON document is not wired into the runtime loader.

## Verification status

Locally verified in the current execution environment:

- canonical content validation: **PASS**
- art/audio structural + semantic coverage validation: **PASS**
- static architecture/integration audit: **PASS (0 errors, 0 warnings)**
- Python tooling compilation: **PASS**
- Git diff/conflict/whitespace check: **PASS**

Expected blockers:

- `scripts/release-check.py` intentionally blocks release while procedural/store placeholders remain.
- Unity batch compilation, EditMode tests and player builds require an installed Unity 6000.5.9f1 editor. Run `python scripts/verify-unity.py --build windows` (or another target) with `UNITY_PATH` set.
- The first verified Unity import must generate and stabilize Unity `.meta` files and the broader editor-generated `ProjectSettings` set; commit that Unity-generated diff after it passes verification.
- Production Apple/Google purchase adapters still require platform SDK/account integration; the authoritative one-time full-game entitlement boundary is implemented with a local development adapter.

See `docs/INTEGRATION_AUDIT_2026-08-23.md` and `docs/BUILD_RELEASE.md` for the exact release/verification contract.
