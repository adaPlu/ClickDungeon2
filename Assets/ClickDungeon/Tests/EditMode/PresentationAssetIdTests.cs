using NUnit.Framework;
using UnityEngine;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class PresentationAssetIdTests
    {
        [TestCase("hero_ranger_core", "hero.ranger")]
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
        [TestCase("biome_crypt_master", "biome.crypt")]
        [TestCase("healing_potion", "item.healing_potion")]
        [TestCase("iron_sword", "item.iron_sword")]
        public void RuntimeArtFileNamesMapToStablePresentationIds(string fileName, string expected)
        {
            Assert.That(PresentationAssetId.FromRuntimeArtFile(fileName), Is.EqualTo(expected));
        }

        [Test]
        public void HeroPortraitPrefersDedicatedPortraitThenFallsBackToCore()
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
                        new PresentationAssetDatabase.SpriteEntry { Id = "hero.ranger.portrait", Sprite = portrait }
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
    }
}
