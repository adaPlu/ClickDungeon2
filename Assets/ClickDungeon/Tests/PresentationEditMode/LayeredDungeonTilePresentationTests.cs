using System.IO;
using ClickDungeon.Presentation.Assets;
using ClickDungeon.Simulation.Model;
using NUnit.Framework;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class LayeredDungeonTilePresentationTests
    {
        [Test]
        public void LavaTerrainUsesLavaBaseWithoutReplacingMonsterOccupant()
        {
            var tile=new TileState{Terrain=TerrainKind.Lava,Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,Content=TileContentKind.Monster,ContentId="monster.goblin"};
            Assert.That(TilePresentationAssetResolver.BaseAssetId(tile,0),Is.EqualTo("tile.lava"));
            Assert.That(TilePresentationAssetResolver.StructuralAssetId(tile),Is.EqualTo(string.Empty));
            Assert.That(TilePresentationAssetResolver.PrimaryAssetId(tile),Is.EqualTo("monster.goblin"));
        }

        [Test]
        public void FloodedAndArcaneTerrainUseCanonicalBaseLayerOnly()
        {
            var flooded=new TileState{Terrain=TerrainKind.Flooded,Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,Content=TileContentKind.Monster,ContentId="monster.skeleton"};
            var arcane=new TileState{Terrain=TerrainKind.Arcane,Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,Content=TileContentKind.Monster,ContentId="monster.spider"};
            Assert.That(TilePresentationAssetResolver.BaseAssetId(flooded,1),Is.EqualTo("tile.water"));
            Assert.That(TilePresentationAssetResolver.BaseAssetId(arcane,2),Is.EqualTo("tile.shadow"));
            Assert.That(TilePresentationAssetResolver.PrimaryAssetId(flooded),Is.EqualTo("monster.skeleton"));
            Assert.That(TilePresentationAssetResolver.PrimaryAssetId(arcane),Is.EqualTo("monster.spider"));
        }

        [Test]
        public void StructuralLayerTracksChestAndLockedRouteResolution()
        {
            var closedChest=new TileState{Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,Content=TileContentKind.Chest,ContentId="chest.standard"};
            var openChest=new TileState{Visibility=TileVisibility.Revealed,Resolution=TileResolution.Resolved,Content=TileContentKind.Chest,ContentId="chest.standard"};
            var lockedExit=new TileState{Visibility=TileVisibility.Revealed,Resolution=TileResolution.Disabled,Content=TileContentKind.SafeExit,ContentId="exit.safe"};
            var openExit=new TileState{Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,Content=TileContentKind.SafeExit,ContentId="exit.safe"};
            Assert.That(TilePresentationAssetResolver.StructuralAssetId(closedChest),Is.EqualTo("tile.chest.closed"));
            Assert.That(TilePresentationAssetResolver.StructuralAssetId(openChest),Is.EqualTo("tile.chest.open"));
            Assert.That(TilePresentationAssetResolver.StructuralAssetId(lockedExit),Is.EqualTo("tile.stair.down.locked"));
            Assert.That(TilePresentationAssetResolver.StructuralAssetId(openExit),Is.EqualTo("tile.stair.down"));
        }

        [TestCase("trap.pitfall","tile.trap.pit")]
        [TestCase("trap.bomb","tile.trap.bomb")]
        [TestCase("trap.spike","tile.trap.spike")]
        [TestCase("trap.spikes","tile.trap.spike")]
        public void ApprovedTrapFamiliesUseCanonicalStructureVisuals(string contentId,string expected)
        {
            var tile=new TileState{Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,Content=TileContentKind.Trap,ContentId=contentId};
            Assert.That(TilePresentationAssetResolver.StructuralAssetId(tile),Is.EqualTo(expected));
        }

        [TestCase("special.pressure_plate","tile.pressure_plate")]
        [TestCase("special.teleport","tile.teleport")]
        [TestCase("special.fountain.heal","tile.fountain.heal")]
        public void ApprovedSpecialEventsUseCanonicalStructureVisuals(string contentId,string expected)
        {
            var tile=new TileState{Visibility=TileVisibility.Revealed,Resolution=TileResolution.Available,Content=TileContentKind.SpecialEvent,ContentId=contentId};
            Assert.That(TilePresentationAssetResolver.StructuralAssetId(tile),Is.EqualTo(expected));
        }

        [Test]
        public void RuntimeBoardDeclaresFourDistinctCellLayers()
        {
            string source=RuntimeBoardSource();
            Assert.That(source,Does.Contain("\"BaseTerrain\""));
            Assert.That(source,Does.Contain("\"Structure\""));
            Assert.That(source,Does.Contain("\"Content\""));
            Assert.That(source,Does.Contain("\"StateOverlay\""));
            Assert.That(source,Does.Contain("TilePresentationAssetResolver.BaseAssetId(tile,index)"));
            Assert.That(source,Does.Contain("TilePresentationAssetResolver.StructuralAssetId(tile)"));
        }

        [Test]
        public void RuntimeGameplayHudUsesPlayerFacingClickDungeonBrand()
        {
            string source=RuntimeBoardSource();
            Assert.That(source,Does.Contain("CreateText(\"Brand\",_topHud,\"ClickDungeon\""));
            Assert.That(source,Does.Not.Contain("CreateText(\"Brand\",_topHud,\"ClickDungeon2\""));
        }

        private static string RuntimeBoardSource()
        {
            string root=Directory.GetCurrentDirectory();
            while(!File.Exists(Path.Combine(root,"ProjectSettings","ProjectVersion.txt"))&&Directory.GetParent(root)!=null)root=Directory.GetParent(root).FullName;
            string path=Path.Combine(root,"Assets","ClickDungeon","Presentation","UI","RuntimeGameUI.cs");
            Assert.That(File.Exists(path),Is.True);
            return File.ReadAllText(path);
        }
    }
}
