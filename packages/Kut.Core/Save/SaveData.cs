using System.Collections.Generic;

namespace Kut.Core.Save
{
  public sealed class SaveData
  {
    public int SchemaVersion { get; set; } = 2;
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
  }

  public sealed class LevelSaveEntry
  {
    public bool Completed { get; set; }
  }
}
