using System;

namespace EbayDesk.Core {
  public static class AutoDSPricer {
    public static double RoundToCents(double value, double cents = 0.99) {
      if (cents < 0 || cents >= 1) return Math.Round(value, 2);
      var floor = Math.Floor(value);
      var below = value < floor + cents ? (floor - 1) + cents : floor + cents;
      var above = floor + 1 + cents;
      return Math.Round(Math.Abs(value - below) <= Math.Abs(above - value) ? below : above, 2);
    }

    public static double Calculate(double sourcePrice, double profitPercent, double additionalProfit, double percentFees, double fixedFees, double? cents) {
      var baseVal = sourcePrice + additionalProfit + (sourcePrice * (profitPercent / 100.0));
      var denom = 1.0 - (percentFees / 100.0);
      if (denom <= 0) throw new ArgumentException("Percent fees must be < 100%");
      var sell = (baseVal / denom) + fixedFees;
      if (cents.HasValue) sell = RoundToCents(sell, cents.Value);
      return Math.Round(sell, 2);
    }
  }
}
