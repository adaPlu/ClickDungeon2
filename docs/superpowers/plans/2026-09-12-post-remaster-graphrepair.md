# Post-Remaster GraphRepair Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Repair the verified post-remaster release graph without altering gameplay semantics or touching `main` directly.

**Architecture:** Work only on `graphrepair/post-remaster-audit-repair`, based on verified `develop`. Treat branding as the only production-code bug in this pass; treat the obsolete workflow, CI trigger, and stale PRs as repository-topology cleanup. Preserve historical branches until explicit containment proofs are complete.

**Tech Stack:** Unity 6000.5.9f1, C#, NUnit, GitHub Actions.

**Spec:** `Docs/gaudit-post-remaster-2026-09-12.md`

## Global Constraints

- Player-facing product name is `ClickDungeon`; `ClickDungeon2` is repository/internal history only.
- No direct writes to `main`.
- No merge across a red CI gate.
- Do not weaken `unity-platform-ci.yml`.
- Do not delete historical branches without containment proof.

---

### Task 1: Lock player-facing gameplay branding

**Files:**
- Modify: `Assets/ClickDungeon/Tests/PresentationEditMode/RuntimeGameUIHierarchyTests.cs`
- Modify: `Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs`

**Interfaces:**
- Consumes: `RuntimeGameUI.BuildUi()` and the generated `TopHud/Brand` hierarchy.
- Produces: regression coverage requiring the gameplay brand label text to equal `ClickDungeon`.

- [ ] **Step 1: Write the failing test**

Extend `BuildsApprovedGameplayHierarchyWithTwentyFiveBoardCells()` to fetch `safe.Find("TopHud/Brand")`, require a `TMPro.TMP_Text` component, and assert `text == "ClickDungeon"`.

- [ ] **Step 2: Run the focused presentation verification and observe RED**

Use the repository's presentation contract/Unity EditMode CI path on the repair branch. Expected failure before production change: brand text is `ClickDungeon2`.

- [ ] **Step 3: Write the minimal implementation**

Change only the gameplay brand creation line in `RuntimeGameUI.BuildTopHud()` from `ClickDungeon2` to `ClickDungeon`.

- [ ] **Step 4: Run focused verification and observe GREEN**

Require the branding regression plus existing presentation contracts to pass.

### Task 2: Remove dead post-remaster CI scaffolding

**Files:**
- Delete: `.github/workflows/hero-remaster-unity-validation.yml`
- Delete: `ci-trigger.txt`

**Interfaces:**
- Consumes: active release verification from `.github/workflows/unity-platform-ci.yml` and the standard PR checks.
- Produces: a release tree without obsolete feature-only CI or historical trigger files.

- [ ] **Step 1: Verify the obsolete workflow trigger**

Confirm `hero-remaster-unity-validation.yml` only targets `feature/hero-class-remaster-execution` plus manual dispatch.

- [ ] **Step 2: Delete the obsolete workflow**

Remove only `.github/workflows/hero-remaster-unity-validation.yml`; do not edit `unity-platform-ci.yml`.

- [ ] **Step 3: Delete the historical trigger file**

Remove only `ci-trigger.txt`.

- [ ] **Step 4: Verify branch CI through PR**

Open a PR from `graphrepair/post-remaster-audit-repair` to `develop` and require all applicable checks to complete successfully before merge.

### Task 3: Retire superseded PR topology

**Files:** none.

**Interfaces:**
- Consumes: content-level reconciliation evidence for PR #4 and PR #8.
- Produces: closed, unmerged historical PRs with explicit superseded rationale.

- [ ] **Step 1: Add closure note to PR #4**

State that current `develop` already contains/supersedes its monster presentation contract and broader CI coverage; do not merge the diverged branch.

- [ ] **Step 2: Close PR #4 without merge**

- [ ] **Step 3: Add closure note to PR #8**

State that current `develop` already contains the special-tile, layered-presentation, gameplay-layout, and Clickington story behavior while preserving the newer expanded hero catalog; do not merge the older branch.

- [ ] **Step 4: Close PR #8 without merge**

### Task 4: Finish and hand off branch-containment/admin debt

**Files:** none.

- [ ] **Step 1: Keep historical source branches intact**

Do not delete them in this pass.

- [ ] **Step 2: Record operational blocker**

`main` and `develop` remain unprotected until repository-admin rulesets/branch protection can be enabled.

- [ ] **Step 3: After repair PR is green, merge only into `develop`**

Promote `develop -> main` separately only after fresh `develop` verification.
