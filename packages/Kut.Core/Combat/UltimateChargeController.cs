using System;
using Kut.Core.Zodiac;

namespace Kut.Core.Combat
{
  public sealed class UltimateChargeController
  {
    public const int CapacityBasisPoints = 10000;
    private readonly ZodiacSkillController _zodiac;
    public int ChargeBasisPoints { get; private set; }
    public bool IsReady => ChargeBasisPoints >= CapacityBasisPoints;

    public UltimateChargeController(ZodiacSkillController zodiac) { _zodiac = zodiac; }

    public int AddCharge(int clearedTiles, int specialBonusBasisPoints, int cascadeIndex)
    {
      if (IsReady) return ChargeBasisPoints;
      var raw = Math.Max(0, clearedTiles) * 100 + Math.Max(0, specialBonusBasisPoints) + Math.Max(0, cascadeIndex - 1) * 25;
      ChargeBasisPoints = Math.Min(CapacityBasisPoints, ChargeBasisPoints + _zodiac.ModifyUltimateCharge(raw));
      return ChargeBasisPoints;
    }

    public bool TryConsume()
    {
      if (!IsReady) return false;
      ChargeBasisPoints = 0;
      return true;
    }
  }
}
