using System;
using System.Collections.Generic;

namespace Kut.Core.Economy
{
  public enum BoosterType { BlacksmithAxe, ShamanDrum, SkyShuffle }

  public sealed class BoosterInventory
  {
    public int BlacksmithAxe { get; set; }
    public int ShamanDrum { get; set; }
    public int SkyShuffle { get; set; }

    public int Get(BoosterType type) => type switch
    {
      BoosterType.BlacksmithAxe => BlacksmithAxe,
      BoosterType.ShamanDrum => ShamanDrum,
      BoosterType.SkyShuffle => SkyShuffle,
      _ => 0
    };

    public void Set(BoosterType type, int value)
    {
      value = Math.Max(0, value);
      switch (type)
      {
        case BoosterType.BlacksmithAxe: BlacksmithAxe = value; break;
        case BoosterType.ShamanDrum: ShamanDrum = value; break;
        case BoosterType.SkyShuffle: SkyShuffle = value; break;
      }
    }

    public bool TryConsume(BoosterType type)
    {
      var count = Get(type);
      if (count <= 0) return false;
      Set(type, count - 1);
      return true;
    }
  }

  public sealed class EconomyConfig
  {
    public Dictionary<BoosterType, int> BoosterPrices { get; set; } = new Dictionary<BoosterType, int>
    {
      [BoosterType.BlacksmithAxe] = 120,
      [BoosterType.ShamanDrum] = 180,
      [BoosterType.SkyShuffle] = 150
    };
  }

  public static class EconomyService
  {
    public static bool TryPurchase(ref int gold, BoosterInventory inventory, BoosterType type, EconomyConfig config)
    {
      if (!config.BoosterPrices.TryGetValue(type, out var price) || price < 0 || gold < price) return false;
      gold -= price;
      inventory.Set(type, inventory.Get(type) + 1);
      return true;
    }
  }
}
