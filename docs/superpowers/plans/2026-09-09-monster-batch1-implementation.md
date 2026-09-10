# Monster Batch 1 Remaster Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the ten approved Monster Batch 1 identities, presentation families, deterministic encounter behavior, and production art while preserving every existing legacy monster and boss under approved Option A additive compatibility.

**Architecture:** Keep stable gameplay/content IDs in `ClickDungeon.Simulation` and JSON content, add one explicit content-ID→presentation-family mapping in `ClickDungeon.Presentation`, extend regular encounter selection deterministically instead of replacing the biome pool, and convert boss milestones from a single definition to deterministic candidate pools without changing the existing seed model. New Batch-1 identities are strict production-art participants; unremastered legacy monsters retain the existing fallback path.

**Tech Stack:** Unity 6.5 (`6000.5.9f1`), C#, NUnit EditMode tests, JSON content catalogs, deterministic `SeedDerivation`/`XorShift32`, Unity presentation assets, Python validation scripts, GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-09-gameplay-tiles-monster-batch-design.md`

## Global Constraints

- Preserve Option A: legacy monsters and all five legacy campaign bosses remain valid and encounterable.
- Batch 1 contains exactly ten remastered identities and exactly ten presentation variants per identity: `master`, `portrait`, `roster`, `gameplay`, `spawn`, `idle`, `attack`, `hit`, `victory`, `defeat`.
- Batch-1 production-art requirement is exactly 100 PNGs.
- Boss-prefixed families: `goblin_brute_king`, `bat_swarm_leader`, `theater_curtain_demon`; all other Batch-1 families use the monster prefix.
- Compatibility mappings are identity refinements, not content migrations: `monster.slime`→Crowned Slime, `monster.skeleton`→Skeleton Warrior, `monster.spider`→Cave Spider. These legacy IDs must remain readable in saves/replays/content references.
- New regular IDs: `monster.mimic_chest`, `monster.fire_imp`, `monster.armored_boar`, `monster.spooky_spellbook`.
- New boss IDs: `boss.goblin_brute_king`, `boss.bat_swarm_leader`, `boss.theater_curtain_demon`.
- Do not rename/remove generic `monster.goblin`, `monster.bat`, `monster.demon`, or any other legacy monster.
- Regular encounter selection and boss selection remain deterministic from existing run/floor seeds; do not use `UnityEngine.Random`, wall-clock time, or UI state.
- Missing Batch-1 production art is a hard validation failure. Legacy unremastered monsters may retain existing core fallback behavior.
- Preserve existing `.meta` GUIDs for already-tracked runtime files.
- No bespoke parallel combat engine. Fit behavior to the existing threat/intent/damage pipeline and add only the smallest deterministic primitive required.
- PR #5 remains draft/unmerged until the hero, gameplay/tile, and Monster Batch 1 plans all pass their shared final gates.

---

### Task 1: Add an explicit content-ID → presentation-family resolver

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/Assets/MonsterPresentationAssets.cs`
- Create: `Assets/ClickDungeon/Presentation/Assets/MonsterPresentationFamilyResolver.cs`
- Create: `Assets/ClickDungeon/Tests/PresentationEditMode/MonsterPresentationFamilyResolverTests.cs`
- Modify: `.github/workflows/presentation-contract-tests.yml`

**Interfaces:**
- `MonsterPresentationFamilyResolver.FamilyFor(string contentId)` returns the canonical art family without changing the simulation/content ID.
- `MonsterPresentationAssets` resolves variants from the presentation family and preserves legacy fallback only for non-Batch-1 identities.

- [ ] **Step 1: Write the failing family-mapping tests**

```csharp
[TestCase("monster.slime", "crowned_slime")]
[TestCase("monster.skeleton", "skeleton_warrior")]
[TestCase("monster.spider", "cave_spider")]
[TestCase("monster.mimic_chest", "mimic_chest")]
[TestCase("monster.fire_imp", "fire_imp")]
[TestCase("monster.armored_boar", "armored_boar")]
[TestCase("monster.spooky_spellbook", "spooky_spellbook")]
[TestCase("boss.goblin_brute_king", "goblin_brute_king")]
[TestCase("boss.bat_swarm_leader", "bat_swarm_leader")]
[TestCase("boss.theater_curtain_demon", "theater_curtain_demon")]
public void BatchOneContentIdsResolveToCanonicalFamilies(string contentId, string expected)
{
    Assert.That(MonsterPresentationFamilyResolver.FamilyFor(contentId), Is.EqualTo(expected));
}

[Test]
public void LegacyUnremasteredMonsterKeepsItsOwnFamily()
{
    Assert.That(MonsterPresentationFamilyResolver.FamilyFor("monster.goblin"), Is.EqualTo("goblin"));
}
```

Also assert that the three compatibility IDs are still returned unchanged by content migration APIs; presentation mapping must not rewrite their simulation IDs.

- [ ] **Step 2: Run the presentation contract suite and verify RED**

```bash
# Recreate/use the same .NET 8 harness defined by presentation-contract-tests.yml
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --logger "console;verbosity=normal"
```

Expected: compile/failure because the resolver does not yet exist.

- [ ] **Step 3: Implement the smallest explicit resolver**

```csharp
public static string FamilyFor(string contentId)
{
    switch(contentId)
    {
        case "monster.slime": return "crowned_slime";
        case "monster.skeleton": return "skeleton_warrior";
        case "monster.spider": return "cave_spider";
        case "monster.mimic_chest": return "mimic_chest";
        case "monster.fire_imp": return "fire_imp";
        case "monster.armored_boar": return "armored_boar";
        case "monster.spooky_spellbook": return "spooky_spellbook";
        case "boss.goblin_brute_king": return "goblin_brute_king";
        case "boss.bat_swarm_leader": return "bat_swarm_leader";
        case "boss.theater_curtain_demon": return "theater_curtain_demon";
        default: return StripKnownPrefix(contentId);
    }
}
```

Keep this presentation-only. Do not put the mapping into `content_migrations.json`.

- [ ] **Step 4: Make `MonsterPresentationAssets` use the family resolver**

Variant lookup becomes `monster.<family>.<variant>` or `boss.<family>.<variant>` based on the content classification, while legacy unremastered identities still fall back to their current base sprite when a variant is absent. Batch-1 identities must expose an `IsBatchOne`/strict-family helper so validators/runtime tests can reject missing variants rather than silently borrow unrelated art.

- [ ] **Step 5: Run GREEN and commit**

```bash
dotnet test .ci/presentation-contract-tests/PresentationContractTests.csproj --configuration Release --nologo --logger "console;verbosity=normal"
git add Assets/ClickDungeon/Presentation/Assets/MonsterPresentationAssets.cs Assets/ClickDungeon/Presentation/Assets/MonsterPresentationFamilyResolver.cs Assets/ClickDungeon/Tests/PresentationEditMode/MonsterPresentationFamilyResolverTests.cs .github/workflows/presentation-contract-tests.yml
git commit -m "feat: map monster content ids to remastered art families"
```

### Task 2: Add the seven regular Batch-1 content identities without removing legacy monsters

**Files:**
- Modify: `Assets/ClickDungeon/Content/Json/monsters.json`
- Modify: `Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs` only if schema/revision validation requires it
- Modify: `Assets/ClickDungeon/Simulation/Content/GameContent.cs`
- Modify: `Assets/ClickDungeon/Simulation/Content/Definitions.cs` only if encounter weight is modeled on the monster definition in Task 3
- Modify: `Assets/ClickDungeon/Tests/ApplicationEditMode/CanonicalRuntimeContentTests.cs`
- Create: `Assets/ClickDungeon/Tests/EditMode/MonsterBatchOneContentTests.cs`
- Modify: `scripts/validate-content.py`

**Interfaces:**
- Existing `monster.slime`, `monster.skeleton`, `monster.spider` remain the same IDs but expose the approved display identities and current/refined biome memberships.
- Four new regular IDs are loadable through `GameContent.Monster`.

- [ ] **Step 1: Write RED content tests**

```csharp
[TestCase("monster.slime", "Crowned Slime")]
[TestCase("monster.skeleton", "Skeleton Warrior")]
[TestCase("monster.spider", "Cave Spider")]
[TestCase("monster.mimic_chest", "Mimic Chest")]
[TestCase("monster.fire_imp", "Fire Imp")]
[TestCase("monster.armored_boar", "Armored Boar")]
[TestCase("monster.spooky_spellbook", "Spooky Spellbook")]
public void RegularBatchOneContentIsLoadable(string id, string displayName)
{
    var monster = content.Monster(id);
    Assert.That(monster.DisplayName, Is.EqualTo(displayName));
}
```

Add membership assertions:
- slime keeps its existing biome memberships;
- skeleton remains crypt/frozen ruins;
- spider remains sunken temple/thorn wilds/mire and gains cavern;
- mimic is eligible in all ten biomes but is uncommon through Task 3 weighting;
- fire imp is lava field/ash wastes;
- armored boar is thorn wilds/cavern;
- spooky spellbook is crypt/storm plateau/arcane nexus.

Add a regression assertion that representative legacy IDs (`monster.goblin`, `monster.bat`, `monster.demon`, `monster.rat`) remain present.

- [ ] **Step 2: Run RED**

```bash
python scripts/validate-content.py
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter MonsterBatchOneContentTests -testResults Temp/monster-content-red.xml -quit
```

- [ ] **Step 3: Implement the JSON/fallback content additions**

Use the approved identity/relative role to choose first-pass stats within the current power envelope. Update `GameContent.CreateDevelopmentFallback()` with the same IDs/biomes/intent families so fallback behavior cannot diverge from canonical JSON at a basic identity level.

Do not add a second `monster.crowned_slime`, `monster.skeleton_warrior`, or `monster.cave_spider` content record: those are presentation/display refinements of the stable legacy IDs.

- [ ] **Step 4: Extend content validation**

`validate-content.py` must require the four new regular IDs, verify the three compatibility IDs still exist, and reject duplicate replacement IDs that would split one canonical identity across two simulation IDs.

- [ ] **Step 5: Run GREEN and commit**

```bash
python scripts/validate-content.py
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "MonsterBatchOneContentTests|CanonicalRuntimeContentTests" -testResults Temp/monster-content-green.xml -quit
git add Assets/ClickDungeon/Content/Json/monsters.json Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs Assets/ClickDungeon/Simulation/Content/GameContent.cs Assets/ClickDungeon/Simulation/Content/Definitions.cs Assets/ClickDungeon/Tests/ApplicationEditMode/CanonicalRuntimeContentTests.cs Assets/ClickDungeon/Tests/EditMode/MonsterBatchOneContentTests.cs scripts/validate-content.py
git commit -m "feat: add regular monster batch one content"
```

### Task 3: Add deterministic encounter weighting while retaining the legacy biome pool

**Files:**
- Modify: `Assets/ClickDungeon/Simulation/Content/Definitions.cs`
- Modify: `Assets/ClickDungeon/Simulation/Content/GameContent.cs`
- Modify: `Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs`
- Modify: `Assets/ClickDungeon/Content/Json/monsters.json`
- Modify: `Assets/ClickDungeon/Simulation/Generation/FloorGenerator.cs`
- Create: `Assets/ClickDungeon/Tests/EditMode/MonsterEncounterPoolTests.cs`

**Interfaces:**
- Prefer an optional `EncounterWeight` on `MonsterDefinition` with default `100`, preserving existing content behavior.
- `GameContent.MonsterCandidatesForBiome(string biomeId)` returns deterministic ordered `(id, weight)` candidates or equivalent.
- `FloorGenerator` uses its existing floor RNG to make weighted picks.

- [ ] **Step 1: Write RED pool tests**

```csharp
[Test]
public void CavernPoolContainsLegacyAndBatchOneMonsters()
{
    var ids = content.MonsterCandidatesForBiome("biome.cavern").Select(x => x.Id).ToArray();
    Assert.That(ids, Does.Contain("monster.goblin"));
    Assert.That(ids, Does.Contain("monster.rat"));
    Assert.That(ids, Does.Contain("monster.spider"));
    Assert.That(ids, Does.Contain("monster.armored_boar"));
    Assert.That(ids, Does.Contain("monster.mimic_chest"));
}

[Test]
public void SameSeedProducesSameWeightedMonsterSequence()
{
    Assert.That(MonsterSequence(seed: 99173), Is.EqualTo(MonsterSequence(seed: 99173)));
}
```

Add an assertion that Mimic Chest's configured weight is below baseline common monsters and a broad-seed statistical sanity test that it is selectable but materially less frequent; do not assert a fragile exact percentage.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter MonsterEncounterPoolTests -testResults Temp/monster-pool-red.xml -quit
```

- [ ] **Step 3: Implement weighted candidate selection**

Keep `MonsterIdsForBiome` for compatibility if existing callers/tests depend on it; add a weighted accessor rather than silently changing its contract. Sort candidates by ID before weighted selection so JSON dictionary/order differences never affect replay determinism. Draw only from the existing floor RNG passed through generation.

- [ ] **Step 4: Run GREEN and deterministic regression tests**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "MonsterEncounterPoolTests|FloorGenerationDeterminismTests" -testResults Temp/monster-pool-green.xml -quit
```

- [ ] **Step 5: Commit**

```bash
git add Assets/ClickDungeon/Simulation/Content/Definitions.cs Assets/ClickDungeon/Simulation/Content/GameContent.cs Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs Assets/ClickDungeon/Content/Json/monsters.json Assets/ClickDungeon/Simulation/Generation/FloorGenerator.cs Assets/ClickDungeon/Tests/EditMode/MonsterEncounterPoolTests.cs
git commit -m "feat: weight monster encounters deterministically"
```

### Task 4: Convert milestone bosses to deterministic candidate pools

**Files:**
- Modify: `Assets/ClickDungeon/Simulation/Content/Definitions.cs`
- Modify: `Assets/ClickDungeon/Simulation/Content/GameContent.cs`
- Modify: `Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs`
- Modify: `Assets/ClickDungeon/Content/Json/bosses.json`
- Modify: `Assets/ClickDungeon/Content/Json/monsters.json`
- Modify: `Assets/ClickDungeon/Simulation/Generation/FloorGenerator.cs`
- Modify: `Assets/ClickDungeon/Tests/EditMode/BossGateTests.cs`
- Create: `Assets/ClickDungeon/Tests/EditMode/BossCandidatePoolTests.cs`
- Modify: `scripts/validate-content.py`

**Interfaces:**
- `GameContent.Add(BossDefinition)` no longer deletes another boss at the same floor.
- `GameContent.BossCandidatesForFloor(int floor)` returns a stable ID-sorted candidate list.
- `FloorGenerator` deterministically selects one candidate from a seed derived from root seed + run mode + milestone/floor/depth.

- [ ] **Step 1: Write RED candidate-pool tests**

```csharp
[Test]
public void ApprovedMilestonesContainLegacyAndNewBosses()
{
    Assert.That(content.BossCandidatesForFloor(10), Is.EquivalentTo(new[]{"boss.lich_sovereign","boss.goblin_brute_king"}));
    Assert.That(content.BossCandidatesForFloor(20), Is.EquivalentTo(new[]{"boss.rootbound_leviathan","boss.bat_swarm_leader"}));
    Assert.That(content.BossCandidatesForFloor(30), Is.EquivalentTo(new[]{"boss.frostbog_colossus","boss.theater_curtain_demon"}));
    Assert.That(content.BossCandidatesForFloor(40), Is.EquivalentTo(new[]{"boss.archdemon_overlord"}));
    Assert.That(content.BossCandidatesForFloor(50), Is.EquivalentTo(new[]{"boss.primal_ancient_wyrm"}));
}
```

Add tests that same root seed selects the same boss, a reasonable range of seeds reaches both candidates at floors 10/20/30, and Abyss depth 10/20/30 uses the same corresponding candidate mechanism.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter BossCandidatePoolTests -testResults Temp/boss-pool-red.xml -quit
```

- [ ] **Step 3: Change the content container before generation**

Replace the current same-floor destructive add semantics:

```csharp
public void Add(BossDefinition value)
{
    _bosses.RemoveAll(b => b.Floor == value.Floor && b.Id == value.Id);
    _bosses.Add(value);
}

public string[] BossCandidatesForFloor(int floor) =>
    _bosses.Where(b => b.Floor == floor)
           .Select(b => b.Id)
           .Distinct(StringComparer.Ordinal)
           .OrderBy(id => id, StringComparer.Ordinal)
           .ToArray();
```

Retain `BossForFloor` only as a compatibility helper if needed; new floor generation must use deterministic candidate selection explicitly.

- [ ] **Step 4: Add the three new boss content records**

Add monster definitions and boss milestone records for Goblin Brute King (10), Bat Swarm Leader (20), and Theater Curtain Demon (30). Preserve all five existing boss definitions.

- [ ] **Step 5: Implement deterministic selection in `FloorGenerator`**

Derive a dedicated boss-choice seed so adding unrelated RNG calls elsewhere does not reshuffle boss identity:

```csharp
uint bossSeed = SeedDerivation.Derive(state.RootSeed,
    $"boss:{state.Mode}:{milestoneFloor}:{state.AbyssDepth}");
int pick = new XorShift32(bossSeed).NextInt(candidates.Length);
return candidates[pick];
```

Use the campaign milestone floor for the Abyss candidate pool and retain depth in the seed so each Abyss milestone remains deterministic.

- [ ] **Step 6: Run GREEN, content validation, and boss regressions**

```bash
python scripts/validate-content.py
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "BossCandidatePoolTests|BossGateTests|BossPhaseTests" -testResults Temp/boss-pool-green.xml -quit
```

- [ ] **Step 7: Commit**

```bash
git add Assets/ClickDungeon/Simulation/Content Assets/ClickDungeon/Application/Content/JsonContentCatalogLoader.cs Assets/ClickDungeon/Content/Json/bosses.json Assets/ClickDungeon/Content/Json/monsters.json Assets/ClickDungeon/Simulation/Generation/FloorGenerator.cs Assets/ClickDungeon/Tests/EditMode/BossGateTests.cs Assets/ClickDungeon/Tests/EditMode/BossCandidatePoolTests.cs scripts/validate-content.py
git commit -m "feat: add deterministic milestone boss pools"
```

### Task 5: Fit Batch-1 behavior identity into the existing intent/combat pipeline

**Files:**
- Modify: `Assets/ClickDungeon/Content/Json/monsters.json`
- Modify: `Assets/ClickDungeon/Simulation/Combat/MonsterIntentResolver.cs` only for the smallest missing reusable primitives
- Modify: `Assets/ClickDungeon/Simulation/Combat/DamageResolver.cs` only if a deterministic supported death-burst primitive is added
- Modify: `Assets/ClickDungeon/Simulation/GameSession.cs` only when behavior resolution belongs at the session/event boundary
- Modify: `Assets/ClickDungeon/Simulation/Model/RunState.cs` / tile monster state only when reusable phase/temporary state is actually required
- Create: `Assets/ClickDungeon/Tests/EditMode/MonsterBatchOneBehaviorTests.cs`
- Modify: `Assets/ClickDungeon/Tests/EditMode/BossPhaseTests.cs` if new bosses share the existing boss-phase contract

**Interfaces:**
- Behaviors remain expressed through `ThreatPattern`, `MonsterIntentKind`, intent power, and small reusable deterministic modifiers.
- No monster gets a private random source or a UI-driven combat decision.

- [ ] **Step 1: Write one RED identity test per monster**

Minimum assertions:

```csharp
[Test]
public void SkeletonWarriorUsesDefensiveGuardIdentity()
{
    var def = content.Monster("monster.skeleton");
    Assert.That(def.PrimaryIntent, Is.EqualTo(MonsterIntentKind.Guard));
    Assert.That(def.Defense, Is.GreaterThanOrEqualTo(2));
}

[Test]
public void ArmoredBoarUsesTelegraphedHeavyAttack()
{
    var def = content.Monster("monster.armored_boar");
    Assert.That(def.PrimaryIntent, Is.EqualTo(MonsterIntentKind.HeavyAttack));
}
```

Cover all ten identities:
- Goblin Brute King: durable heavy melee boss with deterministic guard/enrage/ground-slam flavor using existing boss phase/intent primitives where possible.
- Crowned Slime: durable brute; do **not** add splitting unless occupancy/replay tests prove it can be done without destabilizing the board.
- Skeleton Warrior: defensive guard identity.
- Bat Swarm Leader: summon/swarm pressure plus deterministic attack cycle.
- Mimic Chest: surprise/chomp/greed-punisher identity; disguise is presentation/visibility behavior, not a second chest content ID.
- Fire Imp: fast fire/hazard identity; death burst is optional and may only be added if the existing damage/event pipeline can represent it deterministically and tests remain simple.
- Armored Boar: heavy telegraphed charge/trample identity.
- Spooky Spellbook: arcane/shadow ranged + summon pressure.
- Cave Spider: poison/web control using existing poison/root/control primitives where available.
- Theater Curtain Demon: magic/control boss using deterministic summon/reposition/teleport primitives already present after gameplay/tile work; do not create a separate teleport engine.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter MonsterBatchOneBehaviorTests -testResults Temp/monster-behavior-red.xml -quit
```

- [ ] **Step 3: Implement the minimum reusable behavior extensions**

Prefer data-only changes when `MonsterIntentResolver` already supports the identity. If a new primitive is necessary, make it reusable and keyed by behavior/intent state rather than by hard-coded monster display name.

Do not implement speculative mechanics merely because they appear on a sheet. In particular, slime splitting and Fire Imp death burst remain deferred unless the smallest implementation is deterministic, occupancy-safe, replay-safe, and covered by a focused test.

- [ ] **Step 4: Balance against current envelopes**

Run existing canonical/balance behavior tests and adjust only the new monsters/bosses. Do not inflate legacy monster stats to make the new art feel stronger.

- [ ] **Step 5: Run GREEN and commit**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "MonsterBatchOneBehaviorTests|CanonicalContentBehaviorTests|BossPhaseTests" -testResults Temp/monster-behavior-green.xml -quit
git add Assets/ClickDungeon/Content/Json/monsters.json Assets/ClickDungeon/Simulation/Combat Assets/ClickDungeon/Simulation/GameSession.cs Assets/ClickDungeon/Simulation/Model/RunState.cs Assets/ClickDungeon/Tests/EditMode/MonsterBatchOneBehaviorTests.cs Assets/ClickDungeon/Tests/EditMode/BossPhaseTests.cs
git commit -m "feat: integrate monster batch one combat identities"
```

### Task 6: Enforce the exact 100-PNG Batch-1 production-art contract

**Files:**
- Modify: `Assets/ClickDungeon/Editor/ProductionArtManifest.cs`
- Modify: `Assets/ClickDungeon/Editor/ProductionArtValidator.cs`
- Modify: `Assets/ClickDungeon/Editor/PixelAssetImporter.cs` only if canonical family naming is not already recognized
- Modify: `scripts/validate-assets.py`
- Modify: `scripts/validate-gameplay-art-bindings.py`
- Modify: `scripts/test-validators.py`
- Modify in place/create canonical runtime files under the current `Assets/ClickDungeon/Resources/**` production-art locations
- Preserve matching `.png.meta` files

**Interfaces:**
- Exactly ten Batch-1 families × ten variants = 100 required PNGs.
- Prefix classification matches the approved manifest contract.

- [ ] **Step 1: Add failing validator fixtures before touching art**

Fixtures must prove:
1. one missing Batch-1 variant fails;
2. a boss family named with `monster_` fails;
3. a regular family named with `boss_` fails;
4. a duplicate canonical family/variant fails;
5. a legacy unremastered monster without ten variants is still allowed through the legacy fallback policy.

- [ ] **Step 2: Run RED**

```bash
python scripts/test-validators.py
python scripts/validate-assets.py
python scripts/validate-gameplay-art-bindings.py
```

- [ ] **Step 3: Make the manifest authoritative for exactly these families**

Boss families:
- `goblin_brute_king`
- `bat_swarm_leader`
- `theater_curtain_demon`

Regular families:
- `crowned_slime`
- `skeleton_warrior`
- `mimic_chest`
- `fire_imp`
- `armored_boar`
- `spooky_spellbook`
- `cave_spider`

Required variants for every family: `master`, `portrait`, `roster`, `gameplay`, `spawn`, `idle`, `attack`, `hit`, `victory`, `defeat`.

- [ ] **Step 4: Reconcile the approved reference-sheet art into isolated runtime sprites**

Do not commit encyclopedia-sheet labels, borders, stat text, or neighboring poses into runtime textures. Preserve existing `.meta` GUIDs for any already-tracked canonical files and replace PNG bytes in place.

- [ ] **Step 5: Validate metadata and strict coverage**

```bash
python scripts/validate-unity-metadata.py
python scripts/validate-assets.py
python scripts/validate-gameplay-art-bindings.py
python scripts/test-validators.py
```

Expected: exactly 100 required Batch-1 monster/boss PNGs accepted; missing/misclassified Batch-1 art remains fatal.

- [ ] **Step 6: Run Unity import/animation generation twice and prove idempotence**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/monster-art-pass1.xml -quit
git status --short
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/monster-art-pass2.xml -quit
git status --short
```

The second pass must introduce no unexplained tracked mutations. Never weaken the mutation guard to obtain green.

- [ ] **Step 7: Commit**

```bash
git add Assets/ClickDungeon/Editor Assets/ClickDungeon/Resources scripts/validate-assets.py scripts/validate-gameplay-art-bindings.py scripts/test-validators.py
git commit -m "art: integrate monster batch one production set"
```

### Task 7: Bind live board occupants to the correct gameplay variants

**Files:**
- Modify: `Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs`
- Modify: `Assets/ClickDungeon/Presentation/Assets/MonsterPresentationAssets.cs`
- Modify: `Assets/ClickDungeon/Tests/PresentationEditMode/MonsterPresentationFamilyResolverTests.cs`
- Modify/Create: `Assets/ClickDungeon/Tests/PresentationEditMode/RuntimeGameUIHierarchyTests.cs`

**Interfaces:**
- Revealed Batch-1 monster/boss occupants use their `.gameplay` variant.
- Presentation state transitions may use `.spawn/.idle/.attack/.hit/.victory/.defeat` where the existing animation/presentation controller already supports those states.
- Legacy occupants still resolve through the existing core fallback.

- [ ] **Step 1: Add RED runtime-resolution tests**

```csharp
[TestCase("monster.slime", "monster.crowned_slime.gameplay")]
[TestCase("monster.skeleton", "monster.skeleton_warrior.gameplay")]
[TestCase("monster.spider", "monster.cave_spider.gameplay")]
[TestCase("boss.goblin_brute_king", "boss.goblin_brute_king.gameplay")]
public void BatchOneGameplayOccupantsUseCanonicalVariant(string contentId, string expectedAssetId)
{
    Assert.That(MonsterPresentationAssets.GameplayId(contentId), Is.EqualTo(expectedAssetId));
}
```

Add a legacy assertion such as `monster.goblin` continuing to resolve through its current fallback rather than Crowned Slime or Goblin Brute King art.

- [ ] **Step 2: Run RED**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "MonsterPresentationFamilyResolverTests|RuntimeGameUIHierarchyTests" -testResults Temp/monster-runtime-red.xml -quit
```

- [ ] **Step 3: Wire `RuntimeGameUI.RefreshTile` through the monster presentation resolver**

Do not make `TilePresentationAssetResolver` duplicate the family mapping. For monster/boss content, resolve the gameplay variant with `MonsterPresentationAssets`; for non-monster tile content, keep the normal tile/content resolver. Preserve threat/clue/target overlays from the gameplay/tile plan.

- [ ] **Step 4: Run GREEN and commit**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "MonsterPresentationFamilyResolverTests|RuntimeGameUIHierarchyTests|PresentationAssetIdMapperTests" -testResults Temp/monster-runtime-green.xml -quit
git add Assets/ClickDungeon/Presentation/UI/RuntimeGameUI.cs Assets/ClickDungeon/Presentation/Assets/MonsterPresentationAssets.cs Assets/ClickDungeon/Tests/PresentationEditMode
git commit -m "feat: render remastered monster gameplay variants"
```

### Task 8: Combined remaster verification, adversarial audit, and PR #5 merge gate

**Files:**
- No feature files unless an observed failing gate identifies a scoped defect.

- [ ] **Step 1: Run every engine-free/static validator**

```bash
python -m compileall -q scripts
python scripts/validate-content.py
python scripts/validate-replay.py
python scripts/validate-assets.py
python scripts/validate-gameplay-art-bindings.py
python scripts/validate-unity-metadata.py
python scripts/static-audit.py
python scripts/test-validators.py
grep -F 'm_EditorVersion: 6000.5.9f1' ProjectSettings/ProjectVersion.txt
```

- [ ] **Step 2: Run the complete Unity EditMode suite**

```bash
${UNITY_EDITOR} -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults Temp/combined-remaster-editmode.xml -quit
```

Require all hero/class, save compatibility, tile interaction/generation, regular encounter pool, boss pool, Batch-1 behavior, presentation mapping, art validation, and existing legacy regression tests to pass.

- [ ] **Step 3: Perform final runtime visual verification**

Verify the live landscape gameplay screen against `main(1).png`, then verify all ten Batch-1 identities against their approved sheets for silhouette, palette, key props/equipment, gameplay-scale readability, transparent isolation, and continuity across all ten required variants. Explicitly verify the three compatibility IDs display the new identity art while remaining their old simulation IDs.

- [ ] **Step 4: Run Option A adversarial checks**

Prove:
- legacy regular monsters still occur in eligible generated pools;
- the three new bosses coexist with rather than replace the floor 10/20/30 legacy bosses;
- floors 40/50 remain unchanged for this batch;
- same seeds reproduce identical regular encounter choices and boss identities;
- legacy unremastered presentation fallback still works;
- deleting any one required Batch-1 PNG makes validation fail.

- [ ] **Step 5: Run `/Gaudit` and `/graphRepair`**

Run only after ordinary validation is green. Treat any P0/P1/P2, dependency-graph inconsistency, non-deterministic path, stale duplicate art mapping, weakened validator, or unexplained Unity mutation as a blocker.

- [ ] **Step 6: Check GitHub CI on the exact final head**

Required normal workflows include the simulation tests, application compile/tests, presentation-contract tests, asset/content validators, and any Unity validation workflow triggered by the changed Unity code/assets. Never claim green unless the exact final commit's checks are observed green.

- [ ] **Step 7: Final PR #5 decision**

Only after the hero plan, gameplay/tile plan, and this Monster Batch 1 plan all satisfy their final gates may PR #5 leave draft status and merge. If any gate is red, repair only the observed defect and rerun the affected gate plus the final combined verification before merge.

```bash
git status --short
git log -1 --oneline
```
