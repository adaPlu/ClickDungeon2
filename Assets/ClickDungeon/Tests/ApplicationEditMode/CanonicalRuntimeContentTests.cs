using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ClickDungeon.Application.Content;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.ApplicationEditMode
{
    public sealed class CanonicalRuntimeContentTests
    {
        [Test]
        public void CanonicalJsonLoadsAndGeneratesEntireCampaign()
        {
            string contentDir=FindContentDirectory();
            GameContent content=new JsonContentCatalogLoader().LoadFromDirectory(contentDir);
            Assert.AreEqual(50,content.Balance.CampaignFloors);

            foreach(HeroClassId cls in Enum.GetValues(typeof(HeroClassId)))
            {
                var hero=content.Hero(cls);
                Assert.AreEqual(5,hero.AbilityIds.Length,$"{cls} canonical ability count");
                Assert.AreEqual(5,content.AbilitiesForClass(cls).Count(),$"{cls} loaded ability count");
            }

            var generator=new FloorGenerator(content);
            var state=generator.CreateNewRun(0xC1C1D00Du,HeroClassId.Knight);
            for(int floor=1;floor<=content.Balance.CampaignFloors;floor++)
            {
                generator.GenerateFloor(state,floor,RouteModifier.Standard);
                Assert.AreEqual(25,state.Tiles.Count,$"Floor {floor} tile count");
                Assert.IsFalse(string.IsNullOrEmpty(state.BiomeId),$"Floor {floor} biome");
                Assert.IsNotEmpty(content.MonsterIdsForBiome(state.BiomeId),$"Floor {floor} monster pool");
                string bossId=content.BossForFloor(floor);
                if(!string.IsNullOrEmpty(bossId))
                {
                    Assert.DoesNotThrow(()=>content.Monster(bossId),$"Boss definition {bossId}");
                    Assert.IsTrue(state.Tiles.Any(t=>t.Content==TileContentKind.Boss&&t.ContentId==bossId),$"Floor {floor} boss tile");
                }
            }
        }

        private static string FindContentDirectory()
        {
            string[] starts={Directory.GetCurrentDirectory(),TestContext.CurrentContext.TestDirectory};
            foreach(string start in starts)
            {
                var dir=new DirectoryInfo(start);
                for(int depth=0;depth<12&&dir!=null;depth++,dir=dir.Parent)
                {
                    string candidate=Path.Combine(dir.FullName,"Assets","ClickDungeon","Content","Json");
                    if(Directory.Exists(candidate))return candidate;
                }
            }
            throw new DirectoryNotFoundException("Could not locate Assets/ClickDungeon/Content/Json from test working directory.");
        }
    }
}
