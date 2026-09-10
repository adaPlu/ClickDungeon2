# Hero/Class Remaster Expanded Design

Approved 2026-09-10 for draft PR #6 on `feature/hero-class-remaster-execution`.

## Goal
Turn the supplied title/gameplay references into functional runtime UI and deterministic presentation contracts without replacing working simulation architecture or baking controls into screenshots.

## Approved behavior

- Keep the runtime-built Unity Canvas and the `ClickDungeon` title.
- Sir Clickington remains a distinct hero identity using Knight/Ironheart mechanics and `clickington_campaign`, with dedicated art.
- CONTINUE is enabled only when the selected slot has a valid resumable `ActiveRun`; otherwise it displays `NO ACTIVE RUN` and is disabled.
- PLAY always enters the new-run Hero Select flow and never silently resumes an existing run.
- Inventory, Talents, Settings, Shop, Quit, Hero Select, and Daily Reward are real functional controls.
- Persistent account state owns menu currencies and daily-reward state; run-local simulation gold remains separate.
- Daily Reward grants once per eligible UTC calendar day, updates balances immediately, persists claim/streak state, and exposes next availability.
- Gameplay remains deterministic and exactly 5x5; UI may present state but does not become simulation authority.
- Complete first-class presentation vocabulary covers normal/cracked/mossy floor, pit, bomb/spike traps, stairs up/down/locked, wall/corner, doors closed/open/locked, key, chest closed/open, torch, water, lava, shadow/void, pressure plate, teleport, and healing fountain.
- Existing mechanics map to presentation directly; genuinely new mechanics require explicit deterministic state rather than cosmetic-only tiles.
- The uploaded monster sheet is the starting presentation roster; the larger existing roster remains available on later content.
- No generated replacement art is required for this design pass; preserve tracked Unity `.meta` GUIDs for equivalent art.

## Verification contract

Use strict TDD: RED test/contract, observed failure in CI/Unity, minimal GREEN, focused tests, full verification, adversarial graph-style audit. Keep PR #6 draft/unmerged while any required gate is red.
