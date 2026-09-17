# ClickDungeon2 Vertical-Slice Production Art Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the approved ClickDungeon2 5x5 vertical-slice production art as implementation-ready runtime assets first, validate them deterministically in Unity, and only then derive the five approved polished reference sheets.

**Architecture:** Extend the existing presentation and editor tooling instead of creating a parallel art system. A pure engine-free vertical-slice contract defines the exact approved identities and mandatory frame sequences; an additive slice manifest records `KEEP`/`REPAIR`/`CREATE`, dimensions, pivots, GUID expectations, bindings, and validation state; the existing presentation ID mapper, importer, animation generator, asset database generator, and Unity CI remain the integration path. Existing global production-art requirements stay intact.

**Tech Stack:** Unity 6000.5.9f1, C#, NUnit, .NET 8 engine-free presentation tests, Unity Editor asset APIs, PNG runtime art, Git/GitHub Actions, existing ClickDungeon presentation/editor tools, OpenAI image generation for new or repaired painted art.

**Spec:** `docs/superpowers/specs/2026-09-15-clickdungeon-vertical-slice-production-art-design.md`

## Global Constraints

- Phase order is fixed: **Phase A runtime art first; Phase B reference sheets only after Phase A is green**.
- Approved runtime identities are exactly **Sir Clickington, Goblin Raider, Slime, Mimic Chest, and Lord Blobert**.
- Fidelity is **production-normalized fidelity**: preserve identity/style while normalizing runtime scale, transparent bounds, pivots, lighting/readability, and VFX footprint.
- Existing good runtime files are `KEEP`; preserve their paths, presentation IDs, and Unity `.meta` GUIDs.
- `REPAIR` and `CREATE` are permitted only when the approved slice requires them.
- Do not narrow or replace `ProductionArtManifest`; the vertical-slice contract is additive.
- Do not change simulation balance, enemy mechanics, boss mechanics, content JSON, routing, or menu architecture merely to accommodate art.
- The Mimic's unrevealed disguise must use the same canonical closed-chest visual as a normal chest; do not create a reliable visual tell.
- Mandatory painted frame sequences are Clickington sword attack, Mimic reveal, Mimic bite/tongue attack, and Lord Blobert signature attack. Chest/reward frame animation is optional and must be added only if static states plus VFX fail visual review.
- Runtime PNGs must not contain reference-sheet backgrounds, captions, filenames, callouts, or decorative sheet chrome.
- New/repaired painted art must be generated from the supplied authoritative references or an audited approved runtime exemplar. If those visual references are unavailable to the executing worker, stop before generating substitutes and request access rather than guessing.
- Do not procedurally fabricate painterly production art with PIL or vector placeholders. Scripts may only normalize alpha/canvas/padding or validate files after generation.
- Later heroes, Goblin Bomber, Goblin Key Warden, Skeleton Warrior, Fire Imp, Cave Spider, Crowned Slime, Armored Boar, Bat Swarm Leader, Spooky Spellbook, Theater Curtain Demon, later bosses/encyclopedia content, new biomes, and a full-game art refresh are out of scope.

## File Structure

New focused files:

- `Assets/ClickDungeon/Presentation/Assets/VerticalSliceProductionArtContract.cs` — pure approved identity/static-state/sequence contract.
- `Assets/ClickDungeon/Presentation/Assets/RuntimeArtSequenceId.cs` — pure parser for numbered `anim_*.png` frame files.
- `Assets/ClickDungeon/Tests/PresentationEditMode/VerticalSliceProductionArtContractTests.cs` — exact roster/static/sequence/non-goal contract tests.
- `Assets/ClickDungeon/Tests/PresentationEditMode/RuntimeArtSequenceIdTests.cs` — frame filename parsing/order tests.
- `Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json` — audited asset ledger for this slice only.
- `Assets/ClickDungeon/Editor/VerticalSliceProductionArtManifest.cs` — editor-side JSON model/loader and invariant checks.
- `Assets/ClickDungeon/Editor/VerticalSliceProductionArtValidator.cs` — Unity import, GUID, ID-collision, dimension, alpha, frame-sequence, and manifest validation.
- `Assets/ClickDungeon/Art/Runtime/GeneratedAnimations/VerticalSlice/` — generated clips for the four mandatory frame sequences.
- `Assets/ClickDungeon/Art/Reference/VerticalSlice/` — Phase B documentation/reference sheets, outside runtime lookup.

Existing files modified narrowly:

- `Assets/ClickDungeon/Presentation/Assets/PresentationAssetId.cs` — ignore `anim_` files and map approved generic `ui_`/`vfx_` filename families.
- `Assets/ClickDungeon/Editor/AnimationClipGenerator.cs` — generate validated clips from isolated frame sequences.
- `Assets/ClickDungeon/Editor/PresentationAssetGenerator.cs` — ensure animation/reference files never enter the static runtime Sprite database; no architecture rewrite.
- `.github/workflows/presentation-contract-tests.yml` — compile/run the new pure contract/parser tests.
- Presentation consumers under `Assets/ClickDungeon/Presentation/` only where the audit proves an existing visible slot needs a new art binding.

---

### Task 1: Lock the engine-free vertical-slice art contract

**Files:**
- Create: `Assets/ClickDungeon/Presentation/Assets/VerticalSliceProductionArtContract.cs`
- Create: `Assets/ClickDungeon/Tests/PresentationEditMode/VerticalSliceProductionArtContractTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- Consumes: `DungeonRoomPresentationLayout.CanonicalTileIds`.
- Produces: `VerticalSliceProductionArtContract.RequiredStaticIds`, `VerticalSliceProductionArtContract.RequiredSequences`, and `VerticalSliceSequenceRequirement` for every later task.

- [ ] **Step 1: Write the failing contract test**

Create `VerticalSliceProductionArtContractTests.cs` with assertions for the exact actor set, mandatory sequences, and stale-scope exclusions:

```csharp
using System.Linq;
using NUnit.Framework;
using ClickDungeon.Presentation.Assets;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class VerticalSliceProductionArtContractTests
    {
        [Test]
        public void ActorIds_AreExactlyApprovedCoreSlice()
        {
            CollectionAssert.AreEquivalent(new[]
            {
                "hero.clickington",
                "monster.goblin_raider",
                "monster.slime",
                "monster.mimic_chest",
                "boss.lord_blobert"
            }, VerticalSliceProductionArtContract.ActorIds);
        }

        [Test]
        public void RequiredSequences_AreExactHybridMinimum()
        {
            var pairs = VerticalSliceProductionArtContract.RequiredSequences
                .Select(x => $"{x.Id}:{x.FrameCount}:{x.FramesPerSecond}").ToArray();
            CollectionAssert.AreEqual(new[]
            {
                "hero.clickington.attack:5:12",
                "monster.mimic_chest.reveal:6:12",
                "monster.mimic_chest.bite:5:12",
                "boss.lord_blobert.attack:6:12"
            }, pairs);
        }

        [TestCase("monster.fire_imp")]
        [TestCase("monster.cave_spider")]
        [TestCase("monster.crowned_slime")]
        [TestCase("boss.goblin_brute_king")]
        public void Contract_DoesNotReintroduceLaterScope(string id)
        {
            Assert.That(VerticalSliceProductionArtContract.ActorIds, Does.Not.Contain(id));
        }
    }
}
```

Add the new source and test `<Compile Include=...>` entries to the workflow-generated `.csproj` before running the test.

- [ ] **Step 2: Run the test and verify RED**

Run the workflow shell command locally from the repository root after copying the workflow's `.ci/presentation-contract-tests/PresentationContractTests.csproj` generation block, then:

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter VerticalSliceProductionArtContractTests
```

Expected: FAIL to compile because `VerticalSliceProductionArtContract` and `VerticalSliceSequenceRequirement` do not exist.

- [ ] **Step 3: Implement the minimal pure contract**

Create the contract with these exact public shapes:

```csharp
using System;
using System.Collections.Generic;

namespace ClickDungeon.Presentation.Assets
{
    public readonly struct VerticalSliceSequenceRequirement
    {
        public VerticalSliceSequenceRequirement(string id, string filePrefix, int frameCount, int framesPerSecond)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            FilePrefix = filePrefix ?? throw new ArgumentNullException(nameof(filePrefix));
            FrameCount = frameCount;
            FramesPerSecond = framesPerSecond;
        }
        public string Id { get; }
        public string FilePrefix { get; }
        public int FrameCount { get; }
        public int FramesPerSecond { get; }
    }

    public static class VerticalSliceProductionArtContract
    {
        public static readonly string[] ActorIds =
        {
            "hero.clickington", "monster.goblin_raider", "monster.slime",
            "monster.mimic_chest", "boss.lord_blobert"
        };

        public static readonly VerticalSliceSequenceRequirement[] RequiredSequences =
        {
            new VerticalSliceSequenceRequirement("hero.clickington.attack", "anim_clickington_attack_", 5, 12),
            new VerticalSliceSequenceRequirement("monster.mimic_chest.reveal", "anim_mimic_chest_reveal_", 6, 12),
            new VerticalSliceSequenceRequirement("monster.mimic_chest.bite", "anim_mimic_chest_bite_", 5, 12),
            new VerticalSliceSequenceRequirement("boss.lord_blobert.attack", "anim_lord_blobert_attack_", 6, 12)
        };

        public static readonly string[] RequiredStaticIds =
        {
            "hero.clickington.master", "hero.clickington.gameplay", "hero.clickington.portrait",
            "hero.clickington.roster", "hero.clickington.idle", "hero.clickington.attack",
            "hero.clickington.hit", "hero.clickington.victory", "hero.clickington.defeat",
            "monster.goblin_raider.gameplay", "monster.goblin_raider.idle", "monster.goblin_raider.attack",
            "monster.goblin_raider.hit", "monster.goblin_raider.defeat",
            "monster.slime.gameplay", "monster.slime.idle", "monster.slime.attack",
            "monster.slime.hit", "monster.slime.defeat",
            "monster.mimic_chest.gameplay", "monster.mimic_chest.idle", "monster.mimic_chest.attack",
            "monster.mimic_chest.hit", "monster.mimic_chest.defeat",
            "boss.lord_blobert.gameplay", "boss.lord_blobert.idle", "boss.lord_blobert.attack",
            "boss.lord_blobert.hit", "boss.lord_blobert.defeat"
        };
    }
}
```

Keep portrait/master/spawn states out of the mandatory static set for monsters/bosses until a current presentation consumer is proven in Task 2; they can be added to the manifest as consumer-backed requirements without expanding actor scope.

- [ ] **Step 4: Run GREEN**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter VerticalSliceProductionArtContractTests
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Presentation/Assets/VerticalSliceProductionArtContract.cs Assets/ClickDungeon/Tests/PresentationEditMode/VerticalSliceProductionArtContractTests.cs .github/workflows/presentation-contract-tests.yml
git commit -m "test: lock vertical-slice production art contract"
```

---

### Task 2: Build and freeze the KEEP / REPAIR / CREATE manifest

**Files:**
- Create: `Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json`
- Create: `Assets/ClickDungeon/Editor/VerticalSliceProductionArtManifest.cs`
- Test: `Assets/ClickDungeon/Tests/PresentationEditMode/VerticalSliceProductionArtManifestContractTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- Consumes: `VerticalSliceProductionArtContract.RequiredStaticIds` and `RequiredSequences`.
- Produces: JSON entries with `assetId`, `path`, `category`, `role`, `referenceKey`, `disposition`, `width`, `height`, `pivotX`, `pivotY`, `sequenceId`, `expectedGuid`, `bindingId`, `validationStatus`.

- [ ] **Step 1: Write RED tests for manifest invariants**

Create a pure parser/model test that requires exactly one disposition and rejects unresolved values:

```csharp
[Test]
public void Parse_RejectsUnclassifiedDisposition()
{
    const string json = "{\"entries\":[{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"UNCLASSIFIED\"}]}";
    Assert.Throws<InvalidOperationException>(() => VerticalSliceProductionArtManifest.Parse(json));
}

[Test]
public void Parse_RejectsDuplicateAssetIds()
{
    const string json = "{\"entries\":[{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"KEEP\"},{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"CREATE\"}]}";
    Assert.Throws<InvalidOperationException>(() => VerticalSliceProductionArtManifest.Parse(json));
}
```

- [ ] **Step 2: Run RED**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter VerticalSliceProductionArtManifestContractTests
```

Expected: FAIL because the manifest model/parser does not exist.

- [ ] **Step 3: Implement the parser/model and perform the repository audit**

Implement `VerticalSliceProductionArtManifest.Parse(string json)` as a pure `System.Text.Json` parser in the Presentation/Assets namespace if engine-free compilation is needed; keep Unity-specific GUID/import checks in the editor validator. Use an enum equivalent to `KEEP`, `REPAIR`, `CREATE`; reject any other string, duplicate asset IDs, non-positive dimensions, pivots outside `[0,1]`, and `KEEP` entries with empty `expectedGuid`.

Audit `Assets/ClickDungeon/Art/Runtime/`, `PresentationAssetDatabase`, current menu/HUD/reward consumers, and `.meta` files. Populate the JSON so every approved required static ID and every mandatory sequence frame is present with one final disposition before Task 6 begins.

Dimension rule during audit:

```text
KEEP: retain the existing file dimensions exactly.
REPAIR/CREATE character family: match the approved existing family exemplar; if no approved exemplar exists, 512x512.
REPAIR/CREATE Lord Blobert boss family: match current boss exemplar; if none exists, 768x768.
REPAIR/CREATE UI icon: match current action/resource icon exemplar; if none exists, 256x256.
REPAIR/CREATE local VFX: match current VFX exemplar; if none exists, 512x512.
REPAIR/CREATE tile state: exact dimensions of the audited canonical tile family, never a guessed independent size.
New full-screen presentation art with no exemplar: 1920x1080 master.
```

For every `KEEP`, obtain the GUID from the existing `.meta` and store it in `expectedGuid`. For `REPAIR`, store the existing GUID when the path is preserved. For `CREATE`, `expectedGuid` is empty until Unity imports the new file; Task 5 records it after first deterministic import.

- [ ] **Step 4: Run GREEN**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter VerticalSliceProductionArtManifestContractTests
```

Expected: PASS, and a manual JSON scan contains no `UNCLASSIFIED`, stale enemy identity, or empty dimensions.

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Art/Manifests Assets/ClickDungeon/Presentation/Assets/VerticalSliceProductionArtManifest.cs Assets/ClickDungeon/Tests/PresentationEditMode/VerticalSliceProductionArtManifestContractTests.cs .github/workflows/presentation-contract-tests.yml
git commit -m "feat: add vertical-slice production art manifest"
```

---

### Task 3: Separate static presentation IDs from animation-frame IDs

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Assets/PresentationAssetId.cs`
- Create: `Assets/ClickDungeon/Tests/PresentationEditMode/VerticalSlicePresentationAssetIdTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- Consumes: runtime PNG filename.
- Produces: `PresentationAssetId.FromRuntimeArtFile(string)` static ID or empty string for numbered animation frames.

- [ ] **Step 1: Write RED tests**

```csharp
[TestCase("anim_clickington_attack_01.png")]
[TestCase("anim_mimic_chest_reveal_06.png")]
public void AnimationFrames_DoNotBecomeStaticPresentationIds(string name)
{
    Assert.That(PresentationAssetId.FromRuntimeArtFile(name), Is.Empty);
}

[TestCase("ui_action_attack.png", "ui.action.attack")]
[TestCase("ui_enemy_alert.png", "ui.enemy.alert")]
[TestCase("vfx_slash_arc.png", "vfx.slash.arc")]
public void GenericUiAndVfxFamilies_MapDeterministically(string name, string expected)
{
    Assert.That(PresentationAssetId.FromRuntimeArtFile(name), Is.EqualTo(expected));
}
```

- [ ] **Step 2: Run RED**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter VerticalSlicePresentationAssetIdTests
```

Expected: at least the `anim_` exclusion and generic `ui_`/`vfx_` cases fail.

- [ ] **Step 3: Implement minimal mapping**

At the top of `FromRuntimeArtFile`, before generic suffix processing:

```csharp
if (n.StartsWith("anim_", StringComparison.Ordinal)) return string.Empty;
if (n.StartsWith("ui_", StringComparison.Ordinal)) return "ui." + n.Substring(3).Replace('_', '.');
if (n.StartsWith("vfx_", StringComparison.Ordinal)) return "vfx." + n.Substring(4).Replace('_', '.');
```

Preserve every existing hero/monster/boss/tile mapping branch unchanged.

- [ ] **Step 4: Run GREEN plus existing ID tests**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter "VerticalSlicePresentationAssetIdTests|PresentationAssetIdMapperTests"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Presentation/Assets/PresentationAssetId.cs Assets/ClickDungeon/Tests/PresentationEditMode/VerticalSlicePresentationAssetIdTests.cs .github/workflows/presentation-contract-tests.yml
git commit -m "fix: isolate animation frames from static asset ids"
```

---

### Task 4: Parse and validate mandatory numbered frame sequences

**Files:**
- Create: `Assets/ClickDungeon/Presentation/Assets/RuntimeArtSequenceId.cs`
- Create: `Assets/ClickDungeon/Tests/PresentationEditMode/RuntimeArtSequenceIdTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- Produces: `bool RuntimeArtSequenceId.TryParse(string filename, out string sequenceId, out int frameIndex)`.
- Consumed by: `AnimationClipGenerator` and `VerticalSliceProductionArtValidator`.

- [ ] **Step 1: Write RED parser tests**

```csharp
[TestCase("anim_clickington_attack_01.png", "hero.clickington.attack", 1)]
[TestCase("anim_mimic_chest_reveal_04.png", "monster.mimic_chest.reveal", 4)]
[TestCase("anim_mimic_chest_bite_05.png", "monster.mimic_chest.bite", 5)]
[TestCase("anim_lord_blobert_attack_06.png", "boss.lord_blobert.attack", 6)]
public void TryParse_RecognizesApprovedSequences(string file, string expectedId, int expectedFrame)
{
    Assert.That(RuntimeArtSequenceId.TryParse(file, out string id, out int frame), Is.True);
    Assert.That(id, Is.EqualTo(expectedId));
    Assert.That(frame, Is.EqualTo(expectedFrame));
}

[TestCase("anim_clickington_attack_xx.png")]
[TestCase("anim_fire_imp_attack_01.png")]
[TestCase("hero_clickington_attack.png")]
public void TryParse_RejectsMalformedOrOutOfScopeFiles(string file)
{
    Assert.That(RuntimeArtSequenceId.TryParse(file, out _, out _), Is.False);
}
```

- [ ] **Step 2: Run RED**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter RuntimeArtSequenceIdTests
```

Expected: compile failure because `RuntimeArtSequenceId` does not exist.

- [ ] **Step 3: Implement parser against `RequiredSequences`**

Use `Path.GetFileNameWithoutExtension`, match exactly one `VerticalSliceSequenceRequirement.FilePrefix`, require a two-digit positive suffix, and reject a frame index above that requirement's `FrameCount`. Do not maintain a second hard-coded sequence list.

- [ ] **Step 4: Run GREEN**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter RuntimeArtSequenceIdTests
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Presentation/Assets/RuntimeArtSequenceId.cs Assets/ClickDungeon/Tests/PresentationEditMode/RuntimeArtSequenceIdTests.cs .github/workflows/presentation-contract-tests.yml
git commit -m "feat: add vertical-slice animation frame contract"
```

---

### Task 5: Add the Unity-side slice validator and deterministic animation generation

**Files:**
- Create: `Assets/ClickDungeon/Editor/VerticalSliceProductionArtValidator.cs`
- Modify: `Assets/ClickDungeon/Editor/AnimationClipGenerator.cs`
- Modify: `Assets/ClickDungeon/Editor/PresentationAssetGenerator.cs`
- Test: `Assets/ClickDungeon/Tests/EditMode/VerticalSliceProductionArtValidatorTests.cs`

**Interfaces:**
- Consumes: slice JSON manifest, `PresentationAssetId.FromRuntimeArtFile`, `RuntimeArtSequenceId.TryParse`, Unity `TextureImporter`, `AssetDatabase.AssetPathToGUID`.
- Produces: menu/CI callable `VerticalSliceProductionArtValidator.ValidateOrThrow()` and clips under `Assets/ClickDungeon/Art/Runtime/GeneratedAnimations/VerticalSlice/`.

- [ ] **Step 1: Write RED Unity EditMode tests**

Use temporary test assets under `Assets/ClickDungeon/Tests/TempVerticalSliceArt/` and assert:

```csharp
[Test]
public void Validator_RejectsDuplicateStaticPresentationIds() { /* two files mapping to same ID -> throws */ }

[Test]
public void Validator_RejectsKeepGuidDrift() { /* manifest expectedGuid != AssetPathToGUID -> throws */ }

[Test]
public void Validator_RejectsWrongSequenceFrameCount() { /* 4 of required 5 Clickington frames -> throws */ }

[Test]
public void Validator_RejectsMixedSequenceDimensions() { /* one frame has different width -> throws */ }
```

The fixture must delete its temporary assets in `TearDown`.

- [ ] **Step 2: Run RED in Unity EditMode**

Run with the repository's Unity 6000.5.9f1 installation:

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform EditMode -testFilter ClickDungeon.Tests.EditMode.VerticalSliceProductionArtValidatorTests -testResults TestResults/vertical-slice-validator.xml -logFile -
```

Expected: FAIL because `VerticalSliceProductionArtValidator` does not exist.

- [ ] **Step 3: Implement the validator**

`ValidateOrThrow()` must, in this order:

1. load and parse `vertical_slice_production_art_manifest.json`;
2. require every contract static ID and every required sequence frame to have exactly one manifest entry;
3. reject duplicate non-empty `PresentationAssetId` values;
4. ensure every runtime static PNG required by the manifest exists;
5. compare `KEEP`/preserved `REPAIR` GUIDs to `AssetDatabase.AssetPathToGUID(path)`;
6. inspect `TextureImporter` and require Sprite import, alpha transparency where manifest category requires it, and manifest width/height after import;
7. group `anim_` files through `RuntimeArtSequenceId`, require frames `01..N` exactly once, fixed dimensions, and fixed pivot;
8. reject runtime files from `Art/Reference/VerticalSlice` if the static database generator attempts to include them.

Update `PresentationAssetGenerator` so an empty `PresentationAssetId` is skipped explicitly and only `Art/Runtime` is considered.

- [ ] **Step 4: Extend `AnimationClipGenerator`**

For each `RequiredSequences` entry, enumerate `Assets/ClickDungeon/Art/Runtime/<prefix>*.png`, parse/sort by frame index, load the Sprite, write a non-looping clip at the required FPS, and save to:

```text
GeneratedAnimations/VerticalSlice/clickington_attack.anim
GeneratedAnimations/VerticalSlice/mimic_chest_reveal.anim
GeneratedAnimations/VerticalSlice/mimic_chest_bite.anim
GeneratedAnimations/VerticalSlice/lord_blobert_attack.anim
```

The generator must throw before writing a clip if frames are missing, duplicated, out of order, or dimensionally inconsistent.

- [ ] **Step 5: Run GREEN**

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform EditMode -testFilter ClickDungeon.Tests.EditMode.VerticalSliceProductionArtValidatorTests -testResults TestResults/vertical-slice-validator.xml -logFile -
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add Assets/ClickDungeon/Editor/VerticalSliceProductionArtValidator.cs Assets/ClickDungeon/Editor/AnimationClipGenerator.cs Assets/ClickDungeon/Editor/PresentationAssetGenerator.cs Assets/ClickDungeon/Tests/EditMode/VerticalSliceProductionArtValidatorTests.cs
git commit -m "feat: validate vertical-slice runtime art"
```

---

### Task 6: Produce and integrate Sir Clickington's Phase A runtime set

**Files:**
- Modify/Create only manifest-classified Clickington files under `Assets/ClickDungeon/Art/Runtime/`
- Create: `anim_clickington_attack_01.png` through `anim_clickington_attack_05.png`
- Modify: `Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json`
- Modify presentation consumer only if Task 2 proves an existing Clickington slot is not yet bound.

**Interfaces:**
- Static fallback IDs are `hero.clickington.master`, `.gameplay`, `.portrait`, `.roster`, `.idle`, `.attack`, `.hit`, `.victory`, `.defeat`.
- Animated sequence ID is `hero.clickington.attack` at 5 frames / 12 fps.

- [ ] **Step 1: Establish RED from the manifest/validator before generation**

Run:

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -executeMethod ClickDungeon.EditorTools.VerticalSliceProductionArtValidator.ValidateOrThrow -logFile -
```

Expected: FAIL listing the exact Clickington `CREATE`/`REPAIR` assets or missing attack frames; existing `KEEP` files must not be listed as replaceable.

- [ ] **Step 2: Generate only the listed Clickington needs**

Use the supplied Sir Clickington identity reference and audited approved runtime exemplar. Preserve lucky crown, adventurer scarf, sword, royal shield, palette, mascot silhouette, 3/4 gameplay readability, and the manifest's exact canvas/pivot. Generate transparent isolated PNGs; for attack frames, keep identical canvas and contact point through all five frames.

For a `REPAIR`, edit the existing visual while preserving its path and `.meta`; for a `CREATE`, write the new PNG to the manifest path and let Unity create its `.meta`. Never overwrite a `KEEP` PNG.

- [ ] **Step 3: Import, generate clip, and record new GUIDs**

Run the existing importer/generator plus the slice animation generator once. Record generated GUIDs for `CREATE` entries back into the manifest after import, then rerun the validator.

- [ ] **Step 4: Verify GREEN at runtime scale**

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -executeMethod ClickDungeon.EditorTools.VerticalSliceProductionArtValidator.ValidateOrThrow -logFile -
```

Expected: Clickington-related validation passes. In Unity Game view, inspect both portrait and landscape: idle/attack/hit swaps keep stable contact, attack silhouette reads inside one board cell, and master/portrait art fits existing menu/reward slots.

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Art/Runtime Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json
git commit -m "art: complete Sir Clickington vertical-slice set"
```

---

### Task 7: Produce and integrate the four core encounters

**Files:**
- Modify/Create only manifest-classified Goblin Raider, Slime, Mimic, and Lord Blobert files under `Assets/ClickDungeon/Art/Runtime/`
- Create mandatory animation frames:
  - `anim_mimic_chest_reveal_01.png` … `_06.png`
  - `anim_mimic_chest_bite_01.png` … `_05.png`
  - `anim_lord_blobert_attack_01.png` … `_06.png`
- Modify manifest JSON.
- Modify presentation binding only where an existing consumer needs an identity alias; do not edit `monsters.json` or `bosses.json`.

**Interfaces:**
- Goblin presentation identity: `monster.goblin_raider`.
- Slime presentation identity: `monster.slime`.
- Mimic presentation identity: `monster.mimic_chest` after reveal; pre-reveal uses canonical normal closed chest art.
- Boss presentation identity: `boss.lord_blobert`.

- [ ] **Step 1: Run RED validator for encounter assets**

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -executeMethod ClickDungeon.EditorTools.VerticalSliceProductionArtValidator.ValidateOrThrow -logFile -
```

Expected: FAIL only on unresolved core-encounter entries after Clickington is green.

- [ ] **Step 2: Produce Goblin Raider and Slime**

Generate/repair only manifest-listed states. Goblin uses gameplay/idle/attack/hit/defeat minimum; Slime uses gameplay/idle/body-impact attack/hit/defeat minimum. Use Unity motion/squash for simple movement rather than adding extra painted frames. Add master/portrait/spawn only when Task 2's consumer audit recorded them as required.

- [ ] **Step 3: Produce Mimic with a no-tell disguise**

Do not generate a special unrevealed chest. Bind the unrevealed state to the same canonical closed chest sprite used by ordinary chests. Generate the monster key poses plus 6-frame reveal and 5-frame bite/tongue sequence. The first painted Mimic-specific visual appears only after reveal begins.

- [ ] **Step 4: Produce Lord Blobert**

Generate gameplay/idle/attack fallback/hit/defeat plus the 6-frame signature attack. Add portrait/master/spawn only if the manifest's existing-consumer audit requires them. Keep signature VFX separate where practical.

- [ ] **Step 5: Validate and review in the 5x5 composition**

Run the validator and inspect all four encounters alongside Clickington in both orientations. Standard enemies must not overwhelm a cell; Blobert may be larger but cannot hide adjacent board information.

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -executeMethod ClickDungeon.EditorTools.VerticalSliceProductionArtValidator.ValidateOrThrow -logFile -
```

Expected: encounter-related manifest and sequence checks PASS.

- [ ] **Step 6: Commit**

```bash
git add Assets/ClickDungeon/Art/Runtime Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json
git commit -m "art: complete core vertical-slice encounters"
```

---

### Task 8: Fill only missing environment, interactable, and board-overlay art

**Files:**
- Modify/Create manifest-classified tile/interactable/overlay files under `Assets/ClickDungeon/Art/Runtime/`
- Modify manifest JSON.
- Test/modify existing canonical tile registry/presentation tests only if a new visual state needs a new presentation ID; do not add new simulation tile concepts.

**Interfaces:**
- Consumes: all 24 `DungeonRoomPresentationLayout.CanonicalTileIds`.
- Produces: consumer-backed selection/target/danger/lock/chest/exit/trap state visuals.

- [ ] **Step 1: Run RED validator and isolate only board/interactable deficits**

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -executeMethod ClickDungeon.EditorTools.VerticalSliceProductionArtValidator.ValidateOrThrow -logFile -
```

Expected: FAIL on board/interactable manifest entries still marked `CREATE`/`REPAIR`; canonical assets marked `KEEP` remain untouched.

- [ ] **Step 2: Fill only consumer-backed visual states**

Preserve the existing canonical 24 tile identities. Create/repair normal chest closed/open, key/use, lock/unlock, exit/resolution, switch/plate, trap telegraph/trigger, selection, target, and danger art only when the current flow exposes that state. Match exact audited tile-family dimensions and do not create new simulation semantics.

- [ ] **Step 3: Verify layered-board readability**

Run existing canonical and layered presentation tests plus the slice validator. In Game view verify that base terrain, structure, content, and state overlays remain independently legible and that selected/target/danger treatment does not cover enemy/interactable identity.

- [ ] **Step 4: Commit**

```bash
git add Assets/ClickDungeon/Art/Runtime Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json Assets/ClickDungeon/Tests/PresentationEditMode
git commit -m "art: finish vertical-slice board interactions"
```

---

### Task 9: Fill the consumer-backed HUD and reusable VFX set

**Files:**
- Modify/Create manifest-classified `ui_*.png` and `vfx_*.png` under `Assets/ClickDungeon/Art/Runtime/`
- Modify manifest JSON.
- Modify existing HUD/action consumer files only when the manifest records an existing visible slot needing the new ID.

**Interfaces:**
- Static ID convention: `ui_foo_bar.png -> ui.foo.bar`, `vfx_foo_bar.png -> vfx.foo.bar`.
- VFX remain separate from actor frames whenever reusable.

- [ ] **Step 1: RED via consumer audit and validator**

For each proposed UI/VFX file, prove an existing consumer or approved visible slice state in the manifest. Then run the validator and confirm it fails for the missing files. Do not generate an icon simply because an earlier concept board contained it.

- [ ] **Step 2: Produce the minimal HUD/VFX set**

Expected families, only when the audited consumer exists: attack/slash, interact, potion/heal, hero/enemy/boss health, selected/target/danger, enemy alert, slash arc, attack impact, hit feedback, heal glow, pickup sparkle, unlock pulse, chest-opening burst, Mimic reveal support, Blobert signature effect, defeat support, and reward burst. Shield/block remains conditional on an existing active action.

- [ ] **Step 3: GREEN and visual hierarchy check**

Run presentation contract tests and the Unity slice validator. In both orientations verify no effect obscures critical tile state, health/status remains readable, and action/resource icons remain clear at actual HUD size.

- [ ] **Step 4: Commit**

```bash
git add Assets/ClickDungeon/Art/Runtime Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json Assets/ClickDungeon/Presentation
git commit -m "art: complete vertical-slice HUD and VFX"
```

---

### Task 10: Complete title/start and victory/reward presentation art without routing changes

**Files:**
- Modify/Create manifest-classified flow-screen assets under `Assets/ClickDungeon/Art/Runtime/`
- Modify existing `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`, `HeroCardPresentation.cs`, or `MenuOverlayFactory.cs` only if a current slot lacks a production binding.
- Modify the existing reward/end-of-run presentation file found by the Task 2 consumer audit; do not introduce a second reward screen.
- Modify manifest JSON.

**Interfaces:**
- Existing `MainMenuRunRouting` behavior remains unchanged.
- Title/reward presentation consumes Clickington master/portrait/victory and consumer-backed panel/button/reward assets.

- [ ] **Step 1: Write/extend a presentation contract test before any binding change**

If a binding is missing, add a test that exercises the current presentation helper and expects the new static asset ID. If all slots already bind correctly, no code change is required; the RED state is the manifest validator reporting missing production PNGs.

- [ ] **Step 2: Produce/repair only the missing flow-screen assets**

Complete the ClickDungeon title treatment, dungeon backdrop, Clickington showcase/portrait/card presentation, Continue/Play and Daily Reward visuals, bottom-nav/panel chrome actually used, required button states, victory treatment, reward container states, awarded resource icons, reward burst, return/continue controls, and reward panel background only where the existing current flow consumes them.

- [ ] **Step 3: Run menu routing/presentation tests and validator**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --filter "MainMenuRunRoutingTests|HeroPresentationContractTests"
```

Then run the Unity slice validator. Expected: PASS with no routing semantics changed.

- [ ] **Step 4: Commit**

```bash
git add Assets/ClickDungeon/Art/Runtime Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json Assets/ClickDungeon/Presentation Assets/ClickDungeon/Tests
git commit -m "art: finish vertical-slice flow presentation"
```

---

### Task 11: Prove Phase A completeness, deterministic import, and end-to-end visual acceptance

**Files:**
- Modify manifest JSON validation statuses to `PASS` only after each gate succeeds.
- Do not change art during the second deterministic import pass.

**Interfaces:**
- Consumes the complete Phase A runtime set.
- Produces a green manifest and a clean repository after a repeat Unity pass.

- [ ] **Step 1: Run all engine-free presentation tests**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo
```

Expected: PASS.

- [ ] **Step 2: Run Unity EditMode validation**

```bash
Unity -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -logFile -
```

Expected: PASS.

- [ ] **Step 3: Run first complete import/generation/validation pass and commit expected generated metadata**

Run the existing asset importer, `PresentationAssetGenerator`, `AnimationClipGenerator`, and `VerticalSliceProductionArtValidator.ValidateOrThrow`. Review `git status --short`; only expected runtime art, `.meta`, generated vertical-slice clips, manifest, tests, and narrow presentation bindings may be present. Commit those expected outputs.

- [ ] **Step 4: Run the exact same complete pass a second time and require zero mutation**

```bash
git status --porcelain > /tmp/clickdungeon-before-second-pass.txt
# run the same importer/generator/validator commands used in Step 3
git diff --exit-code
test -z "$(git status --porcelain)"
```

Expected: both commands succeed; the second pass creates no tracked or untracked mutation.

- [ ] **Step 5: Exercise the approved visual path in both orientations**

Verify in a real player/Game view: `title/start -> 5x5 run -> Goblin Raider/Slime/Mimic presentations -> Lord Blobert presentation -> exit/resolution -> victory/reward`. This is a presentation acceptance pass, not authorization to invent missing simulation mechanics. If an approved art identity has no existing runtime consumer, record that as a separate gameplay/product gap and keep this art change scoped to assets/presentation rather than modifying simulation in this plan.

Acceptance: stable pivots, no clipping, no tile-information obstruction, readable overlays, Mimic no-tell before reveal, coherent title/gameplay/boss/reward language, and correct portrait/landscape hierarchy.

- [ ] **Step 6: Run repository CI on the exact Phase A head**

Push the branch and run/observe `Presentation Contract Tests` and `.github/workflows/unity-platform-ci.yml`. Do not claim Phase A green until the exact commit under review passes all applicable checks.

- [ ] **Step 7: Commit Phase A validation state**

```bash
git add Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json
git commit -m "test: certify vertical-slice phase A art"
```

---

### Task 12: Generate Phase B reference sheets only from the validated Phase A set

**Files:**
- Create under `Assets/ClickDungeon/Art/Reference/VerticalSlice/`:
  - `clickington_character_sheet.png`
  - `core_enemy_boss_sheet.png`
  - `dungeon_interactable_sheet.png`
  - `ui_vfx_sheet.png`
  - `complete_vertical_slice_presentation.png`
- Add matching `.meta` files through Unity import.
- Modify: `Assets/ClickDungeon/Tests/EditMode/VerticalSliceProductionArtValidatorTests.cs` if needed to lock exclusion from runtime lookup.

**Interfaces:**
- Consumes: only Phase A assets with manifest validation `PASS`.
- Produces: exactly five documentation/reference sheets; no runtime IDs.

- [ ] **Step 1: Write RED exclusion test**

Add a test that places a temporary PNG under `Art/Reference/VerticalSlice/`, runs presentation database generation, and asserts no database entry resolves from that path.

- [ ] **Step 2: Run RED if exclusion is not already guaranteed**

Run the focused Unity EditMode test. If it already passes because the generator strictly scans `Art/Runtime`, retain the regression test and do not change production code.

- [ ] **Step 3: Generate exactly five polished reference sheets**

Use the validated Phase A PNGs as the visual source. These sheets may contain labels, layout chrome, role annotations, state callouts, and composite backgrounds because they are documentation artifacts. They may not introduce a new character, enemy, gameplay state, or visual identity absent from Phase A.

- [ ] **Step 4: Validate runtime isolation**

Run the full slice validator and presentation database generator. Expected: all five sheets import as reference assets but produce zero runtime presentation IDs and do not alter Phase A manifest completeness.

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Art/Reference/VerticalSlice Assets/ClickDungeon/Tests/EditMode/VerticalSliceProductionArtValidatorTests.cs
git commit -m "docs: add vertical-slice production reference sheets"
```

---

### Task 13: Final scope audit and release-quality verification

**Files:**
- No production code change expected; modify only tests/manifest if a verification defect is found, then rerun the affected task's RED/GREEN cycle.

**Interfaces:**
- Consumes: final Phase A + Phase B branch head.
- Produces: evidence that the implementation matches the approved spec without stale-scope drift.

- [ ] **Step 1: Run the full engine-free and Unity test suites applicable to changed files**

Run presentation contract tests, full Unity EditMode tests, the slice validator, and the repository Unity Platform CI on the exact head.

- [ ] **Step 2: Verify preserve-and-fill invariants**

Compare all manifest `KEEP` entries to their recorded GUIDs and paths. Confirm no `KEEP` PNG changed. Confirm the global `Assets/ClickDungeon/Editor/ProductionArtManifest.cs` still contains the existing broader production manifest and was not narrowed to this slice.

- [ ] **Step 3: Search for forbidden scope drift**

Review changed files and ensure no newly added Phase A/Phase B asset entry targets Goblin Bomber, Goblin Key Warden, Skeleton Warrior, Fire Imp, Cave Spider, Crowned Slime, Armored Boar, Bat Swarm Leader, Spooky Spellbook, Theater Curtain Demon, later heroes, or new biomes. Existing pre-work assets may remain untouched; the rule is no new work for them in this plan.

- [ ] **Step 4: Verify clean deterministic state**

```bash
git diff --exit-code
test -z "$(git status --porcelain)"
```

Expected: clean working tree after the final validation pass.

- [ ] **Step 5: Final commit only if verification metadata changed**

```bash
git add Assets/ClickDungeon/Art/Manifests/vertical_slice_production_art_manifest.json Assets/ClickDungeon/Tests
git commit -m "test: finalize vertical-slice production art validation"
```

If Step 5 has nothing to commit, do not create an empty commit.

## Self-Review Result

- **Spec coverage:** Phase A contract, preserve-first audit, runtime file rules, exact core roster, hybrid animation, environment/interactables, HUD/VFX, start/reward flow, deterministic import, full-path visual acceptance, and Phase B's exact five sheets each have explicit tasks.
- **Scope:** The plan is one cohesive presentation/art subsystem. It explicitly forbids gameplay/content redesign and later-game expansion.
- **Type consistency:** `VerticalSliceProductionArtContract`, `VerticalSliceSequenceRequirement`, `RuntimeArtSequenceId.TryParse`, manifest fields, validator entry point, and mandatory sequence IDs/prefixes are used consistently across tasks.
- **No unresolved implementation placeholders:** conditional art states are decided by the deterministic Task 2 consumer audit; if no current consumer exists, they are omitted rather than guessed.
- **Preservation:** the existing broad `ProductionArtManifest` remains untouched by design; slice validation is additive.
