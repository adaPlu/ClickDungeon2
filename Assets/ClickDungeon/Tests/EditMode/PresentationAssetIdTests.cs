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
        [TestCase("hero_ranger_victory", "hero.ranger.victory")]
        [TestCase("hero_ranger_defeat", "hero.ranger.defeat")]
        [TestCase("monster_slime_core", "monster.slime")]
        [TestCase("biome_crypt_master", "biome.crypt")]
        [TestCase("healing_potion", "item.healing_potion")]
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
