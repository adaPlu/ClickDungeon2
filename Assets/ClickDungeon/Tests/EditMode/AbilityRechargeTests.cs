using NUnit.Framework;
using ClickDungeon.Simulation.Content;
using ClickDungeon.Simulation.Model;

public sealed class AbilityRechargeTests
{
    [Test] public void RechargeDoesNotBankPastMaximum()
    {
        var content=GameContent.CreateDevelopmentFallback();
        var def=content.Ability("ability.knight.shield_wall");
        var state=new AbilityChargeState{AbilityId=def.Id,Charges=def.MaxCharges};
        state.GainProgress(999,def.RechargeProgressRequired,def.MaxCharges);
        Assert.AreEqual(def.MaxCharges,state.Charges);
        Assert.AreEqual(0,state.RechargeProgress);
    }
}
