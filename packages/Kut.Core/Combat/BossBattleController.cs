using System;
using System.Collections.Generic;
using Kut.Core.Zodiac;

namespace Kut.Core.Combat
{
  public sealed class BossDefinition
  {
    public string Id { get; set; } = "";
    public int MaxHealth { get; set; }
    public int DefenseBasisPoints { get; set; }
    public DamageElement Weakness { get; set; }
    public int WeaknessMultiplierBasisPoints { get; set; } = 12500;
    public List<int> PhaseThresholdPercent { get; set; } = new List<int>();
  }

  public sealed class BossState
  {
    public string BossId { get; set; } = "";
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int PhaseIndex { get; set; }
    public int TurnsTaken { get; set; }
    public bool IsDefeated => CurrentHealth <= 0;
  }

  public readonly struct DamagePacket
  {
    public DamagePacket(int baseDamage, DamageElement element, int cascadeIndex = 1, int multiplierBasisPoints = 10000)
    { BaseDamage = baseDamage; Element = element; CascadeIndex = cascadeIndex; MultiplierBasisPoints = multiplierBasisPoints; }
    public int BaseDamage { get; }
    public DamageElement Element { get; }
    public int CascadeIndex { get; }
    public int MultiplierBasisPoints { get; }
  }

  public sealed class BossDamageResult
  {
    public int DamageApplied { get; set; }
    public int HealthRemaining { get; set; }
    public bool Defeated { get; set; }
    public bool PhaseChanged { get; set; }
    public int PhaseIndex { get; set; }
  }

  public sealed class BossBattleController
  {
    private readonly BossDefinition _definition;
    private readonly ZodiacSkillController _zodiac;
    public BossState State { get; }

    public BossBattleController(BossDefinition definition, ZodiacSkillController zodiac)
    {
      if (definition.MaxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(definition.MaxHealth));
      _definition = definition;
      _zodiac = zodiac;
      State = new BossState { BossId = definition.Id, MaxHealth = definition.MaxHealth, CurrentHealth = definition.MaxHealth };
    }

    public BossDamageResult ApplyDamage(DamagePacket packet)
    {
      if (State.IsDefeated) return Result(0, false);
      var zodiacDamage = _zodiac.ModifyDamage(new DamageContext(Math.Max(0, packet.BaseDamage), packet.Element)).FinalAmount;
      var cascadeBp = 10000 + Math.Max(0, packet.CascadeIndex - 1) * 2500;
      long amount = (long)zodiacDamage * Math.Max(0, packet.MultiplierBasisPoints) / 10000;
      amount = amount * cascadeBp / 10000;
      if (packet.Element == _definition.Weakness) amount = amount * _definition.WeaknessMultiplierBasisPoints / 10000;
      amount = amount * Math.Max(0, 10000 - _definition.DefenseBasisPoints) / 10000;
      var applied = (int)Math.Min(State.CurrentHealth, Math.Max(0, amount));
      State.CurrentHealth -= applied;
      var oldPhase = State.PhaseIndex;
      while (State.PhaseIndex < _definition.PhaseThresholdPercent.Count &&
             State.CurrentHealth * 100 <= State.MaxHealth * _definition.PhaseThresholdPercent[State.PhaseIndex])
        State.PhaseIndex++;
      return Result(applied, State.PhaseIndex != oldPhase);
    }

    public void ResolveEndOfTurn()
    {
      if (!State.IsDefeated) State.TurnsTaken++;
    }

    private BossDamageResult Result(int damage, bool phaseChanged) => new BossDamageResult
    {
      DamageApplied = damage,
      HealthRemaining = State.CurrentHealth,
      Defeated = State.IsDefeated,
      PhaseChanged = phaseChanged,
      PhaseIndex = State.PhaseIndex
    };
  }
}
