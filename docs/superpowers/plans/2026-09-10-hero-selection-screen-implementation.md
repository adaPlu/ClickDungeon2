# Hero Selection Screen Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the class-first selector with a one-hero-at-a-time reference-sheet-inspired selection screen using canonical stats and explicit SELECT/NEXT controls.

**Architecture:** Keep canonical mechanics in `GameContent` and identity/art lookup in the existing hero catalog/presentation layer. Extend `HeroCardPresentation` with selection ordering and canonical-stat descriptors that remain engine-free/testable, then have `MainMenuUI` render the selected page and delegate NEXT/SELECT behavior to that presentation contract.

**Tech Stack:** Unity 6000.5.9f1, C#, TextMeshPro/UGUI, NUnit EditMode tests, GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-10-hero-selection-screen-design.md`

## Global Constraints

- Exactly nine selectable identities, displayed one at a time.
- Exact selection order: `ironheart`, `clickington`, `windsong`, `shadowcut`, `emberwisp`, `lightbringer`, `rageclaw`, `gearspark`, `dawnward`.
- Bottom primary controls are `SELECT` and `NEXT`; NEXT wraps and never starts a run.
- SELECT starts the currently displayed hero in the current save slot.
- Use canonical `HeroDefinition.BaseHp`, `BaseAttack`, `BaseDefense`, `AbilityIds`, `Identity`, and `BoardPassive`; do not invent numerical stats.
- Sir Clickington remains a Knight identity with Knight mechanics and `clickington_campaign`.
- Prefer `hero.<id>.master` for the large showcase; identity-scoped fallbacks only.
- Do not generate or add artwork.
- Do not merge PR #5 or PR #6 during this task.

---

### Task 1: Lock the selector contract with RED tests

**Files:**
- Modify: `Assets/ClickDungeon/Tests/PresentationEditMode/HeroPresentationContractTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- Tests consume the existing `HeroCardPresentation` and source text of `MainMenuUI.cs`.
- Tests require a nine-id presentation order, canonical stat fields, and explicit bottom SELECT/NEXT wiring.

- [ ] **Step 1: Add failing presentation-contract tests**

Add tests that assert the exact nine-id order, require `HeroCardDescriptor` to expose `BaseHp`, `BaseAttack`, `BaseDefense`, `GameplayIdentity`, `BoardPassive`, and `AbilityIds`, and source-inspect `MainMenuUI.cs` to require `HeroSelectSelect`/`HeroSelectNext` while rejecting the old `PreviousClass`, `NextClass`, `PreviousHero`, and `NextHero` controls.

Use runtime reflection for the new descriptor properties so the RED phase is a test failure rather than a compile failure.

- [ ] **Step 2: Include the engine-free menu presentation file in CI**

Add:

```xml
<Compile Include="../../Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs" />
```

to the temporary engine-free presentation-contract project.

- [ ] **Step 3: Push the test-only change and observe RED**

Expected: `Presentation Contract Tests` fails because the current selector still has class/hero arrow controls and the descriptor lacks canonical stat data/selection order.

- [ ] **Step 4: Commit**

```bash
git add Assets/ClickDungeon/Tests/PresentationEditMode/HeroPresentationContractTests.cs .github/workflows/presentation-contract-tests.yml
git commit -m "test: lock single-page hero selector contract"
```

### Task 2: Implement the engine-free hero selection presentation contract

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs`

**Interfaces:**
- Produces `HeroCardPresentation.SelectionOrder` as the exact nine hero IDs.
- Produces `Describe(HeroIdentityDefinition hero, HeroDefinition mechanics)` with canonical stat/gameplay fields.
- Produces `SelectionHeroAt(int index)` and `WrapSelectionIndex(int index)` for deterministic UI navigation.

- [ ] **Step 1: Implement the minimal GREEN presentation model**

Keep the existing `Describe(HeroIdentityDefinition)` overload for existing menu consumers. Add a second overload that validates the mechanics class matches the identity class and copies only canonical values:

```csharp
BaseHp = mechanics.BaseHp;
BaseAttack = mechanics.BaseAttack;
BaseDefense = mechanics.BaseDefense;
GameplayIdentity = mechanics.Identity ?? string.Empty;
BoardPassive = mechanics.BoardPassive ?? string.Empty;
AbilityIds = mechanics.AbilityIds ?? Array.Empty<string>();
```

`SelectionHeroAt` scans `HeroIdentityCatalog.All` for the exact ID at the wrapped index and throws if the approved ID is missing.

- [ ] **Step 2: Push and verify presentation-contract GREEN**

Expected: reflection/order contract tests pass. The source-layout test may remain RED until Task 3; do not weaken it.

- [ ] **Step 3: Commit**

```bash
git add Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs
git commit -m "feat: add canonical hero selection presentation data"
```

### Task 3: Replace the existing selector UI with one full hero page

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`

**Interfaces:**
- Consumes `HeroCardPresentation.SelectionHeroAt`, `WrapSelectionIndex`, and `Describe(hero, _content.Hero(hero.ClassId))`.
- Produces `HeroSelectSelect` and `HeroSelectNext` buttons and one current hero page host.

- [ ] **Step 1: Replace class/identity indexes with one selection index**

Remove `_heroSelectClassIndex`, `_heroSelectHeroIndex`, `_heroSelectClassLabel`, `_heroSelectHeroLabel`, and class/identity cycle methods. Add `_heroSelectIndex` and `_heroSelectPageHost`.

- [ ] **Step 2: Build the approved page layout**

Use a near-full-screen framed panel. Render large master art on the left/center. In the upper-right, render canonical HP, Attack, Defense, and ability count/signature information. Replace the old animation region with a `CORE STATS` strip using only canonical values. Add `CLASS KIT` and `GAMEPLAY IDENTITY` sections populated from ability display names, identity, and board passive.

- [ ] **Step 3: Wire explicit bottom actions**

```csharp
CreateButton("HeroSelectSelect", panel, "SELECT", SelectCurrentHero, ...);
CreateButton("HeroSelectNext", panel, "NEXT  ▶", NextHeroSelection, ...);
```

`NextHeroSelection` increments/wraps and refreshes only. `SelectCurrentHero` calls `StartNew(CurrentHeroSelection().HeroId)`.

- [ ] **Step 4: Keep CLOSE non-primary**

Place CLOSE in the top-right or another non-bottom-primary location so the bottom pair stays SELECT/NEXT.

- [ ] **Step 5: Push and verify GREEN**

Expected: Presentation Contract Tests, Application Compile Tests, Simulation Compile/Tests, and the branch-specific Hero Remaster Unity EditMode workflow all pass on the same head.

- [ ] **Step 6: Commit**

```bash
git add Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs
git commit -m "feat: redesign hero selection as single-page carousel"
```

### Task 4: Adversarial verification

**Files:**
- No production changes unless verification identifies a real defect.

**Interfaces:**
- Confirms no class rebalance, no art generation, no selector action ambiguity, and no regression to Clickington identity semantics.

- [ ] **Step 1: Confirm exact diff scope**

Compare the final head to the pre-task head and verify the changes are limited to spec/plan, presentation tests/workflow inclusion, `HeroCardPresentation`, and `MainMenuUI`.

- [ ] **Step 2: Confirm CI on the exact final head**

Require all relevant workflow runs to complete successfully. Do not claim Unity validation unless the exact head's branch-specific Unity workflow is observed green.

- [ ] **Step 3: Leave PR #6 draft**

Do not merge. Report the final head and any remaining runtime visual/manual verification gate.
