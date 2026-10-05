using System;
using System.Collections.Generic;
using Kut.Core.Combat;
using Kut.Core.Random;
using Kut.Core.Zodiac;

namespace Kut.Core.Generation
{
  public sealed class DifficultyCurve
  {
    public int BaseMoves { get; set; } = 24;
    public int BaseObjectiveTarget { get; set; } = 10;
    public int ObjectiveGrowthPerBand { get; set; } = 2;
  }

  public sealed class LevelArchetype
  {
    public string Id { get; set; } = "collect_runes";
    public int Weight { get; set; } = 1;
    public string ObjectiveType { get; set; } = "collect_runes";
    public string MechanicId { get; set; } = "";
  }

  public sealed class SeasonDefinition
  {
    public int SeasonNumber { get; set; }
    public int FirstLevel { get; set; }
    public int LastLevel { get; set; }
    public string NameKey { get; set; } = "";
    public string MapVisualSetId { get; set; } = "";
    public string GameplayVisualSetId { get; set; } = "";
    public string MusicId { get; set; } = "";
    public List<string> TilePool { get; set; } = new List<string>();
    public List<string> MechanicIds { get; set; } = new List<string>();
    public List<LevelArchetype> Archetypes { get; set; } = new List<LevelArchetype>();
    public DifficultyCurve Difficulty { get; set; } = new DifficultyCurve();
    public BossDefinition Boss { get; set; } = new BossDefinition();
  }

  public sealed class GeneratedLevel
  {
    public int LevelNumber { get; set; }
    public int SeasonNumber { get; set; }
    public int Band { get; set; }
    public int Seed { get; set; }
    public int GeneratorVersion { get; set; }
    public int Width { get; set; } = 8;
    public int Height { get; set; } = 8;
    public int Moves { get; set; }
    public int ObjectiveTarget { get; set; }
    public string ArchetypeId { get; set; } = "";
    public string ObjectiveType { get; set; } = "";
    public string MechanicId { get; set; } = "";
    public bool IsBoss { get; set; }
    public string ContentHash { get; set; } = "";
  }

  public sealed class ContentGenerationException : Exception
  {
    public ContentGenerationException(string message) : base(message) { }
  }

  public sealed class LevelGenerator
  {
    private readonly Dictionary<int, SeasonDefinition> _seasons = new Dictionary<int, SeasonDefinition>();

    public LevelGenerator(IEnumerable<SeasonDefinition> seasons)
    {
      foreach (var season in seasons)
      {
        ValidateSeason(season);
        _seasons.Add(season.SeasonNumber, season);
      }
    }

    public GeneratedLevel Generate(int levelNumber, int generatorVersion = 1)
    {
      if (levelNumber < 1 || levelNumber > 1000) throw new ContentGenerationException("Level number must be between 1 and 1000.");
      var seasonNumber = (levelNumber - 1) / 100 + 1;
      if (!_seasons.TryGetValue(seasonNumber, out var season)) throw new ContentGenerationException($"Season {seasonNumber} is missing.");
      var inSeason = (levelNumber - 1) % 100 + 1;
      var band = (inSeason - 1) / 10 + 1;
      var seed = StableSeed(levelNumber, generatorVersion);
      var rng = new DeterministicRandom(seed);
      var archetype = PickWeighted(season.Archetypes, rng);
      var generated = new GeneratedLevel
      {
        LevelNumber = levelNumber,
        SeasonNumber = seasonNumber,
        Band = band,
        Seed = seed,
        GeneratorVersion = generatorVersion,
        Moves = Math.Max(8, season.Difficulty.BaseMoves - (band - 1) / 2),
        ObjectiveTarget = season.Difficulty.BaseObjectiveTarget + (band - 1) * season.Difficulty.ObjectiveGrowthPerBand,
        ArchetypeId = archetype.Id,
        ObjectiveType = archetype.ObjectiveType,
        MechanicId = archetype.MechanicId,
        IsBoss = inSeason == 100
      };
      generated.ContentHash = Fnv1a($"{levelNumber}|{generatorVersion}|{generated.Seed}|{generated.ArchetypeId}|{generated.Moves}|{generated.ObjectiveTarget}").ToString("x8");
      return generated;
    }

    private static LevelArchetype PickWeighted(List<LevelArchetype> items, DeterministicRandom rng)
    {
      var total = 0;
      foreach (var item in items) total += Math.Max(0, item.Weight);
      if (total <= 0) throw new ContentGenerationException("Season has no weighted level archetype.");
      var roll = rng.NextInt(total);
      foreach (var item in items)
      {
        roll -= Math.Max(0, item.Weight);
        if (roll < 0) return item;
      }
      return items[items.Count - 1];
    }

    private static void ValidateSeason(SeasonDefinition season)
    {
      if (season.SeasonNumber < 1 || season.SeasonNumber > 10) throw new ContentGenerationException("Season number must be 1-10.");
      if (season.FirstLevel != (season.SeasonNumber - 1) * 100 + 1 || season.LastLevel != season.SeasonNumber * 100)
        throw new ContentGenerationException($"Season {season.SeasonNumber} has an invalid level range.");
      if (season.TilePool.Count < 4) throw new ContentGenerationException($"Season {season.SeasonNumber} requires at least four tiles.");
      if (season.Archetypes.Count == 0) throw new ContentGenerationException($"Season {season.SeasonNumber} requires archetypes.");
      if (season.Boss.MaxHealth <= 0) throw new ContentGenerationException($"Season {season.SeasonNumber} requires a boss.");
    }

    private static int StableSeed(int level, int version) => unchecked((level * 73856093) ^ (version * 19349663) ^ 0x4B5554);
    private static uint Fnv1a(string value)
    {
      uint hash = 2166136261;
      foreach (var c in value) { hash ^= c; hash *= 16777619; }
      return hash;
    }
  }
}
