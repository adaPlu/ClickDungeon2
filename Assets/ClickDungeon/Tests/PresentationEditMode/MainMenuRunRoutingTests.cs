using NUnit.Framework;
using ClickDungeon.Presentation.Menu;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class MainMenuRunRoutingTests
    {
        [Test]
        public void MetadataOnlySlotCannotContinueWithoutActiveRun()
        {
            Assert.IsFalse(MainMenuRunRouting.CanContinue(null));
        }

        [Test]
        public void SlotWithActiveRunCanContinue()
        {
            Assert.IsTrue(MainMenuRunRouting.CanContinue(new RunState()));
        }

        [Test]
        public void PlayAlwaysStartsNewRunFlow()
        {
            Assert.AreEqual(MainMenuRunAction.NewRun,MainMenuRunRouting.PlayAction);
        }
    }
}
