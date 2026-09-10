using ClickDungeon.Presentation.Assets;
using NUnit.Framework;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class CanonicalDungeonTileRegistryTests
    {
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

        [TestCase("tile_floor_stone", "tile.floor.stone")]
        [TestCase("tile_floor_cracked", "tile.floor.cracked")]
        [TestCase("tile_floor_moss", "tile.floor.moss")]
        [TestCase("tile_water", "tile.water")]
        [TestCase("tile_lava", "tile.lava")]
        [TestCase("tile_shadow", "tile.shadow")]
        [TestCase("tile_trap_pit", "tile.trap.pit")]
        [TestCase("tile_trap_bomb", "tile.trap.bomb")]
        [TestCase("tile_trap_spike", "tile.trap.spike")]
        [TestCase("tile_pressure_plate", "tile.pressure_plate")]
        [TestCase("tile_teleport", "tile.teleport")]
        [TestCase("tile_fountain_heal", "tile.fountain.heal")]
        [TestCase("tile_stair_up", "tile.stair.up")]
        [TestCase("tile_stair_up_locked", "tile.stair.up.locked")]
        [TestCase("tile_stair_down", "tile.stair.down")]
        [TestCase("tile_stair_down_locked", "tile.stair.down.locked")]
        [TestCase("tile_wall", "tile.wall")]
        [TestCase("tile_wall_corner", "tile.wall.corner")]
        [TestCase("tile_key", "tile.key")]
        [TestCase("tile_chest_closed", "tile.chest.closed")]
        [TestCase("tile_chest_open", "tile.chest.open")]
        [TestCase("tile_door_locked", "tile.door.locked")]
        [TestCase("tile_door_open", "tile.door.open")]
        [TestCase("tile_torch", "tile.torch")]
        public void CanonicalTileRuntimeFilenamesMapToCanonicalIds(string file, string expected)
        {
            Assert.That(PresentationAssetIdMapper.SpriteId(file), Is.EqualTo(expected));
        }
    }
}
