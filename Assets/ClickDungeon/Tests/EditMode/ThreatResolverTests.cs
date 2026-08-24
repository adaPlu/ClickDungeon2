using NUnit.Framework;
using ClickDungeon.Simulation.Model;
using ClickDungeon.Simulation.Rules;

public sealed class ThreatResolverTests
{
    [Test] public void AdjacentThreatOnlyHitsOrthogonalNeighbor()
    {
        var source=new GridPosition(2,2);
        Assert.IsTrue(ThreatResolver.Threatens(source,new GridPosition(2,3),ThreatPattern.Adjacent));
        Assert.IsFalse(ThreatResolver.Threatens(source,new GridPosition(3,3),ThreatPattern.Adjacent));
    }
}
