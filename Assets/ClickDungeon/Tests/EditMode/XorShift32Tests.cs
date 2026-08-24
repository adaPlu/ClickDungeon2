using NUnit.Framework;
using ClickDungeon.Simulation.Randomness;

public sealed class XorShift32Tests
{
    [Test] public void SameSeedProducesSameSequence()
    {
        var a=new XorShift32(1234); var b=new XorShift32(1234);
        for(int i=0;i<100;i++) Assert.AreEqual(a.NextUInt(),b.NextUInt());
    }
    [Test] public void ZeroSeedIsNormalized() { Assert.AreNotEqual(0u,new XorShift32(0).State); }
}
