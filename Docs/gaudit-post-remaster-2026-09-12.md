# ClickDungeon post-remaster /Gaudit — 2026-09-12

Baseline audited:
- `develop@1e46c342c8315d37f5b3786b95f0f893e64d432f`
- `main@c0683afcb8d24c291d12f91efdc31334b8bd3bdf`
- Shared release tree: `627194f41591d7866fc7a4df265544b24e0f3e3d`

## Release verification

- PR #5 visual-remaster integration is merged into `develop`.
- PR #11 promoted `develop` to `main` with no content drift.
- Fresh `develop` PR checks completed successfully on the integration SHA.
- Fresh `main` Unity Platform CI run `34666939675` completed successfully, including EditMode, Windows, WebGL, Android, and runtime smoke gates.

## Findings

### G1 — Player-facing branding regression — repair required

`ProjectSettings.productName` and `MainMenuUI.TitleText` both use `ClickDungeon`, but `RuntimeGameUI.BuildTopHud()` hard-codes `ClickDungeon2`. This violates the existing approved branding contract that `ClickDungeon2` is repository/internal history only.

Repair: add an engine-free presentation regression contract first, observe RED, then minimally change the gameplay HUD brand to `ClickDungeon` and re-run the relevant presentation/application verification.

### G2 — Dead feature-only Unity workflow — cleanup required

`.github/workflows/hero-remaster-unity-validation.yml` remains in the promoted tree but only triggers on `feature/hero-class-remaster-execution`. The current release path is covered by `.github/workflows/unity-platform-ci.yml` and the old feature branch is no longer an integration target.

Repair: remove the dead feature-specific workflow after the branding change is independently green. Do not weaken or replace `unity-platform-ci.yml`.

### G3 — Historical CI trigger file — cleanup required

`ci-trigger.txt` contains only `trigger PR diagnostics 2026-08-24`. It has no runtime responsibility and exists only as historical CI forcing state.

Repair: delete it on the isolated graph-repair branch.

### G4 — Superseded open PRs #4 and #8 — topology cleanup required

PR #4 and PR #8 remain open/diverged. Content-level reconciliation proves their important functionality is already represented in current `develop`; merging either branch wholesale risks reintroducing older state.

Repair: leave code untouched; add a superseded note to each PR and close them without merge. Preserve source branches until branch-containment cleanup is audited separately.

### G5 — Branch accumulation — containment audit required before deletion

The repository currently retains numerous historical `feature/*`, `gaudit-*`, `graphrepair/*`, and `unity-*` branches. No branch is safe to delete solely because its PR is closed or because a newer branch exists.

Repair boundary: classify each candidate with `branch -> develop` ancestry/content comparison. Delete only branches proven fully represented in `develop` or `main`. Branch deletion is a separate topology pass after this repair PR.

### G6 — `main` and `develop` are unprotected — operational hardening required

GitHub reports both release branches as unprotected. Required-status enforcement is therefore not guaranteed by repository policy even though the current promotion was manually gated correctly.

Repair boundary: enable branch protection/rulesets requiring the established CI checks for `develop` and `main`. This requires repository administration capability and is not a code change.

### G7 — Previous art audit is historical, not current

`Docs/gaudit-art-reconciliation.md` correctly records the earlier art-reconciliation decisions, but several then-open gaps are now implemented: canonical 24-tile presentation IDs, layered board rendering, special tiles, bomb/spike presentation, imported production art, generated animation metadata, and release validation.

Action: preserve the file as historical evidence; use this document as the current post-remaster audit.

## /graphRepair execution order

1. Isolate repair branch from verified `develop`.
2. RED: add player-facing branding regression contract.
3. GREEN: change only gameplay HUD brand `ClickDungeon2` -> `ClickDungeon`.
4. Verify focused presentation/application tests.
5. Remove dead `hero-remaster-unity-validation.yml`.
6. Remove historical `ci-trigger.txt`.
7. Run full available branch CI through a PR to `develop`.
8. Add superseded notes and close PR #4 and PR #8 without merge.
9. Do not delete historical branches until a separate containment proof is complete.
10. Do not modify `main` directly; after `develop` integration, promote through the normal `develop -> main` path only if all gates remain green.
