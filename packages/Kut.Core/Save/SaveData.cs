using System.Collections.Generic;

namespace Kut.Core.Save
{
  public sealed class SaveData
  {
    public int SchemaVersion { get; set; } = 1;
    public bool OnboardingComplete { get; set; }
    public string AnimalId { get; set; } = "";
    public int AnimalAssignmentVersion { get; set; } = 1;
    public int HighestUnlockedLevel { get; set; } = 1;
    public Dictionary<string, LevelSaveEntry> Levels { get; set; } = new Dictionary<string, LevelSaveEntry>();
  }

  public sealed class LevelSaveEntry
  {
    public bool Completed { get; set; }
  }
}
