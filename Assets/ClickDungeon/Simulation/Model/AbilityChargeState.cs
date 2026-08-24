using System;

namespace ClickDungeon.Simulation.Model
{
    [Serializable]
    public sealed class AbilityChargeState
    {
        public string AbilityId = string.Empty;
        public int Charges;
        public int RechargeProgress;

        public void GainProgress(int amount, int required, int maxCharges)
        {
            if (amount <= 0 || required <= 0 || maxCharges <= 0) return;
            if (Charges >= maxCharges) { RechargeProgress = 0; return; }
            RechargeProgress += amount;
            while (RechargeProgress >= required && Charges < maxCharges)
            {
                RechargeProgress -= required;
                Charges++;
            }
            if (Charges >= maxCharges) RechargeProgress = 0;
        }
    }
}
