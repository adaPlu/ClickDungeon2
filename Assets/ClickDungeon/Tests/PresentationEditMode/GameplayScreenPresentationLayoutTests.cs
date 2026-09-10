using ClickDungeon.Presentation.Assets;
using NUnit.Framework;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class GameplayScreenPresentationLayoutTests
    {
        [Test]
        public void LandscapeBoardIsDominantCenterRegion()
        {
            var layout=GameplayScreenPresentationLayout.Landscape;
            Assert.That(layout.Board.Width,Is.GreaterThan(0.50f));
            Assert.That(layout.Board.Height,Is.GreaterThan(0.55f));
            Assert.That(layout.Hud.MaxY,Is.EqualTo(1f));
            Assert.That(layout.ActionBar.MaxY,Is.LessThanOrEqualTo(layout.Board.MinY));
            Assert.That(layout.Footer.MinY,Is.EqualTo(0f));
            Assert.That(layout.Board.MinX,Is.GreaterThan(0f));
            Assert.That(layout.Board.MaxX,Is.LessThan(1f));
        }

        [Test]
        public void PortraitBandsRemainHudBoardActionsFooterInThatOrder()
        {
            var layout=GameplayScreenPresentationLayout.Portrait;
            Assert.That(layout.Hud.MinY,Is.GreaterThanOrEqualTo(layout.Board.MaxY));
            Assert.That(layout.Board.MinY,Is.GreaterThanOrEqualTo(layout.ActionBar.MaxY));
            Assert.That(layout.ActionBar.MinY,Is.GreaterThanOrEqualTo(layout.Footer.MaxY));
            Assert.That(layout.Footer.MinY,Is.EqualTo(0f));
            Assert.That(layout.Board.Height,Is.GreaterThan(layout.Hud.Height));
            Assert.That(layout.Board.Height,Is.GreaterThan(layout.ActionBar.Height));
        }

        [Test]
        public void BothLayoutsStayInsideNormalizedSafeRoot()
        {
            AssertInside(GameplayScreenPresentationLayout.Landscape.Hud);
            AssertInside(GameplayScreenPresentationLayout.Landscape.Board);
            AssertInside(GameplayScreenPresentationLayout.Landscape.ActionBar);
            AssertInside(GameplayScreenPresentationLayout.Landscape.Footer);
            AssertInside(GameplayScreenPresentationLayout.Portrait.Hud);
            AssertInside(GameplayScreenPresentationLayout.Portrait.Board);
            AssertInside(GameplayScreenPresentationLayout.Portrait.ActionBar);
            AssertInside(GameplayScreenPresentationLayout.Portrait.Footer);
        }

        private static void AssertInside(NormalizedRegion region)
        {
            Assert.That(region.MinX,Is.InRange(0f,1f));
            Assert.That(region.MaxX,Is.InRange(0f,1f));
            Assert.That(region.MinY,Is.InRange(0f,1f));
            Assert.That(region.MaxY,Is.InRange(0f,1f));
            Assert.That(region.Width,Is.GreaterThan(0f));
            Assert.That(region.Height,Is.GreaterThan(0f));
        }
    }
}
