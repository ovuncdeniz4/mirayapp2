using System.Collections.Generic;
using Kut.Core.Economy;
using Kut.Core.Zodiac;

namespace Kut.Core.Save
{
  public sealed class SaveData
  {
    public int SchemaVersion { get; set; } = 5;
    public string PlayerId { get; set; } = "";
    public string Language { get; set; } = "";
    public bool OnboardingComplete { get; set; }
    public string AnimalId { get; set; } = "";
    public int AnimalAssignmentVersion { get; set; } = 1;
    public int HighestUnlockedLevel { get; set; } = 1;
    public Dictionary<string, LevelSaveEntry> Levels { get; set; } = new Dictionary<string, LevelSaveEntry>();
    public bool TotemTabUnlocked { get; set; }
    public bool CollectionTabUnlocked { get; set; }
    public int TotemTier { get; set; }
    public bool Chapter2Complete { get; set; }
    public List<string> UnlockedCollectionIds { get; set; } = new List<string>();
    public float MusicVolume { get; set; } = 1f;
    public float SfxVolume { get; set; } = 1f;
    public bool HapticsEnabled { get; set; } = true;
    public bool ReducedMotion { get; set; }
    public List<string> CompletedTutorialIds { get; set; } = new List<string>();
    public bool CompletionSummarySeen { get; set; }
    public ZodiacType SelectedZodiac { get; set; }
    public bool ZodiacSelectionComplete { get; set; }
    public int Gold { get; set; }
    public BoosterInventory Boosters { get; set; } = new BoosterInventory();
    public int UltimateUpgradeLevel { get; set; }
    public int GeneratorVersion { get; set; } = 1;
    public bool SeasonOneEndingSeen { get; set; }
    public Dictionary<int, GeneratedLevelResult> GeneratedLevels { get; set; } = new Dictionary<int, GeneratedLevelResult>();
  }

  public sealed class LevelSaveEntry
  {
    public bool Completed { get; set; }
    public int BestStars { get; set; }
    public int BestMovesRemaining { get; set; }
  }

  public sealed class GeneratedLevelResult
  {
    public bool Completed { get; set; }
    public int BestStars { get; set; }
    public int BestScore { get; set; }
    public int BestMovesRemaining { get; set; }
    public int FewestBoostersUsed { get; set; }
  }
}
