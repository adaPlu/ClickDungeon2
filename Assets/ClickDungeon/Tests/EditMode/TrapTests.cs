using NUnit.Framework;
using ClickDungeon.Simulation.Content;

public sealed class TrapTests
{
    [Test] public void LaterFloorsUnlockMoreTrapKinds()
    {
        var content=GameContent.CreateDevelopmentFallback();Assert.AreEqual(2,content.TrapIdsForFloor(1).Length);Assert.Greater(content.TrapIdsForFloor(35).Length,2);
    }
}
