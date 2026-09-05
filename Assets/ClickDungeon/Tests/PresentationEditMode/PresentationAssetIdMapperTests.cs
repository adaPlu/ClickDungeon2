using System;
using System.IO;
using ClickDungeon.Presentation.Assets;
using NUnit.Framework;

namespace ClickDungeon.Tests.PresentationEditMode
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

        [Test]
        public void ModularRoomLayoutDefinesDeterministicReusableGeometry()
        {
            var assembly=typeof(PresentationAssetIdMapper).Assembly;
            var layout=assembly.GetType("ClickDungeon.Presentation.Assets.DungeonRoomPresentationLayout");
            Assert.That(layout,Is.Not.Null,"The runtime board needs one engine-free modular room layout contract.");

            var floor=layout.GetMethod("FloorIdForCell");
            Assert.That(floor,Is.Not.Null);
            Assert.That(floor.Invoke(null,new object[]{0}),Is.EqualTo("dungeon.floor.stone"));
            Assert.That(floor.Invoke(null,new object[]{6}),Is.EqualTo("dungeon.floor.cracked"));
            Assert.That(floor.Invoke(null,new object[]{12}),Is.EqualTo("dungeon.floor.cracked"));
            Assert.That(floor.Invoke(null,new object[]{24}),Is.EqualTo("dungeon.floor.stone"));

            var edgeType=assembly.GetType("ClickDungeon.Presentation.Assets.DungeonRoomEdge");
            var wallRotation=layout.GetMethod("WallRotationDegrees");
            Assert.That(edgeType,Is.Not.Null);Assert.That(wallRotation,Is.Not.Null);
            Assert.That(wallRotation.Invoke(null,new[]{Enum.Parse(edgeType,"Top")}),Is.EqualTo(0));
            Assert.That(wallRotation.Invoke(null,new[]{Enum.Parse(edgeType,"Right")}),Is.EqualTo(90));
            Assert.That(wallRotation.Invoke(null,new[]{Enum.Parse(edgeType,"Bottom")}),Is.EqualTo(180));
            Assert.That(wallRotation.Invoke(null,new[]{Enum.Parse(edgeType,"Left")}),Is.EqualTo(270));

            var torch=layout.GetMethod("HasTorchAtCell");
            Assert.That(torch,Is.Not.Null);
            Assert.That(torch.Invoke(null,new object[]{1}),Is.EqualTo(true));
            Assert.That(torch.Invoke(null,new object[]{3}),Is.EqualTo(true));
            Assert.That(torch.Invoke(null,new object[]{2}),Is.EqualTo(false));
        }

        [Test]
        public void RuntimeBoardSourceConsumesModularRoomLayoutContract()
        {
            string root=Directory.GetCurrentDirectory();
            while(!File.Exists(Path.Combine(root,"ProjectSettings","ProjectVersion.txt"))&&Directory.GetParent(root)!=null)root=Directory.GetParent(root).FullName;
            string path=Path.Combine(root,"Assets","ClickDungeon","Presentation","UI","RuntimeGameUI.cs");
            Assert.That(File.Exists(path),Is.True);
            string source=File.ReadAllText(path);
            Assert.That(source,Does.Contain("DungeonRoomPresentationLayout.FloorIdForCell(index)"));
            Assert.That(source,Does.Contain("AddRoomDecorations"));
            Assert.That(source,Does.Contain("DungeonRoomPresentationLayout.HasTorchAtCell(index)"));
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
