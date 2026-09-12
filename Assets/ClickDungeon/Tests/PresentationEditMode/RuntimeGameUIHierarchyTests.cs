using System.Reflection;
using ClickDungeon.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class RuntimeGameUIHierarchyTests
    {
        [Test]
        public void BuildsApprovedGameplayHierarchyWithTwentyFiveBoardCells()
        {
            var host=new GameObject("RuntimeGameUIHierarchyTestHost");
            try
            {
                var ui=host.AddComponent<RuntimeGameUI>();
                var build=typeof(RuntimeGameUI).GetMethod("BuildUi",BindingFlags.Instance|BindingFlags.NonPublic);
                Assert.That(build,Is.Not.Null);
                build.Invoke(ui,null);

                var canvas=host.transform.Find("ClickDungeonCanvas");
                Assert.That(canvas,Is.Not.Null);
                var safe=canvas.Find("SafeRoot");
                Assert.That(safe,Is.Not.Null);
                Assert.That(safe.Find("TopHud"),Is.Not.Null);
                Assert.That(safe.Find("PrimaryActionBar"),Is.Not.Null);
                Assert.That(safe.Find("FooterNavigation"),Is.Not.Null);

                var board=safe.Find("BoardFrame/Board");
                Assert.That(board,Is.Not.Null);
                Assert.That(board.childCount,Is.EqualTo(25));
                for(int i=0;i<25;i++)Assert.That(board.Find($"Tile_{i}"),Is.Not.Null);

                var footer=safe.Find("FooterNavigation");
                Assert.That(footer.Find("Inventory"),Is.Not.Null);
                Assert.That(footer.Find("Talents"),Is.Not.Null);
                Assert.That(footer.Find("Shop"),Is.Not.Null);
                Assert.That(footer.Find("Menu"),Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(host);
                var canvas=GameObject.Find("ClickDungeonCanvas");if(canvas!=null)Object.DestroyImmediate(canvas);
            }
        }
    }
}
