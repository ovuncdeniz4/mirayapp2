using System;
using System.IO;
using Kut.Core.Animals;
using Kut.Core.Levels;
using Kut.Core.Meta;
using Kut.Core.Save;

namespace Kut.Core.App
{
  public sealed class GameContentBundle
  {
    public string ContentRoot { get; set; } = "";
    public ChapterCatalog Chapters { get; set; } = new ChapterCatalog();
    public CollectionCatalog Collection { get; set; } = new CollectionCatalog();

    public LevelDefinition LoadLevelDefinition(string levelId) =>
      LevelLoader.LoadFromFile(GameContentPaths.LevelFile(ContentRoot, levelId));

    public static GameContentBundle LoadFromDirectory(string contentRoot)
    {
      return new GameContentBundle
      {
        ContentRoot = contentRoot,
        Chapters = ChapterCatalog.LoadFromFile(GameContentPaths.ChaptersFile(contentRoot)),
        Collection = CollectionCatalog.LoadFromFile(GameContentPaths.CollectionCatalogFile(contentRoot))
      };
    }
  }

  public static class GameShell
  {
    public static void CompleteOnboarding(SaveData save, int month, int day)
    {
      save.AnimalId = AnimalAssignment.Assign(month, day);
      save.AnimalAssignmentVersion = 1;
      save.OnboardingComplete = true;
      save.HighestUnlockedLevel = 1;
    }

    public static string ResolveContinueLevelId(SaveData save, GameContentBundle content)
    {
      var levels = content.Chapters.AllLevelIds;
      var accessible = Math.Min(save.HighestUnlockedLevel, levels.Count);
      for (var i = 0; i < accessible; i++)
      {
        if (!save.Levels.TryGetValue(levels[i], out var entry) || !entry.Completed)
        {
          return levels[i];
        }
      }

      return levels[Math.Max(0, accessible - 1)];
    }

    public static LevelSession CreateLevelSession(SaveData save, LevelDefinition def)
    {
      var session = LevelSession.FromDefinition(def);
      var bonus = AnimalBonus.BonusMovesAtLevelStart(save.AnimalId);
      if (bonus > 0)
      {
        session.Engine.AddBonusMoves(bonus);
      }

      return session;
    }

    public static void ApplyVictory(SaveData save, GameContentBundle content, string levelId, int stars = 1, int movesRemaining = 0)
    {
      MetaProgression.ApplyLevelVictory(save, content.Chapters, content.Collection, levelId, stars, movesRemaining);
    }
  }
}
