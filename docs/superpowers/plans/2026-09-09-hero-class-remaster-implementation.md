# Hero Identity & Mechanical Class Remaster Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship nine selectable hero identities across eight deterministic mechanical classes while preserving existing enum ordinals, Sir Clickington identity/campaign behavior, save compatibility, and the 81-asset hero presentation contract.

**Architecture:** Keep mechanics in `ClickDungeon.Simulation`, identity/persistence in `ClickDungeon.Application`, and art/UI in `ClickDungeon.Presentation`. Extend the existing enum/catalog/content/ability resolver instead of creating a second class system; every new mechanic is represented by explicit deterministic state in `RunState` and exercised through existing commands/ability resolution.

**Tech Stack:** Unity 6.5 (`6000.5.9f1`), C#, NUnit EditMode tests, JSON content catalogs, Python validation scripts, GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-09-hero-identity-class-remaster-design.md`

## Global Constraints

- Preserve serialized ordinals exactly: `Knight=0`, `Ranger=1`, `Thief=2`, `Wizard=3`; append `Paladin=4`, `Berserker=5`, `Engineer=6`, `Cleric=7`.
- Exactly eight mechanical classes and nine hero identities.
- `clickington` remains a Knight identity and retains `clickington_campaign`; never introduce `HeroClassId.Clickington`.
- Keep legacy Knight/Ranger/Thief/Wizard mechanics unchanged except compatibility edits.
- New class mastery thresholds are exactly `0, 24, 56, 96, 150`.
- `classes.json` revision becomes 4; `abilities.json` revision becomes 3.
- Preserve existing runtime hero filenames and `.meta` GUIDs; required hero set remains exactly 81 PNGs.
- Do not weaken content/art/unexpected-mutation validators.
- PR #5 stays draft/unmerged until all shared remaster validation gates are green.

---

### Task 1: Lock enum compatibility and add the four new class records

**Files:**
- Modify: `Assets/ClickDungeon/Simulation/Model/Enums.cs`
- Modify: `Assets/ClickDungeon/Content/Json/classes.json`
- Modify: `Assets/ClickDungeon/Content/Json/abilities.json`
- Modify: `Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs`
- Test: `Assets/ClickDungeon/Tests/EditMode/HeroClassCompatibilityTests.cs`
- Test: `Assets/ClickDungeon/Tests/ApplicationEditMode/CanonicalRuntimeContentTests.cs`

**Interfaces:**
- Produces `HeroClassId.Paladin`, `.Berserker`, `.Engineer`, `.Cleric` with values 4–7.
- Produces canonical content records for Dawnward/Rageclaw/Gearspark/Lightbringer classes and their 20 abilities.

- [ ] **Step 1: Write the failing ordinal/content tests**

```csharp
[Test]
public void HeroClassOrdinalsRemainBackwardCompatible()
{
    Assert.That((int)HeroClassId.Knight, Is.EqualTo(0));
    Assert.That((int)HeroClassId.Ranger, Is.EqualTo(1));
    Assert.That((int)HeroClassId.Thief, Is.EqualTo(2));
    Assert.That((int)HeroClassId.Wizard, Is.EqualTo(3));
    Assert.That((int)HeroClassId.Paladin, Is.EqualTo(4));
    Assert.That((int)HeroClassId.Berserker, Is.EqualTo(5));
    Assert.That((int)HeroClassId.Engineer, Is.EqualTo(6));
    Assert.That((int)HeroClassId.Cleric, Is.EqualTo(7));
}
```

Add an application-content assertion that the loaded catalog exposes exactly eight hero definitions and that every `AbilityIds` entry resolves.

- [ ] **Step 2: Run the focused tests and verify RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter HeroClassCompatibilityTests -testResults Temp/hero-class-red.xml -quit
python scripts/validate-content.py
```

Expected: test compile/failure because the four enum values/content records do not yet exist.

- [ ] **Step 3: Implement the enum and JSON additions**

```csharp
public enum HeroClassId
{
    Knight = 0,
    Ranger = 1,
    Thief = 2,
    Wizard = 3,
    Paladin = 4,
    Berserker = 5,
    Engineer = 6,
    Cleric = 7
}
```

Add the exact class stats and ability IDs from the approved spec. Add all 20 ability records with mastery values `0/24/56/96/150`, exact charges/recharge values, names, role/effect text, and the new class IDs. Update the loader's accepted revision/schema mapping without changing legacy IDs.

- [ ] **Step 4: Run GREEN verification**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter HeroClassCompatibilityTests -testResults Temp/hero-class-green.xml -quit
python scripts/validate-content.py
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Simulation/Model/Enums.cs Assets/ClickDungeon/Content/Json/classes.json Assets/ClickDungeon/Content/Json/abilities.json Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs Assets/ClickDungeon/Tests/EditMode/HeroClassCompatibilityTests.cs Assets/ClickDungeon/Tests/ApplicationEditMode/CanonicalRuntimeContentTests.cs
git commit -m "feat: add four remastered hero classes"
```

### Task 2: Expand the hero identity catalog and deterministic selection order

**Files:**
- Modify: `Assets/ClickDungeon/Application/Heroes/HeroIdentityCatalog.cs`
- Modify: `Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs`
- Modify: `Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs`
- Test: `Assets/ClickDungeon/Tests/ApplicationEditMode/HeroIdentityTests.cs`

**Interfaces:**
- Produces class defaults: Knight→`ironheart`, Paladin→`dawnward`, Berserker→`rageclaw`, Engineer→`gearspark`, Ranger→`windsong`, Cleric→`lightbringer`, Wizard→`emberwisp`, Thief→`shadowcut`.
- Produces exactly two Knight identities: `ironheart`, `clickington`.
- Produces explicit UI class order: Knight, Paladin, Berserker, Engineer, Ranger, Cleric, Wizard, Thief.

- [ ] **Step 1: Extend identity tests first**

```csharp
[Test]
public void CatalogContainsNineIdentitiesAcrossEightClasses()
{
    Assert.That(HeroIdentityCatalog.All.Count, Is.EqualTo(9));
    Assert.That(HeroIdentityCatalog.All.Select(x => x.HeroId).Distinct().Count(), Is.EqualTo(9));
    Assert.That(HeroIdentityCatalog.All.Select(x => x.ClassId).Distinct().Count(), Is.EqualTo(8));
    Assert.That(HeroIdentityCatalog.ForClass(HeroClassId.Knight).Select(x => x.HeroId),
        Is.EquivalentTo(new[]{"ironheart","clickington"}));
}

[Test]
public void KnightClickingtonNeverCollapsesToIronheart()
{
    Assert.That(HeroIdentityCatalog.ResolveHeroId(HeroClassId.Knight,"clickington"), Is.EqualTo("clickington"));
}
```

Add assertions for every class default and UI display order.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter HeroIdentityTests -testResults Temp/hero-identity-red.xml -quit
```

- [ ] **Step 3: Implement catalog records and menu cycling**

Add the seven non-Knight identities, preserve Clickington's existing `clickington_campaign` record, and expose one helper for ordered mechanical classes. In `MainMenuUI`, cycle class by that ordered list; cycle hero only within `HeroIdentityCatalog.ForClass(currentClass)`. A single-identity class must remain stable under hero-cycle input.

- [ ] **Step 4: Run GREEN**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter HeroIdentityTests -testResults Temp/hero-identity-green.xml -quit
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Application/Heroes/HeroIdentityCatalog.cs Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs Assets/ClickDungeon/Presentation/Menu/MainMenuUI.cs Assets/ClickDungeon/Tests/ApplicationEditMode/HeroIdentityTests.cs
git commit -m "feat: expose nine remastered hero identities"
```

### Task 3: Preserve hero identity through save/load and migration

**Files:**
- Modify: `Assets/ClickDungeon/Application/Persistence/SaveDocument.cs`
- Modify: `Assets/ClickDungeon/Application/Persistence/SaveMigrator.cs`
- Modify: `Assets/ClickDungeon/Application/Persistence/LocalSaveRepository.cs`
- Test: `Assets/ClickDungeon/Tests/ApplicationEditMode/HeroIdentityPersistenceTests.cs`
- Test: `Assets/ClickDungeon/Tests/ApplicationEditMode/PersistenceTests.cs`

**Interfaces:**
- Save payload stores both `HeroClassId` and hero identity.
- Load path resolves requested identity against saved class via `HeroIdentityCatalog.ResolveHeroId`.

- [ ] **Step 1: Add the regression test**

```csharp
[Test]
public void SavedClickingtonKnightReloadsAsClickington()
{
    var save = CreateSave(heroClass: HeroClassId.Knight, heroId: "clickington");
    var loaded = RoundTrip(save);
    Assert.That(loaded.HeroClass, Is.EqualTo(HeroClassId.Knight));
    Assert.That(loaded.HeroId, Is.EqualTo("clickington"));
}
```

Also add an old-save fixture with numeric class values 0–3 and no new identity fields; verify migration keeps those classes and chooses their canonical defaults only when identity is absent/invalid.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter HeroIdentityPersistenceTests -testResults Temp/hero-persistence-red.xml -quit
```

- [ ] **Step 3: Implement migration and resolution**

Keep the existing save versioning path. Do not rewrite a valid `clickington` value to the Knight default. Reject/normalize only identity/class mismatches through the catalog.

- [ ] **Step 4: Run GREEN**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "HeroIdentityPersistenceTests|PersistenceTests" -testResults Temp/hero-persistence-green.xml -quit
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Application/Persistence Assets/ClickDungeon/Tests/ApplicationEditMode/HeroIdentityPersistenceTests.cs Assets/ClickDungeon/Tests/ApplicationEditMode/PersistenceTests.cs
git commit -m "fix: preserve hero identity across saves"
```

### Task 4: Add deterministic state primitives for the four new classes

**Files:**
- Modify: `Assets/ClickDungeon/Simulation/Model/RunState.cs`
- Modify: `Assets/ClickDungeon/Simulation/Abilities/AbilityResolver.cs`
- Modify: `Assets/ClickDungeon/Simulation/GameSession.cs`
- Test: `Assets/ClickDungeon/Tests/EditMode/NewHeroClassAbilityTests.cs`
- Test: `Assets/ClickDungeon/Tests/EditMode/NewHeroClassPassiveTests.cs`

**Interfaces:**
- State fields support bounded shield, temporary attack/defense durations, root durations, per-floor passive trigger flags, and refresh-not-stack buffs.
- `AbilityResolver` implements exact Paladin/Berserker/Engineer/Cleric effects from the spec.

- [ ] **Step 1: Write RED tests for exact mechanics**

```csharp
[Test]
public void LayOnHandsHealsFiveWithoutExceedingMaxHp()
{
    var session = NewSession(HeroClassId.Paladin, hp: 10, maxHp: 17);
    Use(session,"ability.paladin.lay_on_hands");
    Assert.That(session.State.Hp, Is.EqualTo(15));
}

[Test]
public void BloodrushNeverReducesHpBelowOneAndRefreshesAttackBuff()
{
    var session = NewSession(HeroClassId.Berserker, hp: 2);
    Use(session,"ability.berserker.bloodrush");
    Assert.That(session.State.Hp, Is.EqualTo(1));
    Assert.That(session.State.TemporaryAttackBonus, Is.EqualTo(2));
    Assert.That(session.State.TemporaryAttackActionsRemaining, Is.EqualTo(2));
}
```

Add focused tests for Radiant Strike, Consecration, Aegis, Divine Bulwark, Cleaving Blow, War Cry, Frenzy, Ragequake, Shock Wrench, Barrier Drone, Snare Mine, Overclock, Clockwork Barrage, Smite, Mend, Sanctuary, Blessing, and Radiant Renewal. Each test asserts charge consumption and duration expiration/refresh where relevant.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter NewHeroClassAbilityTests -testResults Temp/new-class-abilities-red.xml -quit
```

- [ ] **Step 3: Implement the minimum deterministic primitives**

Add only state needed by the approved abilities. Derive Berserker low-HP +1 attack instead of storing a stacking modifier. Re-use existing Knight intent-control semantics for War Cry. Use stable board/index ordering for Clockwork Barrage targeting. Refresh existing timed buffs rather than adding durations.

- [ ] **Step 4: Add and satisfy passive tests**

```csharp
[Test]
public void PaladinHalfHpPassiveFiresOnlyOncePerFloor() { /* assert +3 shield once */ }
[Test]
public void EngineerIdentifiesNearestHiddenThreatWithStableIndexTieBreak() { /* assert exact index */ }
[Test]
public void ClericFirstCompletedShrineHealsThreeOnlyOncePerFloor() { /* assert one trigger */ }
```

Reset per-floor trigger flags in `FloorGenerator.GenerateFloor`/floor-transition path, not from UI.

- [ ] **Step 5: Run the full simulation suite**

```bash
python scripts/validate-content.py
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/new-classes-full.xml -quit
```

- [ ] **Step 6: Commit**

```bash
git add Assets/ClickDungeon/Simulation/Model/RunState.cs Assets/ClickDungeon/Simulation/Abilities/AbilityResolver.cs Assets/ClickDungeon/Simulation/GameSession.cs Assets/ClickDungeon/Simulation/Generation/FloorGenerator.cs Assets/ClickDungeon/Tests/EditMode/NewHeroClassAbilityTests.cs Assets/ClickDungeon/Tests/EditMode/NewHeroClassPassiveTests.cs
git commit -m "feat: implement new hero class mechanics"
```

### Task 5: Bind all nine identities to production presentation keys

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Assets/HeroPresentationAssets.cs`
- Modify: `Assets/ClickDungeon/Presentation/Assets/PresentationAssetIdMapper.cs`
- Modify: `Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs`
- Modify: `Assets/ClickDungeon/Tests/PresentationEditMode/PresentationAssetIdMapperTests.cs`
- Modify: `scripts/validate-gameplay-art-bindings.py`
- Modify: `scripts/validate-assets.py`

**Interfaces:**
- Every hero variant resolves as `hero.<heroId>.<variant>`.
- Required hero variants are exactly `master, portrait, roster, gameplay, idle, attack, hit, victory, defeat`.

- [ ] **Step 1: Add exhaustive mapping tests**

```csharp
[TestCase("ironheart")]
[TestCase("clickington")]
[TestCase("dawnward")]
[TestCase("rageclaw")]
[TestCase("gearspark")]
[TestCase("windsong")]
[TestCase("lightbringer")]
[TestCase("emberwisp")]
[TestCase("shadowcut")]
public void EveryHeroUsesIdentityScopedPresentationKeys(string heroId)
{
    Assert.That(HeroPresentationAssetResolver.GameplayAssetId(heroId), Is.EqualTo($"hero.{heroId}.gameplay"));
    Assert.That(HeroPresentationAssetResolver.PortraitAssetId(heroId), Is.EqualTo($"hero.{heroId}.portrait"));
}
```

Extend validator fixtures so one missing/misnamed hero PNG fails.

- [ ] **Step 2: Run RED validator/test pass**

```bash
python scripts/test-validators.py
python scripts/validate-gameplay-art-bindings.py
```

- [ ] **Step 3: Implement generic identity-driven lookups and validator manifest coverage**

Remove finite switches that collapse unknown/new classes onto Ironheart. Validate against the approved nine hero IDs. Do not add class-based art fallbacks for new identities.

- [ ] **Step 4: Run GREEN**

```bash
python scripts/validate-gameplay-art-bindings.py
python scripts/validate-assets.py
python scripts/test-validators.py
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Presentation/Assets Assets/ClickDungeon/Presentation/Menu/HeroCardPresentation.cs Assets/ClickDungeon/Tests/PresentationEditMode/PresentationAssetIdMapperTests.cs scripts/validate-gameplay-art-bindings.py scripts/validate-assets.py scripts/test-validators.py
git commit -m "feat: bind all hero identities to production art"
```

### Task 6: Reconcile the 81 approved hero PNGs without GUID churn

**Files:**
- Modify in place: existing `Assets/ClickDungeon/Resources/**/hero_<heroId>_<variant>.png` files required by the current production-art manifest
- Preserve: matching `.png.meta` files
- Modify only if required by canonical registry: production-art manifest/import tooling already used by the remaster branch

**Interfaces:**
- Exactly 81 hero PNGs across nine identities × nine variants.

- [ ] **Step 1: Snapshot path→GUID pairs before replacing bytes**

```bash
python scripts/validate-unity-metadata.py
python scripts/validate-assets.py
```

Record the pre-change `.meta` GUIDs in the implementation log/checkpoint.

- [ ] **Step 2: Replace/reconcile PNG content in place**

Use the approved hero sheets as visual source-of-truth. Do not import sheet frames, text, labels, or neighboring poses into runtime sprites.

- [ ] **Step 3: Prove no GUID churn and strict 81-asset coverage**

```bash
python scripts/validate-unity-metadata.py
python scripts/validate-assets.py
python scripts/validate-gameplay-art-bindings.py
```

Expected: all nine hero families resolve all nine variants; `.meta` identities for pre-existing paths are unchanged.

- [ ] **Step 4: Commit**

```bash
git add Assets/ClickDungeon/Resources
git commit -m "art: reconcile remastered hero runtime assets"
```

### Task 7: Hero-remaster verification gate

**Files:**
- No feature files unless a failing gate identifies a scoped defect.

- [ ] **Step 1: Run engine-free/content validators**

```bash
python -m compileall -q scripts
python scripts/validate-content.py
python scripts/validate-replay.py
python scripts/validate-assets.py
python scripts/validate-gameplay-art-bindings.py
python scripts/static-audit.py
python scripts/test-validators.py
grep -F 'm_EditorVersion: 6000.5.9f1' ProjectSettings/ProjectVersion.txt
```

- [ ] **Step 2: Run Unity EditMode validation**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/hero-remaster-editmode.xml -quit
```

- [ ] **Step 3: Perform visual verification**

Check all nine identities for silhouette, face/hair, palette, primary equipment, VFX language, transparent isolation, gameplay-scale readability, and continuity across master/portrait/roster/gameplay/idle/attack/hit/victory/defeat.

- [ ] **Step 4: Run `/Gaudit` and `/graphRepair` after the implementation is otherwise green**

No merge on any unresolved P0/P1/P2 finding or any red CI/Unity gate.

- [ ] **Step 5: Commit only scoped repairs, then hand off to the gameplay/tile plan**

```bash
git status --short
git log -1 --oneline
```
