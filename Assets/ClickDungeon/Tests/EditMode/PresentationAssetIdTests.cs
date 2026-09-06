using NUnit.Framework;
using UnityEngine;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class PresentationAssetIdTests
    {
        [TestCase("hero_ranger_core", "hero.ranger")]
        [TestCase("hero_ironheart_portrait", "hero.ironheart.portrait")]
        [TestCase("hero_ironheart_select", "hero.ironheart.select")]
        [TestCase("hero_clickington_gameplay", "hero.clickington.gameplay")]
        [TestCase("hero_ranger_portrait", "hero.ranger.portrait")]
        [TestCase("hero_ranger_roster", "hero.ranger.roster")]
        [TestCase("hero_ranger_gameplay", "hero.ranger.gameplay")]
        [TestCase("hero_ranger_master", "hero.ranger.master")]
        [TestCase("hero_ranger_idle", "hero.ranger.idle")]
        [TestCase("hero_ranger_attack", "hero.ranger.attack")]
        [TestCase("hero_ranger_hit", "hero.ranger.hit")]
        [TestCase("hero_ranger_victory", "hero.ranger.victory")]
        [TestCase("hero_ranger_defeat", "hero.ranger.defeat")]
        [TestCase("monster_slime_core", "monster.slime")]
        [TestCase("monster_crowned_slime_master", "monster.crowned_slime.master")]
        [TestCase("monster_crowned_slime_gameplay", "monster.crowned_slime.gameplay")]
        [TestCase("monster_crowned_slime_portrait", "monster.crowned_slime.portrait")]
        [TestCase("monster_crowned_slime_roster", "monster.crowned_slime.roster")]
        [TestCase("monster_crowned_slime_spawn", "monster.crowned_slime.spawn")]
        [TestCase("monster_crowned_slime_idle", "monster.crowned_slime.idle")]
        [TestCase("monster_crowned_slime_attack", "monster.crowned_slime.attack")]
        [TestCase("monster_crowned_slime_hit", "monster.crowned_slime.hit")]
        [TestCase("monster_crowned_slime_victory", "monster.crowned_slime.victory")]
        [TestCase("monster_crowned_slime_defeat", "monster.crowned_slime.defeat")]
        [TestCase("monster_crowned_slime_split", "monster.crowned_slime.split")]
        [TestCase("boss_theater_curtain_demon_teleport", "boss.theater_curtain_demon.teleport")]
        [TestCase("dungeon_floor_stone", "dungeon.floor.stone")]
        [TestCase("dungeon_floor_cracked", "dungeon.floor.cracked")]
        [TestCase("dungeon_wall_top", "dungeon.wall.top")]
        [TestCase("dungeon_corner_tl", "dungeon.corner.tl")]
        [TestCase("dungeon_torch", "dungeon.torch")]
        [TestCase("dungeon_door_locked", "dungeon.door.locked")]
        [TestCase("dungeon_lock", "dungeon.lock")]
        [TestCase("dungeon_shadow", "dungeon.shadow")]
        [TestCase("trap_spikes", "trap.spikes")]
        [TestCase("biome_crypt_master", "biome.crypt")]
        [TestCase("healing_potion", "item.healing_potion")]
        [TestCase("iron_sword", "item.iron_sword")]
        public void RuntimeArtFileNamesMapToStablePresentationIds(string fileName, string expected)
        {
            Assert.That(PresentationAssetId.FromRuntimeArtFile(fileName), Is.EqualTo(expected));
        }

        [TestCase(HeroClassId.Knight, "ironheart")]
        [TestCase(HeroClassId.Ranger, "windsong")]
        [TestCase(HeroClassId.Thief, "shadowcut")]
        [TestCase(HeroClassId.Wizard, "emberwisp")]
        public void StandardClassesResolveToApprovedHeroIdentities(HeroClassId heroClass, string expectedHeroId)
        {
            Assert.That(HeroPresentationAssets.StandardHeroId(heroClass), Is.EqualTo(expectedHeroId));
            Assert.That(HeroPresentationAssets.BaseId(heroClass), Is.EqualTo("hero." + expectedHeroId));
        }

        [Test]
        public void HeroPortraitPrefersApprovedIdentityThenFallsBackToLegacyClassCore()
        {
            var db = ScriptableObject.CreateInstance<PresentationAssetDatabase>();
            var coreTexture = new Texture2D(2, 2);
            var portraitTexture = new Texture2D(2, 2);
            var core = Sprite.Create(coreTexture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));
            var portrait = Sprite.Create(portraitTexture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));

            try
            {
                db.Replace(
                    new[]
                    {
                        new PresentationAssetDatabase.SpriteEntry { Id = "hero.ranger", Sprite = core },
                        new PresentationAssetDatabase.SpriteEntry { Id = "hero.windsong.portrait", Sprite = portrait }
                    },
                    null);

                Assert.That(HeroPresentationAssets.Portrait(db, HeroClassId.Ranger), Is.SameAs(portrait));

                db.Replace(
                    new[] { new PresentationAssetDatabase.SpriteEntry { Id = "hero.ranger", Sprite = core } },
                    null);

                Assert.That(HeroPresentationAssets.Portrait(db, HeroClassId.Ranger), Is.SameAs(core));
            }
            finally
            {
                Object.DestroyImmediate(core);
                Object.DestroyImmediate(portrait);
                Object.DestroyImmediate(coreTexture);
                Object.DestroyImmediate(portraitTexture);
                Object.DestroyImmediate(db);
            }
        }

        [Test]
        public void ClickingtonCanUseKnightMechanicsWithoutUsingIronheartArt()
        {
            var db = ScriptableObject.CreateInstance<PresentationAssetDatabase>();
            var knightTexture = new Texture2D(2, 2);
            var ironheartTexture = new Texture2D(2, 2);
            var clickingtonTexture = new Texture2D(2, 2);
            var knight = Sprite.Create(knightTexture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));
            var ironheart = Sprite.Create(ironheartTexture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));
            var clickington = Sprite.Create(clickingtonTexture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));

            try
            {
                db.Replace(
                    new[]
                    {
                        new PresentationAssetDatabase.SpriteEntry { Id = "hero.knight", Sprite = knight },
                        new PresentationAssetDatabase.SpriteEntry { Id = "hero.ironheart.portrait", Sprite = ironheart },
                        new PresentationAssetDatabase.SpriteEntry { Id = "hero.clickington.portrait", Sprite = clickington }
                    },
                    null);

                Assert.That(HeroPresentationAssets.Portrait(db, HeroClassId.Knight), Is.SameAs(ironheart));
                Assert.That(HeroPresentationAssets.Portrait(db, "clickington", HeroClassId.Knight), Is.SameAs(clickington));
            }
            finally
            {
                Object.DestroyImmediate(knight);
                Object.DestroyImmediate(ironheart);
                Object.DestroyImmediate(clickington);
                Object.DestroyImmediate(knightTexture);
                Object.DestroyImmediate(ironheartTexture);
                Object.DestroyImmediate(clickingtonTexture);
                Object.DestroyImmediate(db);
            }
        }

        [Test]
        public void MonsterStatePrefersDedicatedStateThenFallsBackToCore()
        {
            var db = ScriptableObject.CreateInstance<PresentationAssetDatabase>();
            var coreTexture = new Texture2D(2, 2);
            var attackTexture = new Texture2D(2, 2);
            var core = Sprite.Create(coreTexture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));
            var attack = Sprite.Create(attackTexture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));

            try
            {
                db.Replace(
                    new[]
                    {
                        new PresentationAssetDatabase.SpriteEntry { Id = "monster.crowned_slime", Sprite = core },
                        new PresentationAssetDatabase.SpriteEntry { Id = "monster.crowned_slime.attack", Sprite = attack }
                    },
                    null);

                Assert.That(MonsterPresentationAssets.Attack(db, "crowned_slime"), Is.SameAs(attack));

                db.Replace(
                    new[] { new PresentationAssetDatabase.SpriteEntry { Id = "monster.crowned_slime", Sprite = core } },
                    null);

                Assert.That(MonsterPresentationAssets.Attack(db, "crowned_slime"), Is.SameAs(core));
            }
            finally
            {
                Object.DestroyImmediate(core);
                Object.DestroyImmediate(attack);
                Object.DestroyImmediate(coreTexture);
                Object.DestroyImmediate(attackTexture);
                Object.DestroyImmediate(db);
            }
        }
    }
}
