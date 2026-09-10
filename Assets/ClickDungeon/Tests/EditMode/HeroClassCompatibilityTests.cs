using NUnit.Framework;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Tests.EditMode
{
    public sealed class HeroClassCompatibilityTests
    {
        [Test]
        public void HeroClassOrdinalsRemainBackwardCompatible()
        {
            Assert.That((int)HeroClassId.Knight, Is.EqualTo(0));
            Assert.That((int)HeroClassId.Ranger, Is.EqualTo(1));
            Assert.That((int)HeroClassId.Thief, Is.EqualTo(2));
            Assert.That((int)HeroClassId.Wizard, Is.EqualTo(3));
            Assert.That((int)HeroClassId.Paladin, Is.EqualTo(4));
            Assert.That((int)HeroClassId.Berserker, Is.EqualTo(5));
            Assert.That((int)HeroClassId.Engineer, Is.EqualTo(6));
            Assert.That((int)HeroClassId.Cleric, Is.EqualTo(7));
        }
    }
}
