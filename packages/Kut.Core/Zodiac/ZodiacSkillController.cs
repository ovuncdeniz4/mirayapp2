using System;

namespace Kut.Core.Zodiac
{
  public enum ZodiacType
  {
    None,
    GokParsi,
    AkSazan,
    Bozkurt,
    DemirKazik,
    OksuzBuga
  }

  public enum DamageElement { Neutral, Earth, Water, Fire, Air, Metal, Spirit }
  public enum ObstacleTag { None, Freeze, WaterLock, Lava, Seal, Vine, Soil }

  public readonly struct ZodiacModifiers
  {
    public ZodiacModifiers(int fireDamageBasisPoints, int obstacleProgressBonus,
      int ultimateChargeBasisPoints, int starterAxeCount, int lavaCooldownBonus)
    {
      FireDamageBasisPoints = fireDamageBasisPoints;
      ObstacleProgressBonus = obstacleProgressBonus;
      UltimateChargeBasisPoints = ultimateChargeBasisPoints;
      StarterAxeCount = starterAxeCount;
      LavaCooldownBonus = lavaCooldownBonus;
    }

    public int FireDamageBasisPoints { get; }
    public int ObstacleProgressBonus { get; }
    public int UltimateChargeBasisPoints { get; }
    public int StarterAxeCount { get; }
    public int LavaCooldownBonus { get; }
  }

  public readonly struct DamageContext
  {
    public DamageContext(int amount, DamageElement element) { Amount = amount; Element = element; }
    public int Amount { get; }
    public DamageElement Element { get; }
  }

  public readonly struct DamageResult
  {
    public DamageResult(int baseAmount, int finalAmount) { BaseAmount = baseAmount; FinalAmount = finalAmount; }
    public int BaseAmount { get; }
    public int FinalAmount { get; }
  }

  public readonly struct LevelStartGrant
  {
    public LevelStartGrant(int temporaryAxeCount) { TemporaryAxeCount = temporaryAxeCount; }
    public int TemporaryAxeCount { get; }
  }

  public sealed class ZodiacSkillController
  {
    private readonly ZodiacType _zodiac;

    public ZodiacSkillController(ZodiacType zodiac) { _zodiac = zodiac; }

    public ZodiacModifiers GetModifiers() => GetModifiers(_zodiac);

    public static ZodiacModifiers GetModifiers(ZodiacType zodiac) => zodiac switch
    {
      ZodiacType.GokParsi => new ZodiacModifiers(11500, 0, 10000, 0, 0),
      ZodiacType.AkSazan => new ZodiacModifiers(10000, 1, 10000, 0, 0),
      ZodiacType.Bozkurt => new ZodiacModifiers(10000, 0, 11000, 0, 0),
      ZodiacType.DemirKazik => new ZodiacModifiers(10000, 0, 10000, 1, 0),
      ZodiacType.OksuzBuga => new ZodiacModifiers(10000, 0, 10000, 0, 1),
      _ => new ZodiacModifiers(10000, 0, 10000, 0, 0)
    };

    public DamageResult ModifyDamage(DamageContext context)
    {
      var amount = Math.Max(0, context.Amount);
      var bp = context.Element == DamageElement.Fire ? GetModifiers().FireDamageBasisPoints : 10000;
      return new DamageResult(amount, (amount * bp + 5000) / 10000);
    }

    public int ModifyObstacleProgress(ObstacleTag tag, int progress)
    {
      var bonus = tag == ObstacleTag.Freeze || tag == ObstacleTag.WaterLock
        ? GetModifiers().ObstacleProgressBonus : 0;
      return Math.Max(0, progress) + bonus;
    }

    public int ModifyUltimateCharge(int chargeBasisPoints) =>
      Math.Max(0, chargeBasisPoints) * GetModifiers().UltimateChargeBasisPoints / 10000;

    public LevelStartGrant GetLevelStartGrant() => new LevelStartGrant(GetModifiers().StarterAxeCount);

    public int ModifyHazardCooldown(ObstacleTag tag, int turns) =>
      Math.Max(0, turns) + (tag == ObstacleTag.Lava ? GetModifiers().LavaCooldownBonus : 0);
  }
}
