using System;
using System.Collections.Generic;
using System.IO;
using Kut.Core.App;
using Kut.Core.Board;
using Kut.Core.Combat;
using Kut.Core.Economy;
using Kut.Core.Generation;
using Kut.Core.Match;
using Kut.Core.Progression;
using Kut.Core.Save;
using Kut.Core.Zodiac;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  public sealed class ErlikRebuildTests
  {
    [Test]
    public void GeneratorCoversAllOneThousandLevelsDeterministically()
    {
      var generator = new LevelGenerator(DefaultSeasonCatalog.Create());
      for (var level = 1; level <= 1000; level++)
      {
        var first = generator.Generate(level);
        var second = generator.Generate(level);
        Assert.That(first.ContentHash, Is.EqualTo(second.ContentHash));
        Assert.That(first.SeasonNumber, Is.EqualTo((level - 1) / 100 + 1));
        Assert.That(first.IsBoss, Is.EqualTo(level % 100 == 0));
      }

      var finale = generator.Generate(1000);
      Assert.That(finale.IsBoss, Is.True);
      Assert.That(DefaultSeasonCatalog.Create()[9].Boss.MaxHealth, Is.EqualTo(20000));
    }

    [Test]
    public void DataDrivenSeasonCatalogLoadsAndGeneratesFinale()
    {
      var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "content", "seasons.json");
      var catalog = SeasonCatalogLoader.LoadFromFile(Path.GetFullPath(path));
      Assert.That(catalog.Seasons, Has.Count.EqualTo(10));
      var finale = new LevelGenerator(catalog.Seasons).Generate(1000);
      Assert.That(finale.SeasonNumber, Is.EqualTo(10));
      Assert.That(catalog.Seasons[9].Boss.MaxHealth, Is.EqualTo(20000));
    }

    [Test]
    public void DrySoilRequiresWaterThenSeedThenSun()
    {
      var manager = new ObstacleProgressionManager(new[] { SeasonOneRecipes.DrySoil() });
      var obstacle = new ProgressiveObstacle("dry_soil", new GridPos(2, 3));
      Assert.That(manager.Apply(obstacle, At("seed")), Is.Null);
      Assert.That(manager.Apply(obstacle, At("water"))!.ToStateId, Is.EqualTo("wet_soil"));
      Assert.That(manager.Apply(obstacle, At("seed"))!.ToStateId, Is.EqualTo("planted_seed"));
      var final = manager.Apply(obstacle, At("sun"))!;
      Assert.That(final.Completed, Is.True);
      Assert.That(final.Effect!.Type, Is.EqualTo(GameplayEffectType.AreaBlast));

      ObstacleTrigger At(string tag) => new ObstacleTrigger(new GridPos(2, 3), TriggerScope.SameCellMatch, tag, 3);
    }

    [Test]
    public void ZodiacBonusesOnlyApplyToTheirContext()
    {
      var pars = new ZodiacSkillController(ZodiacType.GokParsi);
      Assert.That(pars.ModifyDamage(new DamageContext(100, DamageElement.Fire)).FinalAmount, Is.EqualTo(115));
      Assert.That(pars.ModifyDamage(new DamageContext(100, DamageElement.Water)).FinalAmount, Is.EqualTo(100));
      Assert.That(new ZodiacSkillController(ZodiacType.AkSazan).ModifyObstacleProgress(ObstacleTag.Freeze, 1), Is.EqualTo(2));
      Assert.That(new ZodiacSkillController(ZodiacType.Bozkurt).ModifyUltimateCharge(1000), Is.EqualTo(1100));
      Assert.That(new ZodiacSkillController(ZodiacType.DemirKazik).GetLevelStartGrant().TemporaryAxeCount, Is.EqualTo(1));
      Assert.That(new ZodiacSkillController(ZodiacType.OksuzBuga).ModifyHazardCooldown(ObstacleTag.Lava, 2), Is.EqualTo(3));
    }

    [Test]
    public void BossDamageIsIntegerDeterministicAndStopsAtZero()
    {
      var boss = new BossBattleController(new BossDefinition
      {
        Id = "test", MaxHealth = 1000, Weakness = DamageElement.Fire,
        WeaknessMultiplierBasisPoints = 12500, PhaseThresholdPercent = new List<int> { 70, 35 }
      }, new ZodiacSkillController(ZodiacType.GokParsi));
      var first = boss.ApplyDamage(new DamagePacket(100, DamageElement.Fire));
      Assert.That(first.DamageApplied, Is.EqualTo(143));
      while (!boss.State.IsDefeated) boss.ApplyDamage(new DamagePacket(500, DamageElement.Fire));
      Assert.That(boss.State.CurrentHealth, Is.Zero);
      Assert.That(boss.ApplyDamage(new DamagePacket(10, DamageElement.Fire)).DamageApplied, Is.Zero);
    }

    [Test]
    public void EconomyPurchaseIsAtomic()
    {
      var gold = 100;
      var inventory = new BoosterInventory();
      Assert.That(EconomyService.TryPurchase(ref gold, inventory, BoosterType.BlacksmithAxe, new EconomyConfig()), Is.False);
      Assert.That(gold, Is.EqualTo(100));
      Assert.That(inventory.BlacksmithAxe, Is.Zero);
    }

    [Test]
    public void SaveV4MigratesToV5WithoutCompletingZodiacConfirmation()
    {
      var path = Path.Combine(Path.GetTempPath(), $"kut_{Guid.NewGuid():N}.json");
      try
      {
        File.WriteAllText(path, "{\"SchemaVersion\":4,\"AnimalId\":\"wolf\",\"Gold\":-3,\"HighestUnlockedLevel\":5000}");
        var save = SaveStore.Load(path);
        Assert.That(save.SchemaVersion, Is.EqualTo(5));
        Assert.That(save.SelectedZodiac, Is.EqualTo(ZodiacType.Bozkurt));
        Assert.That(save.ZodiacSelectionComplete, Is.False);
        Assert.That(save.Gold, Is.Zero);
        Assert.That(save.HighestUnlockedLevel, Is.EqualTo(1000));
      }
      finally
      {
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".bak")) File.Delete(path + ".bak");
      }
    }

    [Test]
    public void LayeredCellKeepsGroundWhenMovableRuneChanges()
    {
      var board = new LayeredBoardState(new BoardSize(2, 1));
      var ground = new GroundLayerInstance { Progression = new ProgressiveObstacle("dry_soil", new GridPos(0, 0)) };
      board.GetCell(new GridPos(0, 0)).Ground = ground;
      board.GetCell(new GridPos(0, 0)).MovableTile = Rune("water", 1);
      board.GetCell(new GridPos(1, 0)).MovableTile = Rune("seed", 2);
      board.Swap(new GridPos(0, 0), new GridPos(1, 0));
      Assert.That(board.GetCell(new GridPos(0, 0)).Ground, Is.SameAs(ground));
      Assert.That(board.GetCell(new GridPos(0, 0)).MovableTile!.Definition.Id, Is.EqualTo("seed"));

      static MovableRune Rune(string id, long instance) => new MovableRune(new RuneDefinition
      { Id = id, MatchGroup = id, VisualId = "rune_" + id }, instance);
    }

    [TestCase(MatchShape.LineFour, 0,0, 1,0, 2,0, 3,0)]
    [TestCase(MatchShape.LineFiveOrMore, 0,0, 1,0, 2,0, 3,0, 4,0)]
    [TestCase(MatchShape.LShape, 0,0, 0,1, 0,2, 1,2, 2,2)]
    [TestCase(MatchShape.TShape, 0,0, 1,0, 2,0, 1,1, 1,2)]
    public void MatchClassifierRecognizesProductionShapes(MatchShape expected, params int[] coordinates)
    {
      var cells = new List<GridPos>();
      for (var i = 0; i < coordinates.Length; i += 2) cells.Add(new GridPos(coordinates[i], coordinates[i + 1]));
      Assert.That(MatchClassifier.Classify("test", cells).Shape, Is.EqualTo(expected));
    }

    [Test]
    public void UltimateCannotBeConsumedBeforeReadyAndResetsAfterUse()
    {
      var ultimate = new UltimateChargeController(new ZodiacSkillController(ZodiacType.Bozkurt));
      Assert.That(ultimate.TryConsume(), Is.False);
      ultimate.AddCharge(100, 0, 1);
      Assert.That(ultimate.IsReady, Is.True);
      Assert.That(ultimate.TryConsume(), Is.True);
      Assert.That(ultimate.ChargeBasisPoints, Is.Zero);
    }

    [Test]
    public void ChecksumFailureRecoversLastKnownGoodBackup()
    {
      var path = Path.Combine(Path.GetTempPath(), $"kut_checksum_{Guid.NewGuid():N}.json");
      try
      {
        var save = new SaveData { Gold = 40 };
        SaveStore.Save(path, save);
        save.Gold = 90;
        SaveStore.Save(path, save);
        File.WriteAllText(path, "{ corrupt }");
        Assert.That(SaveStore.Load(path).Gold, Is.EqualTo(40));
      }
      finally
      {
        foreach (var suffix in new[] { "", ".bak", ".sha256", ".bak.sha256", ".tmp", ".sha256.tmp" })
        {
          if (File.Exists(path + suffix)) File.Delete(path + suffix);
        }
      }
    }
  }
}
