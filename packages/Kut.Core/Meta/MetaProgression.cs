using System.Collections.Generic;
using System.Linq;
using Kut.Core.Save;

namespace Kut.Core.Meta
{
  public static class MetaProgression
  {
    public static void ApplyLevelVictory(
      SaveData save,
      ChapterCatalog chapters,
      CollectionCatalog collection,
      string levelId,
      int stars = 1,
      int movesRemaining = 0)
    {
      if (!save.Levels.TryGetValue(levelId, out var entry))
      {
        entry = new LevelSaveEntry();
        save.Levels[levelId] = entry;
      }

      entry.Completed = true;
      entry.BestStars = System.Math.Max(entry.BestStars, System.Math.Clamp(stars, 1, 3));
      entry.BestMovesRemaining = System.Math.Max(entry.BestMovesRemaining, movesRemaining);

      var globalIndex = chapters.IndexOfLevel(levelId);
      if (globalIndex >= 0 && save.HighestUnlockedLevel < globalIndex + 2)
      {
        save.HighestUnlockedLevel = globalIndex + 2;
      }

      if (levelId == "level_010")
      {
        save.TotemTabUnlocked = true;
        save.CollectionTabUnlocked = true;
        save.TotemTier = System.Math.Max(save.TotemTier, 1);
      }

      if (levelId == "level_020")
      {
        save.TotemTier = System.Math.Max(save.TotemTier, 2);
        save.Chapter2Complete = true;
      }

      foreach (var item in collection.Items)
      {
        if (item.UnlockOnLevelComplete == levelId && !save.UnlockedCollectionIds.Contains(item.Id))
        {
          save.UnlockedCollectionIds.Add(item.Id);
        }
      }

      save.UnlockedCollectionIds = save.UnlockedCollectionIds.Distinct().OrderBy(x => x).ToList();
    }

    public static bool CanAccessChapter(ChapterDefinition chapter, SaveData save, ChapterCatalog catalog)
    {
      if (string.IsNullOrEmpty(chapter.RequiresChapterComplete))
      {
        return true;
      }

      return catalog.IsChapterComplete(
        chapter.RequiresChapterComplete,
        id => save.Levels.TryGetValue(id, out var e) && e.Completed);
    }
  }
}
