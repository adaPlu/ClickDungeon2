# Hero/Class Remaster Functional UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the approved title/gameplay references functional while preserving deterministic 5x5 simulation authority and existing hero/content architecture.

**Architecture:** Account-persistent menu systems live in `Application/State` plus small application services; `MainMenuUI` binds those services into real overlays and correct PLAY/CONTINUE routing. Gameplay presentation remains a projection of `RunState`; tile vocabulary is completed in presentation contracts first, with simulation-state additions only where a reference mechanic is genuinely absent.

**Tech Stack:** Unity 6000.5.9f1, C#, NUnit, Newtonsoft.Json, GitHub Actions engine-free .NET test harnesses plus Unity EditMode validation.

**Spec:** `docs/superpowers/specs/2026-09-10-hero-class-remaster-expanded-design.md`

## Global Constraints

- Title text is exactly `ClickDungeon`.
- Sir Clickington is hero id `clickington`, class `Knight`, campaign `clickington_campaign`.
- Board size remains exactly 5x5 / 25 interactive cells.
- PLAY never resumes; CONTINUE never starts a new run.
- Account currency is not `RunState.Gold`.
- Reward claim is idempotent for the same UTC calendar day.
- PR #6 stays draft/unmerged while any required CI/Unity gate is red.

---

### Task 1: Account currencies and daily reward domain

**Files:**
- Modify: `Assets/ClickDungeon/Application/State/AccountState.cs`
- Create: `Assets/ClickDungeon/Application/Services/DailyRewardService.cs`
- Test: `Assets/ClickDungeon/Tests/ApplicationEditMode/AccountMenuStateTests.cs`

**Interfaces:**
- Consumes: `AccountState` serialized by `AccountRepository`.
- Produces: `DailyRewardService.GetStatus(AccountState, DateTimeOffset)` and `TryClaim(AccountState, DateTimeOffset)` returning immutable reward status/result values.

- [ ] Write RED tests proving default balances are real state, first claim grants exactly one daily reward, a second same-day claim grants nothing, next-day claim advances streak, and JSON round-trip preserves balances/reward fields.
- [ ] Run Application Compile Tests and observe failures caused by missing fields/service.
- [ ] Add `GoldBalance`, `GemBalance`, `DailyRewardLastClaimUtc`, `DailyRewardStreak`, and reward service logic with UTC-date comparison only.
- [ ] Run focused application tests, then full Application Compile Tests.
- [ ] Commit `feat: add persistent menu rewards and currency`.

### Task 2: Separate PLAY and CONTINUE contracts

**Files:**
- Create: `Assets/ClickDungeon/Presentation/Menu/MainMenuRunRouting.cs`
- Modify: `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/MainMenuRunRoutingTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- Consumes: `SlotSavePayload.ActiveRun` and selected slot existence.
- Produces: `MainMenuRunRouting.CanContinue(SlotSavePayload)` and deterministic `PrimaryAction` values `NewRun` / `ContinueRun`.

- [ ] Write RED engine-free tests proving a metadata-only slot cannot Continue, a slot with `ActiveRun` can Continue, and PLAY always resolves to `NewRun`.
- [ ] Add the new test/contract files to the presentation engine-free workflow and observe RED.
- [ ] Implement routing helper; bind Continue button interactability/text to valid `ActiveRun`; bind bottom PLAY directly to Hero Select.
- [ ] Run presentation contract CI and Unity EditMode verification.
- [ ] Commit `feat: separate play and continue menu flows`.

### Task 3: Functional title overlays and settings

**Files:**
- Create: `Assets/ClickDungeon/Presentation/Menu/MenuOverlayFactory.cs`
- Modify: `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/MainMenuUiContractTests.cs`

**Interfaces:**
- Consumes: `AccountState`, `AccountRepository`, `ServiceRegistry`, selected hero metadata.
- Produces: named runtime overlays `InventoryOverlay`, `TalentsOverlay`, `SettingsOverlay`, `ShopOverlay` with real controls and close navigation.

- [ ] Write RED Unity tests asserting title/nav objects exist and Inventory/Talents/Settings/Shop actions activate corresponding overlays rather than only changing status text.
- [ ] Implement inventory summary from persistent/equipment state already available to the account/slot; implement mastery/talent view from selected slot metadata; implement interactive volume/haptics/reduced-motion/text-scale controls that save `AccountState`; retain existing store purchase path.
- [ ] Verify settings changes survive repository round-trip.
- [ ] Run Unity EditMode and application tests.
- [ ] Commit `feat: make title utility navigation functional`.

### Task 4: Daily reward title HUD integration

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/MainMenuUiContractTests.cs`

**Interfaces:**
- Consumes: Task 1 `DailyRewardService` and account balances.
- Produces: `CurrencyHud`, claim button state `CLAIM` / `CLAIMED`, reward note with next eligibility.

- [ ] Write RED Unity test for dynamic account balances and claim-state presentation.
- [ ] Bind claim button to `TryClaim`, immediately save account, refresh HUD/button/note.
- [ ] Run Unity EditMode and Application Compile Tests.
- [ ] Commit `feat: wire persistent daily reward menu`.

### Task 5: Gameplay reference layout without simulation authority drift

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs`
- Create: `Assets/ClickDungeon/Presentation/UI/GameplayHudPresentation.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/GameplayHudPresentationTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- Consumes: `RunState`, hero identity, biome display data.
- Produces: deterministic strings/IDs for hero HUD, resource HUD, action-bar labels; runtime layout remains 25 Button cells.

- [ ] Write RED engine-free tests for title branding/HUD projection and 25-cell board contract.
- [ ] Reshape landscape anchors into top HUD + centered board + bottom action/utility regions while preserving `OnTilePressed` command submission.
- [ ] Add Inventory/Talents/Shop utility navigation to gameplay presentation without moving rules into UI.
- [ ] Run presentation contract, simulation, and Unity EditMode tests.
- [ ] Commit `feat: remaster gameplay screen layout`.

### Task 6: Complete tile presentation vocabulary

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Assets/PresentationAssetId.cs`
- Modify: `Assets/ClickDungeon/Presentation/Assets/PresentationAssetIdMapper.cs`
- Modify: `Assets/ClickDungeon/Presentation/Assets/DungeonRoomPresentationLayout.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/PresentationAssetIdMapperTests.cs`

**Interfaces:**
- Produces canonical IDs for floor/structure/content/state variants named in the approved spec.

- [ ] Write RED tests enumerating every required tile identity and door/chest/stair state variant.
- [ ] Implement only missing presentation mappings for mechanics already represented by simulation state.
- [ ] Run engine-free presentation contracts and strict asset validators.
- [ ] Commit `feat: complete dungeon tile presentation vocabulary`.

### Task 7: Add genuinely missing deterministic tile mechanics

**Files:**
- Modify only simulation model/command/generation files proven necessary after Task 6 mapping audit.
- Test: `Assets/ClickDungeon/Tests/EditMode/*` and engine-free simulation tests.

**Interfaces:**
- New state must serialize inside existing `RunState`/`TileState` and be command-driven/replayable.

- [ ] For each reference mechanic with no existing equivalent, first write one RED deterministic simulation test proving state transition and replay stability.
- [ ] Implement the minimal state/command/generation change for that mechanic.
- [ ] Run all 62+ simulation tests after each mechanic; stop on regression.
- [ ] Commit each independently reviewable mechanic.

### Task 8: Starting monster presentation roster reconciliation

**Files:**
- Modify canonical content JSON and presentation mappings only where existing IDs cannot express: Goblin Brute King, Crowned Slime, Skeleton Warrior, Bat Swarm Leader, Mimic, Fire Imp, Armored Boar, Spellbook, Cave Spider, Theater Demon.
- Test: `Assets/ClickDungeon/Tests/ApplicationEditMode/CanonicalRuntimeContentTests.cs`

**Interfaces:**
- Produces a starting-floor presentation pool while retaining existing later-floor monster/boss catalog.

- [ ] Write RED tests that the starting roster identities resolve to canonical content + presentation IDs and that existing catalog entries remain present.
- [ ] Reconcile aliases/content entries without deleting later-floor roster.
- [ ] Run content validators, simulation tests, presentation contracts, and Unity metadata validation.
- [ ] Commit `feat: reconcile reference starting monster roster`.

### Task 9: Full verification and adversarial graph audit

**Files:**
- Update verification evidence only; no behavior changes unless a failing gate produces a new RED test first.

- [ ] Run Application Compile Tests, Presentation Contract Tests, Simulation Compile, Simulation Tests DotNet, strict content/asset/Unity-metadata validators, and Hero Remaster Unity EditMode workflow.
- [ ] Inspect CI logs for skipped tests, mutation evidence, compile warnings/errors, and false-green path omissions.
- [ ] Perform `/graphRepair`-style dependency audit from account persistence -> menu -> scene routing -> save/resume -> simulation -> presentation IDs -> runtime assets.
- [ ] Keep PR #6 draft/unmerged; report any remaining red gate with exact evidence.
