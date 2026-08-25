# ClickDungeon2 Audit And GraphRepair Handoff

Generated on 2026-08-24 for worktree `C:\Users\plugu\Documents\ChatGPT\clickdungeon`, branch `develop`, baseline `c3d67067caf95fff317954408ec207f6ebf17983`.

Use `/audit` for read-only discovery and `/graphRepair` for fixes. Do not use `/remediate` terminology for this project.

## Architecture Map

| Component | Responsibility | Inputs | Outputs | Dependencies | Trust Boundaries |
| --- | --- | --- | --- | --- | --- |
| `Assets/ClickDungeon/Simulation` | Deterministic rules, commands, generation, combat, loot, status, terrain, boss logic | `GameCommand`, `RunState`, canonical `GameContent` | `CommandResult`, `GameEvent`, mutated `RunState` | No `UnityEngine`; simulation content/model helpers | Presentation command API -> simulation state |
| `Assets/ClickDungeon/Application` | Content loading, generated databases, saves, migrations, account state, replay, services | JSON content, slot/account files, replay envelopes | save docs, account docs, replay strings/results | Newtonsoft.Json, Unity persistence path where compiled in Unity | Local files and WebGL persistence -> durable state |
| `Assets/ClickDungeon/Presentation` | Unity boot/menu/runtime UI/audio | User UI actions, generated resources, services | Commands, UI state, audio feedback, saves/replays | UnityEngine, Application and Simulation assemblies | User input -> command execution |
| `Assets/Plugins/WebGL` | Browser persistence bridge | C# extern call | `FS.syncfs` request | Unity WebGL runtime FS | Unity memory FS -> IndexedDB/browser storage |
| `Assets/ClickDungeon/Content/Json` | Canonical gameplay definitions | Authored JSON | Runtime content catalog | Python validators, content loader | Authored data -> deterministic runtime rules |
| `Assets/ClickDungeon/Editor` | Unity asset generation, project settings, build automation | Unity editor import/build | generated resources, player builds | Unity editor APIs | Editor code -> CI with Unity credentials |
| `scripts` | Static, content, replay, asset, metadata, release validation | Source tree and generated assets | pass/fail validation | Python 3 | Source tree -> CI/release gates |
| `.github/workflows` | CI/build/release automation | GitHub events, secrets, source checkout | test/build/release statuses and artifacts | GitHub Actions, GameCI, Unity secrets | Untrusted PR/content -> CI and secrets |

## Audit Graph

A0 Architecture map -> A1/A2/A3/A4

A1 Simulation/content determinism audit -> A5

A2 Persistence/replay/platform audit -> A5

A3 Unity editor/build/CI/release audit -> A5

A4 Test and validation strength audit -> A5

A5 Cross-component reconciliation -> A6

A6 Durable handoff -> `/graphRepair`

## Consolidated Findings

### REL-01: Statuses tick in the same command that applies them

- Severity: Medium
- Confidence: CONFIRMED
- Category: Reliability / gameplay correctness
- Component: Simulation status and command execution
- Files: `Assets/ClickDungeon/Simulation/GameSession.cs`, `Assets/ClickDungeon/Simulation/Status/StatusResolver.cs`, `Assets/ClickDungeon/Simulation/Combat/MonsterIntentResolver.cs`
- Execution path: `AttackCommand` hits a surviving poison monster -> monster response applies `status.poison` -> `GameSession.Apply` immediately calls `StatusResolver.AdvanceMeaningfulAction` for that same command. Trap reveal can apply burn/poison/root and then immediately tick it too.
- Impact: newly applied poison/burn/root durations are shortened immediately, and damage statuses can tick on the same action that created them.
- Recommended fix: make status advancement apply only to statuses that existed before command resolution, or otherwise mark new statuses to skip the current action.
- Recommended test: session-level poison/burn/root application through `GameSession.Apply`, asserting no same-command tick.

### REL-02: Camouflage threat bypass is unbounded for movement

- Severity: Medium
- Confidence: CONFIRMED
- Category: Reliability / gameplay correctness
- Component: Simulation movement and status/action counters
- Files: `Assets/ClickDungeon/Simulation/GameSession.cs`, `Assets/ClickDungeon/Simulation/Status/StatusResolver.cs`, `Assets/ClickDungeon/Content/Json/abilities.json`
- Execution path: camouflage sets `State.CamouflageActions`; movement uses it to bypass threatened-cell rejection; `MoveCommand` is not a meaningful action and never decrements the counter.
- Impact: unlimited threatened moves while camouflage remains active.
- Recommended fix: consume camouflage on movement when it is used to bypass threat, or make movement a meaningful action if that is the intended rule.
- Recommended test: with `CamouflageActions = 1`, move into one threatened tile, then assert a second threatened move is rejected.

### REL-03: Rejected rooted moves mutate state

- Severity: Medium
- Confidence: CONFIRMED
- Category: Reliability / gameplay correctness
- Component: Simulation movement/status
- Files: `Assets/ClickDungeon/Simulation/GameSession.cs`, `Assets/ClickDungeon/Simulation/Status/StatusResolver.cs`
- Execution path: rooted `MoveCommand` decrements `RootedActions` and consumes `status.root`, then returns `Reject("player.rooted")`; rejected commands do not increment command number.
- Impact: players can clear root by spamming rejected movement without consuming an accepted action; replay/save policy diverges for rejected mutations.
- Recommended fix: make rejected root moves side-effect-free or accept them as a consumed action. Prefer side-effect-free rejection unless design says root consumes the attempted move.
- Recommended test: rooted move rejection leaves root state unchanged.

### REL-04: Self-targeted line/mobility abilities can consume charges with no useful effect

- Severity: Low
- Confidence: CONFIRMED
- Category: Reliability / input validation
- Component: Ability resolver
- Files: `Assets/ClickDungeon/Simulation/Abilities/AbilityResolver.cs`
- Execution path: `eagle_eye` on the player tile computes zero direction and identifies the same tile repeatedly; `shadowstep` on the current tile passes distance and occupancy checks.
- Impact: accepted no-op commands spend scarce charges.
- Recommended fix: reject self-targets for these abilities.
- Recommended test: self-target `eagle_eye` and `shadowstep` return `ability.invalid_target` with unchanged charges.

### DATA-01: Development fallback status timing diverges from canonical JSON

- Severity: Medium
- Confidence: CONFIRMED
- Category: Data integrity / test fidelity
- Component: Fallback content
- Files: `Assets/ClickDungeon/Simulation/Content/GameContent.cs`, `Assets/ClickDungeon/Content/Json/statuses.json`
- Execution path: tests/default generator often use `GameContent.CreateDevelopmentFallback`; fallback defines `status.root` as `meaningful_action`; canonical JSON defines `status.root` as `enemy_response`.
- Impact: fallback-backed tests can validate status lifetimes that differ from runtime canonical content.
- Recommended fix: align fallback status definitions with canonical JSON or add an explicit equivalence test.
- Recommended test: fallback and canonical shared status IDs have matching timing/effect/stack semantics.

### DATA-02: Corrupted-slot continue can silently overwrite the slot with a fresh run

- Severity: High
- Confidence: CONFIRMED
- Category: Data integrity
- Component: Save loading and presentation bootstrap
- Files: `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`, `Assets/ClickDungeon/Presentation/GameBootstrap.cs`, `Assets/ClickDungeon/Application/Persistence/LocalSaveRepository.cs`
- Execution path: menu continue checks `SlotExists`; `GameBootstrap.TryLoadExisting` catches load failure and returns false; `Awake` creates and saves a fresh run into that slot.
- Impact: corrupted/unreadable saves can be overwritten rather than surfaced for recovery.
- Recommended fix: distinguish absent save from corrupt save. Only create a fresh run when no save exists; block or mark recovery required when load fails.
- Recommended test: corrupt primary and backup slot, simulate continue/load, assert no overwrite.

### DATA-03: Corrupt account state resets and can overwrite achievements/settings

- Severity: High
- Confidence: CONFIRMED
- Category: Data integrity
- Component: Account persistence
- Files: `Assets/ClickDungeon/Application/Persistence/AccountRepository.cs`, `Assets/ClickDungeon/Presentation/GameBootstrap.cs`
- Execution path: account primary and backup unreadable -> `Load` returns new `AccountState`; subsequent run/save overwrites default account data.
- Impact: achievements, totals, victories/deaths, and settings can be silently lost.
- Recommended fix: distinguish missing account from corrupt account. Raise recovery failure on corrupt primary+backup rather than defaulting.
- Recommended test: corrupt account primary and backup; assert load fails with recovery error and save does not overwrite implicitly.

### REL-05: WebGL persistence sync failures are fire-and-forget

- Severity: Medium
- Confidence: CONFIRMED
- Category: Reliability / durability
- Component: WebGL persistence
- Files: `Assets/ClickDungeon/Application/Platform/PersistentDataSync.cs`, `Assets/Plugins/WebGL/ClickDungeonPersistence.jslib`, persistence repositories
- Execution path: save succeeds in Unity FS memory -> JS `FS.syncfs(false, callback)` logs callback error only.
- Impact: browser quota/private-mode/page-lifecycle failures can lose data while UI proceeds as if durable.
- Recommended fix: route async sync result back into C# or expose a pending/failed persistence state.
- Recommended test: WebGL shim forces sync failure and UI/repository observes failure.

### CI-01: Active `develop` integration path skips strong validation

- Severity: Medium
- Confidence: CONFIRMED
- Category: CI reliability
- Component: GitHub Actions
- Files: `.github/workflows/validate-remote.yml`, `.github/workflows/simulation-tests-dotnet.yml`, `.github/workflows/application-compile-tests.yml`, `.github/workflows/static-validation.yml`
- Execution path: push to `develop` runs only `validate-remote`; stronger .NET/static workflows run on PRs to `main` or manual dispatch.
- Impact: regressions can land on the active integration branch despite `develop` being documented as verified.
- Recommended fix: trigger strong validation workflows on `develop` push/PR or expand `validate-remote` to include replay/static/.NET tests.
- Recommended check: CI topology lint or workflow update.

### SEC-01: PR Unity jobs expose Unity secrets to PR-controlled project code

- Severity: High
- Confidence: CONFIRMED
- Category: CI security
- Component: GitHub Actions / Unity CI
- Files: `.github/workflows/unity-platform-ci.yml`
- Execution path: `pull_request` to `main` checks out PR code then runs GameCI with Unity secrets in environment; Unity imports and executes project/editor code.
- Impact: same-repo PR authors or compromised editor code can attempt credential exfiltration.
- Recommended fix: move secret-bearing Unity jobs to trusted refs only, environment-approved manual dispatch, or post-merge protected branch execution.
- Recommended check: workflow policy forbids `secrets.UNITY_*` in `pull_request` jobs.

### SEC-02: GitHub Actions are tag-pinned, not commit-pinned

- Severity: Medium
- Confidence: CONFIRMED
- Category: Supply chain
- Component: GitHub Actions
- Files: `.github/workflows/*.yml`
- Execution path: workflows use `actions/checkout@v4`, `actions/setup-python@v5`, `actions/setup-dotnet@v4`, `game-ci/*@v4`, and `actions/upload-artifact@v4`.
- Impact: mutable tags expand supply-chain blast radius, especially where credentials are available.
- Recommended fix: pin external actions to reviewed full commit SHAs and add a workflow linter.
- Recommended check: static workflow lint rejecting non-SHA `uses:`.

### REL-06: Release readiness is not tied to current build artifacts or Android signing

- Severity: Medium
- Confidence: CONFIRMED
- Category: Release reliability
- Component: Release workflow
- Files: `.github/workflows/release-gate.yml`, `scripts/release-check.py`, editor build settings
- Execution path: release gate runs validators but does not build/download/inspect artifacts for the same SHA or verify Android signing.
- Impact: source can pass release gate without store-uploadable artifacts.
- Recommended fix: release workflow requires successful Unity CI for the same SHA, downloads artifacts, inspects checksums, and verifies Android signing.

### REL-07: CI accepts dirty Unity-generated imports/builds

- Severity: Medium
- Confidence: CONFIRMED
- Category: Build reproducibility
- Component: Unity CI and metadata
- Files: `.github/workflows/unity-platform-ci.yml`, `scripts/validate-unity-metadata.py`
- Execution path: metadata audit runs advisory; Unity builders set `allowDirtyBuild: true`; dirty status is uploaded but not enforced.
- Impact: CI can pass from generated/uncommitted state.
- Recommended fix: after canonical metadata exists, run strict metadata and fail on dirty `Assets`, `ProjectSettings`, or `Packages`.

### SEC-03: Unity package resolution is not locked

- Severity: Medium
- Confidence: CONFIRMED
- Category: Supply chain / reproducibility
- Component: Unity packages
- Files: `Packages/manifest.json`
- Execution path: no `Packages/packages-lock.json`; GameCI resolves packages dynamically.
- Impact: dependency drift and registry resolution nondeterminism.
- Recommended fix: generate and commit `Packages/packages-lock.json` from Unity and fail CI if absent/changed.

### SEC-04: Asset provenance validator permits unmanifested runtime media

- Severity: Medium
- Confidence: CONFIRMED
- Category: Supply chain / asset licensing
- Component: Asset validation
- Files: `scripts/validate-assets.py`, `Assets/ClickDungeon/Editor/PresentationAssetGenerator.cs`
- Execution path: validator checks manifest rows but not extra runtime files; generator scans runtime folders.
- Impact: unmanifested media can ship if dropped into runtime folders.
- Recommended fix: reject build-consumable runtime files absent from manifests and placeholder-named runtime files for release.
- Recommended test: fixture with unmanifested PNG/WAV fails validation.

### REL-08: `release-check.py` omits replay and static audit despite being documented as complete

- Severity: Medium
- Confidence: CONFIRMED
- Category: Release reliability
- Component: Release gate
- Files: `scripts/release-check.py`, `README.md`
- Execution path: `release-check.py` runs content/assets/metadata/manifests but not `validate-replay.py` or `static-audit.py`.
- Impact: users following the single complete release gate can miss replay/static regressions.
- Recommended fix: call replay and static validators from `release-check.py`.

### TEST-01: Replay tests mostly validate codec shape, not command execution

- Severity: Medium
- Confidence: CONFIRMED
- Category: Test gap
- Component: Replay tests
- Files: `Assets/ClickDungeon/Tests/ApplicationEditMode/ReplayTests.cs`, `Assets/ClickDungeon/Application/Replay/ReplayRunner.cs`
- Execution path: command codec round-trips all commands, but playback test applies only one reveal.
- Impact: decoded command behavior can break while tests pass.
- Recommended fix: add table/scenario replay tests for command families and accepted/rejected policy.

### TEST-02: Ability behavior is under-tested

- Severity: Medium
- Confidence: CONFIRMED
- Category: Test gap
- Component: Ability tests
- Files: `Assets/ClickDungeon/Simulation/Abilities/AbilityResolver.cs`, `FEATURE_LIST.md`
- Execution path: 20 switch cases exist; tests cover charge banking and replay references, not behavior matrix.
- Impact: ability validation and side effects can regress unnoticed.
- Recommended fix: parameterized tests for each ability.

### TEST-03: Threat, monster intent, and terrain coverage is incomplete

- Severity: Medium
- Confidence: CONFIRMED
- Category: Test gap
- Component: Simulation tests
- Files: threat/intent/terrain tests and resolvers
- Execution path: current tests cover adjacent threat, one regen path, and thorn first-entry damage.
- Impact: other threat patterns, intents, and terrain effects can regress.
- Recommended fix: add matrix tests for all threat patterns, monster intents, and terrain types.

### TEST-04: Save checksum tests lack valid-JSON tamper coverage

- Severity: Medium
- Confidence: CONFIRMED
- Category: Test gap / data integrity
- Component: Persistence tests
- Files: `Assets/ClickDungeon/Tests/ApplicationEditMode/PersistenceTests.cs`, `Assets/ClickDungeon/Application/Persistence/LocalSaveRepository.cs`
- Execution path: round-trip uses same code path; corrupt-primary test uses malformed JSON.
- Impact: checksum validation could weaken while tests pass.
- Recommended fix: edit valid payload without checksum update and assert primary rejection/backup fallback or load failure.

### TEST-05: Account repository lacks direct tests

- Severity: Medium
- Confidence: CONFIRMED
- Category: Test gap / data integrity
- Component: Account persistence
- Files: `Assets/ClickDungeon/Application/Persistence/AccountRepository.cs`, tests
- Execution path: account repository owns progression but tests only cover slot saves.
- Impact: account fallback/default behavior can regress independently.
- Recommended fix: account round-trip, corrupt-primary backup fallback, corrupt-both failure/default policy tests.

### MAINT-01: Static validators are token-presence checks without fixture tests

- Severity: Low
- Confidence: CONFIRMED
- Category: Maintainability / test gap
- Component: Python validators
- Files: `scripts/static-audit.py`, `scripts/validate-replay.py`
- Execution path: validators search for strings that can remain in comments/dead code.
- Impact: false-green validator results.
- Recommended fix: add fixture tests or more structural parsing where practical.

## /graphRepair Graph

G1 Simulation command semantics: `REL-01`, `REL-02`, `REL-03`, `REL-04`, `DATA-01`, `TEST-01` partial, `TEST-02` partial, `TEST-03` partial.

G2 Save/account recovery semantics: `DATA-02`, `DATA-03`, `TEST-04`, `TEST-05`.

G3 Release/CI validators: `CI-01`, `REL-08`, `SEC-04`, `MAINT-01` partial.

G4 Blocked release hardening: `SEC-01`, `SEC-02`, `REL-05`, `REL-06`, `REL-07`, `SEC-03`, production asset/metadata blockers.

Run order: G1 -> G2 -> G3. G4 remains unresolved until Unity metadata, package lock, production assets, reviewed action SHAs, or a workflow trust decision are available.

## /graphRepair Results

Completed in this session:

- G1 fixed `REL-01`, `REL-02`, `REL-03`, `REL-04`, and `DATA-01`, including separate `meaningful_action` and `floor_action` status advancement.
- G1 added focused tests that partially close `TEST-01`, `TEST-02`, and `TEST-03` for the repaired paths.
- G2 fixed `DATA-02` and `DATA-03`.
- G2 added focused tests that close `TEST-04` and partially close `TEST-05`.
- G3 fixed `CI-01`, `REL-08`, and `SEC-04`.
- G3 partially improved `MAINT-01` by making `static-audit.py` enforce the expanded release-check contract and adding a fixture test for asset-validator negative cases.

Blocked or intentionally unresolved:

- `REL-05` needs a WebGL async persistence callback design and Unity/WebGL validation.
- `SEC-01` needs a workflow trust-policy decision for secret-bearing Unity jobs.
- `SEC-02` needs reviewed immutable SHA pins for third-party actions.
- `REL-06` needs a release artifact/signing process decision.
- `REL-07` needs Unity-generated canonical `.meta` files and ProjectSettings before dirty-build enforcement can be disabled.
- `SEC-03` needs a Unity-generated `Packages/packages-lock.json`.
- Production asset/media blockers require final art/audio/store assets and manifests.

Validation run after GraphRepair:

- `python scripts\validate-content.py`: PASS
- `python scripts\validate-replay.py`: PASS
- `python scripts\static-audit.py`: PASS, `0 errors, 0 warnings`
- `python -m compileall -q scripts`: PASS
- `python scripts\test-validators.py`: PASS
- .NET simulation harness: PASS, `50/50`
- .NET application harness: PASS, `19/19`
- `python scripts\validate-assets.py`: expected BLOCKED, missing production art/audio manifests and runtime media
- `python scripts\validate-unity-metadata.py`: advisory WARNING, missing 156 Unity `.meta` sidecars and essential ProjectSettings
- `python scripts\release-check.py`: expected BLOCKED, now includes content, replay, static, asset, and strict metadata checks

Independent review:

- First repair review approved `REL-02`, `REL-04`, `DATA-02`, `DATA-03`, `CI-01`, and `REL-08`; it requested follow-up changes for `REL-01`, `REL-03`, `DATA-01`, and `SEC-04`.
- Follow-up repairs addressed those requested changes.
- Second-pass review approved `REL-01`, `REL-03`, and `SEC-04`; after the final status timing split it approved `DATA-01` with no remaining findings.

## Verification Matrix

| ID | Status | Evidence Level | Notes |
| --- | --- | --- | --- |
| REL-01 | Fixed | VERIFIED | New statuses are skipped by same-command meaningful-action advancement; covered by session poison test. |
| REL-02 | Fixed | VERIFIED | Camouflage decrements when it actually bypasses threatened movement; covered by movement test. |
| REL-03 | Fixed | VERIFIED | Rooted movement is an accepted consumed blocked action only for otherwise valid movement; covered by valid and invalid rooted movement tests. |
| REL-04 | Fixed | VERIFIED | Self-targeted `eagle_eye` and `shadowstep` reject before charge consumption; covered by ability test. |
| DATA-01 | Fixed | VERIFIED | Development fallback `status.root` and `status.curse` timing now match canonical content, and `floor_action` advancement is distinct from `meaningful_action`; covered by resolver and session tests. |
| DATA-02 | Fixed | VERIFIED | Continue path now treats existing corrupt slot as recovery failure rather than absent save; repository corrupt-both coverage added. |
| DATA-03 | Fixed | VERIFIED | Account repository throws on corrupt primary+backup instead of defaulting; account fallback and corrupt-both tests added. |
| REL-05 | Blocked | CONFIRMED | Requires WebGL async callback design/Unity runtime validation. |
| CI-01 | Fixed | VERIFIED | Strong static/simulation/application workflows now trigger for `develop`; remote validator also runs replay validation. |
| SEC-01 | Blocked | CONFIRMED | Requires workflow trust policy decision. |
| SEC-02 | Blocked | CONFIRMED | Requires reviewed action SHA pins. |
| REL-06 | Blocked | CONFIRMED | Requires artifact/signing release process. |
| REL-07 | Blocked | CONFIRMED | Requires Unity-generated canonical metadata. |
| SEC-03 | Blocked | CONFIRMED | Requires Unity-generated package lock. |
| SEC-04 | Fixed | VERIFIED | Asset validator now rejects runtime files missing manifest coverage and placeholder runtime filenames. |
| REL-08 | Fixed | VERIFIED | `release-check.py` now runs replay contract validation and static architecture audit. |
| TEST-01 | Partially fixed | VERIFIED | Added session-level status/replay-policy-adjacent tests; full replay command behavioral matrix remains open. |
| TEST-02 | Partially fixed | VERIFIED | Added ability self-target charge-preservation tests; full 20-ability behavior matrix remains open. |
| TEST-03 | Partially fixed | VERIFIED | Added camouflage/root/status timing coverage; full threat/intent/terrain matrix remains open. |
| TEST-04 | Fixed | VERIFIED | Added valid-JSON tamper checksum fallback test. |
| TEST-05 | Partially fixed | VERIFIED | Added account fallback and corrupt-both tests; broader account progression tests remain open. |
| MAINT-01 | Partially fixed | VERIFIED | Static audit now checks release gate includes replay/static, and `scripts/test-validators.py` covers the asset validator's recursive unmanifested-file failure path. |
