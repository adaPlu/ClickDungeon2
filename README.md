# ClickDungeon2

Clean-room Unity/C# implementation of **ClickDungeon**.

> Read the dungeon. Reveal the danger. Risk the deeper path.

ClickDungeon2 is a deterministic tactical tile-reveal roguelite built around a compact 5x5 board. Hidden tiles expose truthful but incomplete clues, monsters reshape spatial threat, hero classes read and manipulate the board differently, and scarce Big Keys force a choice between immediate vault rewards and more dangerous Forbidden Descents.

This repository is currently developed on `develop` and mirrored to:

`https://github.com/adaPlu/ClickDungeon2.git`

## Current Status

`develop` is a verified vertical-slice implementation of the core simulation, persistence, replay, canonical content, presentation bootstrap, and CI validation graph. The latest pre-README baseline verified for this documentation was:

- Commit: `d3aef4053b40e63c14ea7bdb83ce0442fc79c37d`
- Message: `fix: qualify Unity Application in editor validator`
- Branch: `develop`

The project is not yet commercially release-ready. It still needs licensed Unity player-build verification, final production art/audio/store assets, asset provenance review, platform validation, balance review, and manual release approval.

## Technology Stack

- Unity 6.5 / `6000.5.9f1` (`b57deb96f08d`)
- C# with Unity assembly definitions
- Universal Render Pipeline 2D / URP `17.5.0`
- Unity UI (`com.unity.ugui` `2.0.0`)
- Unity Test Framework `1.7.0`
- Newtonsoft.Json via `com.unity.nuget.newtonsoft-json` `3.2.2`
- Engine-free deterministic simulation designed for .NET/Roslyn validation
- Canonical JSON content with generated Unity runtime assets
- Versioned save files and deterministic replay envelopes
- GitHub Actions, GameCI v4, Python validation tooling, and .NET 8 CI harnesses

## Gameplay Scope

The integrated scope on `develop` includes:

- 50-floor campaign across 10 biomes
- Endless Abyss mode after campaign progression
- Knight, Ranger, Thief, and Wizard hero classes
- 20 class abilities with mastery unlocks
- 23 base monsters, deterministic biome variants, and five bosses
- Clues, spatial threat zones, traps, terrain, shrines, merchant encounters, equipment, affixes, and deterministic loot
- Safe Exit, Forbidden Descent, and Sealed Vault routing
- Big-Key economy with a two-key carry cap
- Trap Disarm Kit behavior with targeted safe-disarm rules and combat-turn reaction contracts
- Class mastery and achievements
- Four save slots with account-level and slot-level state ownership
- Portrait and landscape runtime UI
- Menu/game audio routing and event-driven presentation feedback
- Deterministic replay recording, playback, compatibility checks, and final-state hashing

For the full implementation-grounded feature inventory, see [FEATURE_LIST.md](FEATURE_LIST.md).

## Architecture

The codebase is split into three main runtime layers:

- `Assets/ClickDungeon/Simulation`
  - Pure deterministic gameplay logic.
  - No `UnityEngine` dependency.
  - Owns board state, commands, combat, loot, abilities, bosses, terrain, traps, threats, generation, balance evaluation, and model types.

- `Assets/ClickDungeon/Application`
  - Runtime services around the simulation.
  - Owns canonical content loading, generated content database access, saves, migrations, account state, slot state, replay encoding/running/storage, version contracts, and platform persistence sync.

- `Assets/ClickDungeon/Presentation`
  - Unity-facing bootstrapping and UI.
  - Submits commands to the simulation and renders returned command results and game events.
  - Does not own gameplay outcomes.

The core rule is:

`ClickDungeon.Simulation` must stay engine-free and deterministic. Presentation submits commands; simulation returns outcomes.

## Repository Layout

```text
Assets/
  ClickDungeon/
    Application/          Save, replay, content loading, versioning, services
    Content/Json/         Canonical gameplay/content definitions
    Editor/               Unity generation, validation, build, and bootstrap tooling
    Presentation/         Boot loaders, runtime UI, audio routing, presentation assets
    Simulation/           Engine-free deterministic game rules
    Tests/
      EditMode/           Simulation-focused NUnit tests
      ApplicationEditMode/ Persistence/replay/content integration tests
  Plugins/WebGL/          WebGL persistent-data bridge
Packages/                 Unity package manifest
ProjectSettings/          Unity editor version pin and project settings
scripts/                  Python validation and release-gate scripts
.github/workflows/        Static, .NET, Unity, diagnostic, and release workflows
ci/                       CI readiness marker files
```

## Canonical Content

Canonical gameplay data lives in:

`Assets/ClickDungeon/Content/Json`

Important files include:

- `classes.json`
- `abilities.json`
- `monsters.json`
- `bosses.json`
- `biomes.json`
- `floor_archetypes.json`
- `items.json`
- `affixes.json`
- `statuses.json`
- `balance.json`
- `loot_tables.json`
- `shops.json`
- `monster_variants.json`
- `traps.json`
- `progression.json`
- `achievements.json`
- `content_migrations.json`

The content loader intentionally fails on unsupported hero classes, threat patterns, monster intents, missing references, invalid charge/recharge values, invalid status/trap relationships, duplicate IDs, and incomplete progression contracts. Boss threat and intent data is canonical JSON rather than inferred from boss IDs.

## Runtime Version Contracts

Version constants are defined in:

`Assets/ClickDungeon/Application/Versioning/GameVersionInfo.cs`

Current values:

- Game version: `0.2.0`
- Save schema version: `2`
- Simulation version: `2`
- Content revision: `2`

Save documents and replay envelopes carry simulation/content version metadata. Future-version saves are rejected, compatible old content IDs can migrate through `content_migrations.json`, and replay playback validates compatibility before running the command stream.

## Persistence And Replay

Persistence is intentionally split by responsibility:

- `AccountRepository` owns account-wide progress.
- `LocalSaveRepository` owns slot save documents and recovery behavior.
- `SlotMetaState` stores per-slot metadata.
- `SlotSavePayload` stores active run state.
- `SaveMigrator` upgrades compatible save data.
- `PersistentDataSync` flushes WebGL saves through the JavaScript bridge in `Assets/Plugins/WebGL/ClickDungeonPersistence.jslib`.

Replay support includes:

- Command encoding and decoding in `ReplayCommandCodec`
- Compact replay serialization in `ReplayCodec`
- Replay envelopes with root seed, class, mode, campaign limit, unlocked abilities, command stream, and final hash
- Recording through `ReplayRecorder`
- Deterministic playback through `ReplayRunner`
- Last-replay storage with `.tmp` and `.bak` recovery handling in `ReplayRepository`
- Final-state hashing through `StateHasher`

Active run state must be mutated only through deterministic commands, not through presentation or meta-progression side effects.

## Local Development

Required tools:

- Unity Editor `6000.5.9f1`
- Python 3.12 or compatible Python 3
- .NET 8 SDK for the engine-free CI-equivalent harnesses
- Git

Clone and switch to the development branch:

```bash
git clone https://github.com/adaPlu/ClickDungeon2.git
cd ClickDungeon2
git switch develop
```

Open the project folder in Unity Hub with Unity `6000.5.9f1`.

Unity-generated folders such as `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, and `UserSettings/` are intentionally ignored.

## Validation Commands

Run static and content validation from the repository root:

```bash
python scripts/validate-content.py
python scripts/validate-replay.py
python scripts/static-audit.py
python scripts/validate-unity-metadata.py
```

Compile the Python tooling:

```bash
python -m compileall -q scripts
```

Run Unity verification when Unity is installed and discoverable:

```bash
python scripts/verify-unity.py
```

Optional Unity player-build checks:

```bash
python scripts/verify-unity.py --build windows
python scripts/verify-unity.py --build android
python scripts/verify-unity.py --build web
python scripts/verify-unity.py --build ios
```

Inspect generated player artifacts after Unity builds:

```bash
python scripts/inspect-build-artifact.py windows Builds/Windows
python scripts/inspect-build-artifact.py android Builds/Android
python scripts/inspect-build-artifact.py webgl Builds/Web
```

If Unity is not on `PATH`, set `UNITY_PATH` to the Unity `6000.5.9f1` editor executable.

## Automated Verification Status

Current engine-free evidence on `develop` includes:

- Simulation assembly real .NET/Roslyn compilation: **PASS**
- Simulation tests: **40/40 PASS**
- Generation fuzzing: **500 seeds PASS**
- Trap-kit economy and turn-contract tests: **PASS**
- Multi-policy balance cohorts: **PASS**
- Application assembly plus persistence/replay layer real .NET/Roslyn compilation: **PASS**
- Application tests: **15/15 PASS**
- Canonical JSON to complete 50-floor generation: **PASS**
- Save migration, recovery, replay record/playback/hash, and future-version rejection: **PASS**
- Canonical content validation: **PASS**
- Replay contract validation: **PASS**
- Static architecture/integration audit: **PASS (0 errors, 0 warnings)**

Unity-specific EditMode execution and player builds require GitHub Unity licensing secrets or a local licensed Unity installation.

## GitHub Actions

Workflow files live in `.github/workflows`.

Primary validation lanes:

- `static-validation.yml`
  - Compiles Python scripts.
  - Runs canonical content validation.
  - Runs replay contract validation.
  - Runs static architecture audit.
  - Verifies the Unity editor pin.

- `simulation-compile.yml`
  - Builds the engine-free simulation assembly with .NET 8.

- `simulation-tests-dotnet.yml`
  - Runs the engine-free simulation NUnit test suite through a generated .NET 8 test project.

- `application-compile-tests.yml`
  - Compiles Simulation + Application with minimal Unity stubs.
  - Runs persistence, replay, migration, and canonical runtime content tests.

- `unity-platform-ci.yml`
  - Preflights source and Unity license secrets.
  - Audits Unity metadata reproducibility in advisory mode.
  - Runs Unity EditMode tests through GameCI.
  - Captures Unity import metadata evidence.
  - Builds Windows.
  - Builds Android AAB after Windows.
  - Builds WebGL after Windows.
  - Inspects Windows, Android, and WebGL build artifact structure.

- `release-gate.yml`
  - Manual release-readiness workflow.
  - Runs content validation, static audit, asset validation, metadata validation, and the complete release check.

Most automated workflows currently target pull requests into `main` and can also run through `workflow_dispatch`.

## Unity CI Licensing

For Unity Personal with GameCI v4, configure these GitHub repository secrets:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

`UNITY_LICENSE` should contain the full contents of the locally activated Unity `.ulf` file. Do not commit Unity license files or paste credentials into issues, pull requests, commits, or chat.

Unity Pro should use GameCI's serial-based configuration with `UNITY_SERIAL` plus `UNITY_EMAIL` and `UNITY_PASSWORD`.

The intended licensed build graph is:

`preflight -> Unity EditMode -> Windows -> Android AAB + WebGL`

## Assets And Placeholder Policy

Clean CI checkouts can generate deterministic development-only `_placeholder` PNG/WAV media when binary runtime assets are absent. This keeps sprite slicing, animation generation, audio routing, and player-build verification testable before production media lands.

Important rules:

- Placeholder media is acceptable for development verification.
- Existing runtime media should not be overwritten by placeholder generation.
- Commercial release requires final art/audio/store assets.
- Production assets need provenance, usage terms, and SHA-256 manifest coverage.
- Placeholder media is never considered shippable.

Asset validation is handled by:

```bash
python scripts/validate-assets.py
```

The complete release gate is handled by:

```bash
python scripts/release-check.py
```

During vertical-slice development, the release gate may intentionally block until production assets, store materials, strict Unity metadata reproducibility, and clean-build policy are complete.

## Build Targets

Editor build automation lives in:

`Assets/ClickDungeon/Editor/BuildAutomation.cs`

Configured targets include:

- Windows player
- Android App Bundle
- WebGL build
- iOS export

Build automation applies project/player settings, ensures TextMeshPro resources, generates content assets, generates presentation assets, scaffolds core scenes, saves assets, and forces required import updates before building.

Current player/version contracts include:

- Application identifier: `com.adaplu.clickdungeon`
- Android minimum API level: 23
- Android target API level: 36
- Android architectures: ARMv7 and ARM64
- iOS target OS: 13.0
- Default orientation: Portrait

## Branch Strategy

- `develop` is the active integration branch.
- `main` is the protected/release-facing branch.
- Pull requests into `main` are expected to pass static, simulation, application, and licensed Unity gates where applicable.
- Feature work should branch from `develop`.
- Release promotion should happen from a verified `develop` state into `main`.

Suggested local workflow:

```bash
git switch develop
git pull --ff-only origin develop
git switch -c feature/short-description
```

## Release Checklist

Before treating a build as release-ready:

- Confirm `python scripts/validate-content.py` passes.
- Confirm `python scripts/validate-replay.py` passes.
- Confirm `python scripts/static-audit.py` passes with zero errors.
- Confirm Unity EditMode tests pass in the pinned Unity editor.
- Confirm Windows, Android AAB, and WebGL builds pass through licensed Unity CI.
- Replace development placeholder art/audio with production assets.
- Verify asset manifests include SHA-256 hashes, usage terms, and prompt/source references.
- Run `python scripts/validate-assets.py`.
- Run `python scripts/validate-unity-metadata.py --strict`.
- Run `python scripts/release-check.py`.
- Confirm Unity Platform CI no longer requires `allowDirtyBuild: true`.
- Complete manual playtesting for campaign, Endless Abyss, save/load, replay, vault, Forbidden Descent, trap kit, shops, bosses, and class mastery.
- Review balance and economy progression.
- Validate platform-specific save behavior, especially WebGL IndexedDB persistence.
- Prepare store assets and remove placeholder store materials.

## Troubleshooting

If Unity verification reports no editor executable:

```bash
set UNITY_PATH=C:\Path\To\Unity.exe
python scripts/verify-unity.py
```

If static validation fails:

- Check canonical JSON references first.
- Confirm every ID referenced by content exists in the target content file.
- Confirm simulation files do not reference `UnityEngine`, `System.Random`, frame time, or wall-clock time.
- Confirm generated resource names still match runtime `Resources.Load` calls.

If Unity metadata validation fails:

- Open the project with Unity `6000.5.9f1`.
- Let Unity create canonical `.meta` files and ProjectSettings.
- Review the generated metadata before committing it.
- Do not hand-generate Unity GUIDs.

If replay validation fails:

- Check command codec coverage for every `GameCommand` type.
- Confirm replay envelopes still include root seed, hero class, mode, campaign floor limit, unlocked abilities, command stream, versions, and final hash.
- Confirm active run-state mutation occurs through command execution.

If WebGL saves do not persist:

- Confirm `PersistentDataSync` still imports `ClickDungeonSyncPersistentData`.
- Confirm `ClickDungeonPersistence.jslib` still exports `ClickDungeonSyncPersistentData`.
- Confirm the JavaScript bridge still calls `FS.syncfs(false, ...)`.

## Commercial Release Rule

Passing deterministic/unit/static CI does **not** mean the game is commercially ready. Release still requires licensed Unity import/player-build verification, stabilized Unity-generated metadata and project settings, production asset replacement and provenance, platform validation, human playtesting, balance review, store assets, and the manual release gate.
