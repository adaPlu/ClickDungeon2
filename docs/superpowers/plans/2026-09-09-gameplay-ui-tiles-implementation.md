# Gameplay Screen & Dungeon Tile Remaster Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rebuild the live gameplay presentation around the approved `main(1).png` hierarchy and make the 24 approved dungeon tile identities usable through deterministic simulation-backed state.

**Architecture:** Preserve the existing 5×5 `RunState` board and command-driven `RuntimeGameUI`. Extend `DungeonRoomPresentationLayout`/tile presentation mapping for the new art vocabulary, add only the smallest missing tile mechanics to simulation state, then refactor the UI hierarchy to top HUD → dominant board → primary actions → footer navigation.

**Tech Stack:** Unity 6.5 (`6000.5.9f1`), C#, Unity UI/TMP, NUnit EditMode tests, deterministic simulation, production-art import/validation scripts.

**Spec:** `docs/superpowers/specs/2026-09-09-gameplay-tiles-monster-batch-design.md`

## Global Constraints

- Preserve the authoritative 5×5 board and simulation command ownership.
- The gameplay screenshot is a layout reference only; never flatten it into the runtime scene/background.
- Canonical tile set is exactly the 24 identities listed in the approved spec.
- Layer order is `base floor/terrain → structural tile → content/occupant → state/telegraph overlay`.
- Existing semantics remain authoritative; no gameplay meaning may be inferred from art alone.
- Bomb, spike, pressure plate, teleport, and healing fountain require deterministic simulation tests before generator placement.
- Preserve `.meta` GUIDs when reconciling an existing tile PNG.
- Keep unexpected-mutation protections strict and PR #5 draft/unmerged.

---

### Task 1: Normalize the canonical 24-tile presentation vocabulary

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Assets/DungeonRoomPresentationLayout.cs`
- Modify: `Assets/ClickDungeon/Presentation/Assets/PresentationAssetId.cs`
- Modify: `Assets/ClickDungeon/Presentation/Assets/PresentationAssetIdMapper.cs`
- Modify: `Assets/ClickDungeon/Tests/PresentationEditMode/PresentationAssetIdMapperTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml` only if the new pure presentation-contract file list changes

**Interfaces:**
- Produces stable presentation IDs for all 24 approved tiles.
- Produces `BaseTileAssetId(TileState tile, int index)` and `StructuralTileAssetId(TileState tile)` style pure presentation policy without mutating simulation state.

- [ ] **Step 1: Write the exhaustive RED test**

```csharp
[Test]
public void CanonicalDungeonTileRegistryContainsExactlyTwentyFourIds()
{
    var ids = DungeonRoomPresentationLayout.CanonicalTileIds;
    Assert.That(ids.Count, Is.EqualTo(24));
    Assert.That(ids, Does.Contain("tile.floor.stone"));
    Assert.That(ids, Does.Contain("tile.floor.cracked"));
    Assert.That(ids, Does.Contain("tile.floor.moss"));
    Assert.That(ids, Does.Contain("tile.water"));
    Assert.That(ids, Does.Contain("tile.lava"));
    Assert.That(ids, Does.Contain("tile.shadow"));
    Assert.That(ids, Does.Contain("tile.trap.pit"));
    Assert.That(ids, Does.Contain("tile.trap.bomb"));
    Assert.That(ids, Does.Contain("tile.trap.spike"));
    Assert.That(ids, Does.Contain("tile.pressure_plate"));
    Assert.That(ids, Does.Contain("tile.teleport"));
    Assert.That(ids, Does.Contain("tile.fountain.heal"));
    Assert.That(ids, Does.Contain("tile.stair.up"));
    Assert.That(ids, Does.Contain("tile.stair.up.locked"));
    Assert.That(ids, Does.Contain("tile.stair.down"));
    Assert.That(ids, Does.Contain("tile.stair.down.locked"));
    Assert.That(ids, Does.Contain("tile.wall"));
    Assert.That(ids, Does.Contain("tile.wall.corner"));
    Assert.That(ids, Does.Contain("tile.key"));
    Assert.That(ids, Does.Contain("tile.chest.closed"));
    Assert.That(ids, Does.Contain("tile.chest.open"));
    Assert.That(ids, Does.Contain("tile.door.locked"));
    Assert.That(ids, Does.Contain("tile.door.open"));
    Assert.That(ids, Does.Contain("tile.torch"));
}
```

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter PresentationAssetIdMapperTests -testResults Temp/tile-registry-red.xml -quit
```

- [ ] **Step 3: Implement the canonical IDs and compatibility aliases only where an old runtime semantic needs one**

Keep filename normalization in `PresentationAssetId`. The canonical resource filenames are the approved `tile_*.png` names. Do not create duplicate physical sprites for legacy names; map legacy runtime IDs to the canonical presentation ID in code.

- [ ] **Step 4: Add tile-layer transition tests**

```csharp
[Test]
public void ResolvedChestUsesOpenChestPresentation()
{
    var tile = new TileState { Content=TileContentKind.Chest, Visibility=TileVisibility.Revealed, Resolution=TileResolution.Resolved };
    Assert.That(TilePresentationAssetResolver.PrimaryAssetId(tile), Is.EqualTo("tile.chest.open"));
}
```

Add locked/unlocked exit/door and terrain base-layer assertions using actual state flags introduced in later tasks.

- [ ] **Step 5: Run GREEN and commit**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter PresentationAssetIdMapperTests -testResults Temp/tile-registry-green.xml -quit
git add Assets/ClickDungeon/Presentation/Assets Assets/ClickDungeon/Tests/PresentationEditMode/PresentationAssetIdMapperTests.cs .github/workflows/presentation-contract-tests.yml
git commit -m "feat: normalize canonical dungeon tile presentation"
```

### Task 2: Add deterministic tile-interaction state primitives

**Files:**
- Modify: `Assets/ClickDungeon/Simulation/Model/Enums.cs`
- Modify: `Assets/ClickDungeon/Simulation/Model/RunState.cs`
- Modify: `Assets/ClickDungeon/Simulation/Model/TileState.cs`
- Modify: `Assets/ClickDungeon/Simulation/GameSession.cs`
- Test: `Assets/ClickDungeon/Tests/EditMode/SpecialDungeonTileTests.cs`

**Interfaces:**
- Bomb and spike traps resolve through existing trap/damage pipeline.
- Pressure plates store one linked target index/state and resolve once.
- Teleports store deterministic pair identity/destination index.
- Healing fountain stores one-shot resolved state and bounded heal amount.

- [ ] **Step 1: Write RED tests for exact state transitions**

```csharp
[Test]
public void HealingFountainHealsOnceAndCapsAtMaxHp()
{
    var session = SessionWithSpecialTile("fountain.heal", playerHp: 8, maxHp: 12, amount: 5);
    ResolveSpecial(session);
    Assert.That(session.State.Hp, Is.EqualTo(12));
    ResolveSpecial(session);
    Assert.That(session.State.Hp, Is.EqualTo(12));
}

[Test]
public void TeleportUsesStoredDestinationInsteadOfRuntimeRandomness()
{
    var session = SessionWithTeleportPair(sourceIndex: 6, destinationIndex: 18);
    ResolveTile(session, 6);
    Assert.That(session.State.PlayerPosition, Is.EqualTo(GridPosition.FromIndex(18)));
}
```

Add tests proving bomb/spikes use deterministic damage, a pressure plate unlocks/toggles only its stored linked target, and all one-shot objects become resolved.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter SpecialDungeonTileTests -testResults Temp/special-tiles-red.xml -quit
```

- [ ] **Step 3: Implement minimal state**

Prefer explicit scalar fields on `TileState` such as `LinkedTileIndex`, `TeleportDestinationIndex`, and `Amount` where existing serialization already handles them. Add a new enum value only when the current `TileContentKind.SpecialEvent` cannot distinguish deterministic resolution safely. Do not add a separate board-object engine.

- [ ] **Step 4: Route resolution through `GameSession`**

Use the same command result/event pattern as chest/trap/shrine resolution so UI receives state changes without calculating effects.

- [ ] **Step 5: Run GREEN and commit**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter SpecialDungeonTileTests -testResults Temp/special-tiles-green.xml -quit
git add Assets/ClickDungeon/Simulation Assets/ClickDungeon/Tests/EditMode/SpecialDungeonTileTests.cs
git commit -m "feat: add deterministic special dungeon tiles"
```

### Task 3: Generate special tiles and pairs deterministically

**Files:**
- Modify: `Assets/ClickDungeon/Simulation/Generation/FloorGenerator.cs`
- Modify: `Assets/ClickDungeon/Content/Json/floor_archetypes.json` only if current archetype schema is the approved placement configuration point
- Test: `Assets/ClickDungeon/Tests/EditMode/FloorGenerationDeterminismTests.cs`
- Test: `Assets/ClickDungeon/Tests/EditMode/SpecialDungeonTileGenerationTests.cs`

**Interfaces:**
- Generation assigns pressure-plate links and teleport pairs before play begins.
- Same root seed/floor/route produces identical tile IDs, indexes, pair destinations, and linked targets.

- [ ] **Step 1: Add deterministic-generation tests**

```csharp
[Test]
public void SameSeedProducesSameSpecialTilePairing()
{
    var a = new FloorGenerator(content).CreateNewRun(12345, HeroClassId.Knight);
    var b = new FloorGenerator(content).CreateNewRun(12345, HeroClassId.Knight);
    Assert.That(SnapshotSpecialTiles(a), Is.EqualTo(SnapshotSpecialTiles(b)));
}
```

Add assertions that no teleport targets itself, pairs are symmetric where applicable, pressure plates reference a valid generated route/door object, and special tiles never overwrite the start cell/boss cell/mandatory exits.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter SpecialDungeonTileGenerationTests -testResults Temp/special-generation-red.xml -quit
```

- [ ] **Step 3: Implement generation using the existing floor RNG/seed derivation**

Do not call `UnityEngine.Random` or create time-based seeds. Keep optional/special placement bounded so the existing 25-cell mandatory pool remains valid.

- [ ] **Step 4: Run GREEN and the simulation suite**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "SpecialDungeonTileGenerationTests|FloorGenerationDeterminismTests" -testResults Temp/special-generation-green.xml -quit
python scripts/validate-content.py
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Simulation/Generation/FloorGenerator.cs Assets/ClickDungeon/Content/Json/floor_archetypes.json Assets/ClickDungeon/Tests/EditMode
git commit -m "feat: generate special dungeon tiles deterministically"
```

### Task 4: Refactor `RuntimeGameUI` to the approved gameplay hierarchy

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs`
- Create: `Assets/ClickDungeon/Presentation/Assets/GameplayScreenPresentationLayout.cs`
- Create: `Assets/ClickDungeon/Tests/PresentationEditMode/GameplayScreenPresentationLayoutTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml` to include the new engine-free layout file/test

**Interfaces:**
- Landscape anchors expose four bands: top HUD, dominant board, primary action bar, persistent footer.
- Portrait reflows the same logical bands vertically.

- [ ] **Step 1: Define the RED layout contract test**

```csharp
[Test]
public void LandscapeBoardIsDominantCenterRegion()
{
    var layout = GameplayScreenPresentationLayout.Landscape;
    Assert.That(layout.Board.Width, Is.GreaterThan(0.50f));
    Assert.That(layout.Board.Height, Is.GreaterThan(0.55f));
    Assert.That(layout.Hud.MaxY, Is.EqualTo(1f));
    Assert.That(layout.ActionBar.MaxY, Is.LessThanOrEqualTo(layout.Board.MinY));
    Assert.That(layout.Footer.MinY, Is.EqualTo(0f));
}
```

Add portrait-order assertions: HUD above board, board above actions, actions above footer.

- [ ] **Step 2: Run RED with the engine-free presentation harness**

```bash
# Use the same net8 harness recipe defined by .github/workflows/presentation-contract-tests.yml
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --logger "console;verbosity=normal"
```

- [ ] **Step 3: Implement the pure layout constants**

Represent normalized anchors/regions only. Keep all game rules out of the layout contract.

- [ ] **Step 4: Rebuild `RuntimeGameUI.BuildUi` composition**

Create `TopHud`, `BoardFrame/Board`, `PrimaryActionBar`, and `FooterNavigation` transforms. Move hero portrait, HP/readout, currencies, floor/depth placard, settings/menu, ability buttons, and Inventory/Talents/Shop controls into those regions. Preserve existing button command listeners and `_session` mutations exactly at the command layer.

- [ ] **Step 5: Add runtime smoke assertions**

Add Unity EditMode/PlayMode-friendly hierarchy tests if existing test infrastructure permits instantiating `RuntimeGameUI`; otherwise use a small `RuntimeGameUIHierarchyTests` EditMode fixture that builds the UI under a test GameObject and asserts the named transforms and 25 tile buttons exist.

```csharp
Assert.That(root.Find("SafeRoot/TopHud"), Is.Not.Null);
Assert.That(root.Find("SafeRoot/BoardFrame/Board").childCount, Is.EqualTo(25));
Assert.That(root.Find("SafeRoot/PrimaryActionBar"), Is.Not.Null);
Assert.That(root.Find("SafeRoot/FooterNavigation"), Is.Not.Null);
```

- [ ] **Step 6: Run GREEN and commit**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter RuntimeGameUIHierarchyTests -testResults Temp/runtime-ui-green.xml -quit
git add Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs Assets/ClickDungeon/Presentation/Assets/GameplayScreenPresentationLayout.cs Assets/ClickDungeon/Tests/PresentationEditMode .github/workflows/presentation-contract-tests.yml
git commit -m "feat: remaster runtime gameplay screen layout"
```

### Task 5: Render canonical tile layers in the live board

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs`
- Modify: `Assets/ClickDungeon/Presentation/Assets/PresentationAssetIdMapper.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/PresentationAssetIdMapperTests.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/RuntimeGameUIHierarchyTests.cs`

**Interfaces:**
- Each cell owns distinct images for base terrain, structure, content/occupant, and overlay/telegraph.

- [ ] **Step 1: Add layer-policy RED tests**

```csharp
[Test]
public void LavaTerrainUsesLavaBaseWithoutReplacingMonsterOccupant()
{
    var tile = RevealedMonsterTile(terrain: TerrainKind.Lava, contentId: "monster.goblin");
    Assert.That(TilePresentationAssetResolver.BaseAssetId(tile, 0), Is.EqualTo("tile.lava"));
    Assert.That(TilePresentationAssetResolver.PrimaryAssetId(tile), Is.EqualTo("monster.goblin"));
}
```

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter PresentationAssetIdMapperTests -testResults Temp/tile-layer-red.xml -quit
```

- [ ] **Step 3: Implement the four image layers per cell**

Replace the current single floor + icon assumptions with explicit base/structure/content/state image references. Decorative walls/corners/torches remain framing unless the simulation exposes them as stateful structures.

- [ ] **Step 4: Preserve clue/threat/target overlays**

Overlay color/telegraph logic may tint the overlay image only; source sprites remain untinted except deliberate transient hit/selection treatment.

- [ ] **Step 5: Run GREEN and commit**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "PresentationAssetIdMapperTests|RuntimeGameUIHierarchyTests" -testResults Temp/tile-layer-green.xml -quit
git add Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs Assets/ClickDungeon/Presentation/Assets/PresentationAssetIdMapper.cs Assets/ClickDungeon/Tests/PresentationEditMode
git commit -m "feat: render layered dungeon tiles"
```

### Task 6: Reconcile/import the 24 approved tile sprites

**Files:**
- Modify in place or create as canonical: approved `tile_*.png` runtime resources
- Preserve existing matching `.png.meta` files
- Modify: `Assets/ClickDungeon/Editor/ProductionArtManifest.cs`
- Modify: `Assets/ClickDungeon/Editor/ProductionArtValidator.cs` only if tile-category validation requires explicit coverage
- Modify: `scripts/validate-assets.py`
- Modify: `scripts/validate-gameplay-art-bindings.py`
- Modify: `scripts/test-validators.py`

**Interfaces:**
- All 24 canonical presentation IDs resolve to exactly one runtime sprite.

- [ ] **Step 1: Extend validator fixtures first**

Add a fixture where one canonical tile is absent and assert validator failure. Add a duplicate-alias fixture and assert the validator rejects two physical runtime assets for one canonical tile identity.

- [ ] **Step 2: Run RED**

```bash
python scripts/test-validators.py
python scripts/validate-assets.py
```

- [ ] **Step 3: Reconcile approved tile art**

Use final-sheet `tile_*` naming. Replace an existing equivalent PNG in place and retain its `.meta`. Create a new PNG/meta pair only for a truly new canonical identity.

- [ ] **Step 4: Validate dimensions/import metadata/bindings**

```bash
python scripts/validate-unity-metadata.py
python scripts/validate-assets.py
python scripts/validate-gameplay-art-bindings.py
python scripts/test-validators.py
```

- [ ] **Step 5: Run Unity import validation twice and require idempotence**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/tile-import-pass1.xml -quit
git status --short
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/tile-import-pass2.xml -quit
git status --short
```

Expected: second pass introduces no new tracked mutations.

- [ ] **Step 6: Commit**

```bash
git add Assets/ClickDungeon/Resources Assets/ClickDungeon/Editor/ProductionArtManifest.cs Assets/ClickDungeon/Editor/ProductionArtValidator.cs scripts/validate-assets.py scripts/validate-gameplay-art-bindings.py scripts/test-validators.py
git commit -m "art: integrate canonical dungeon tile set"
```

### Task 7: Gameplay/tile visual and regression gate

**Files:**
- No feature files unless a failing gate identifies a scoped defect.

- [ ] **Step 1: Run simulation/presentation/content validators**

```bash
python -m compileall -q scripts
python scripts/validate-content.py
python scripts/validate-replay.py
python scripts/validate-assets.py
python scripts/validate-gameplay-art-bindings.py
python scripts/validate-unity-metadata.py
python scripts/static-audit.py
python scripts/test-validators.py
```

- [ ] **Step 2: Run full Unity EditMode suite**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/gameplay-tiles-full.xml -quit
```

- [ ] **Step 3: Perform live landscape visual verification against `main(1).png`**

Verify: top brand/HUD strip, selected hero portrait, prominent HP bar, resource/ability status, currency counters, floor placard, stone dungeon framing, board dominance, readable tile layers, large primary action row, and persistent Inventory/Talents/Shop footer. Also verify portrait reflow preserves HUD→board→actions→footer order.

- [ ] **Step 4: Confirm no gameplay regression**

Play/drive at least one deterministic floor containing chest/key/trap/monster/exit plus each newly enabled special tile primitive; verify simulation results match EditMode expectations.

- [ ] **Step 5: Run `/Gaudit` and `/graphRepair` only after all ordinary gates are green**

Do not merge PR #5; hand off to the Monster Batch 1 plan next.
