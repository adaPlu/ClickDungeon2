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
    }
}
