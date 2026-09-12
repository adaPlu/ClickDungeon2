# Hero Selection Screen Design

## Goal
Replace the current class-first / hero-second selector with a single full hero presentation page that shows one of the nine hero identities at a time and exposes explicit SELECT and NEXT controls at the bottom.

## Approved behavior

- Show exactly one hero at a time in this order: Ironheart, Sir Clickington, Windsong, Shadowcut, Emberwisp, Lightbringer, Rageclaw, Gearspark, Dawnward.
- NEXT advances to the following hero and wraps from Dawnward back to Ironheart.
- SELECT starts a new run in the already selected save slot using the hero currently displayed.
- Hero artwork itself is decorative; names, stats, buttons, and navigation are real Unity UI controls.
- Use existing production hero artwork, preferring `hero.<id>.master` for the large showcase and identity-scoped fallback variants only. Do not generate new images.
- The screen should echo the supplied hero identity sheets: large hero art, strong name/class identity, stat panels, class kit, and gameplay identity.
- Replace the old upper-right Portrait/HUD, Roster Icon, and Gameplay/Chibi preview concepts with gameplay/stat information rather than duplicate art.
- Replace the Animation States strip with live gameplay stats/information rather than animation thumbnails.
- All displayed numerical values must come from canonical `HeroDefinition` data; do not invent speed/crit/rarity-style values.
- Sir Clickington remains hero id `clickington`, class `Knight`, with Knight mechanics/stats and his existing `clickington_campaign` identity.
- The new Paladin, Berserker, Engineer, and Cleric classes use their canonical class records already implemented on `feature/hero-class-remaster-execution`.
- Keep a CLOSE control outside the bottom SELECT/NEXT pair so the user can dismiss the selector without starting a run.

## Layout

The overlay uses most of the safe viewport. The top area contains ClickDungeon / CHOOSE YOUR HERO framing and the current hero name/class. The left/center region contains the large production hero master art. The upper-right region contains four gameplay fact cards derived from canonical content: HP, Attack, Defense, and ability-count/signature information. A horizontal lower strip replaces Animation States with stat/detail cards. Beneath that, two reference-sheet-inspired panels show the hero's ability kit and gameplay identity/passive.

SELECT and NEXT are the primary bottom actions. NEXT never starts a run. SELECT is the only hero-selection action that calls the new-run path.

## Non-goals

- No new artwork or image generation.
- No rebalance of class statistics or abilities.
- No new mechanical class IDs.
- No change to save-slot selection behavior, game scene loading, or Sir Clickington campaign ownership.
- No merge of PR #5 or PR #6 as part of this UI task; this work remains isolated until validation is green.
