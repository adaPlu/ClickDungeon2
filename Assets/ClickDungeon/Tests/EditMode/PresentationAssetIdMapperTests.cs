using ClickDungeon.Presentation.Assets;
using NUnit.Framework;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class PresentationAssetIdMapperTests
    {
        [TestCase("hero_knight_core","hero.knight")]
        [TestCase("monster_goblin_core","monster.goblin")]
        [TestCase("biome_crypt_master","biome.crypt")]
        [TestCase("chest_closed","chest.standard")]
        [TestCase("chest_open","chest.open")]
        [TestCase("small_key","key.small")]
        [TestCase("big_key","key.big")]
        [TestCase("trap_pitfall","trap.pitfall")]
        public void ExistingCanonicalMappingsRemainStable(string file,string expected)
        {
            Assert.That(PresentationAssetIdMapper.SpriteId(file),Is.EqualTo(expected));
        }

        [TestCase("dungeon_floor_stone","dungeon.floor.stone")]
        [TestCase("dungeon_floor_cracked","dungeon.floor.cracked")]
        [TestCase("dungeon_wall_top","dungeon.wall.top")]
        [TestCase("dungeon_wall_left","dungeon.wall.left")]
        [TestCase("dungeon_corner_tl","dungeon.corner.tl")]
        [TestCase("dungeon_corner_br","dungeon.corner.br")]
        [TestCase("dungeon_torch","dungeon.torch")]
        [TestCase("dungeon_door_locked","dungeon.door.locked")]
        [TestCase("dungeon_lock","dungeon.lock")]
        [TestCase("dungeon_shadow","dungeon.shadow")]
        [TestCase("trap_spikes","trap.spikes")]
        public void ModularDungeonContractMapsToStablePresentationIds(string file,string expected)
        {
            Assert.That(PresentationAssetIdMapper.SpriteId(file),Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("unapproved_unmapped_sprite")]
        public void UnknownOrEmptyNamesAreNotAutoRegistered(string file)
        {
            Assert.That(PresentationAssetIdMapper.SpriteId(file),Is.EqualTo(string.Empty));
        }
    }
}
