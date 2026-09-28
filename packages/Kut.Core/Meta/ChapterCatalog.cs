using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Kut.Core.Meta
{
  public sealed class ChapterDefinition
  {
    public string Id { get; set; } = "";
    public string TitleTr { get; set; } = "";
    public List<string> Levels { get; set; } = new List<string>();
    public string? RequiresChapterComplete { get; set; }
  }

  public sealed class ChapterCatalog
  {
    public List<ChapterDefinition> Chapters { get; set; } = new List<ChapterDefinition>();

    public IReadOnlyList<string> AllLevelIds =>
      Chapters.SelectMany(c => c.Levels).ToList();

    public static ChapterCatalog LoadFromFile(string path)
    {
      using var doc = JsonDocument.Parse(File.ReadAllText(path));
      var root = doc.RootElement;
      var catalog = new ChapterCatalog();
      if (!root.TryGetProperty("chapters", out var chapters) || chapters.ValueKind != JsonValueKind.Array)
      {
        return catalog;
      }

      foreach (var ch in chapters.EnumerateArray())
      {
        var def = new ChapterDefinition
        {
          Id = ch.GetProperty("id").GetString() ?? "",
          TitleTr = ch.TryGetProperty("titleTr", out var t) ? t.GetString() ?? "" : "",
          RequiresChapterComplete = ch.TryGetProperty("requiresChapterComplete", out var req)
            ? req.GetString()
            : null
        };
        if (ch.TryGetProperty("levels", out var levels) && levels.ValueKind == JsonValueKind.Array)
        {
          foreach (var lv in levels.EnumerateArray())
          {
            var id = lv.GetString();
            if (!string.IsNullOrEmpty(id))
            {
              def.Levels.Add(id);
            }
          }
        }

        catalog.Chapters.Add(def);
      }

      return catalog;
    }

    public int IndexOfLevel(string levelId)
    {
      var all = AllLevelIds;
      for (var i = 0; i < all.Count; i++)
      {
        if (all[i] == levelId)
        {
          return i;
        }
      }

      return -1;
    }

    public string? LevelIdAtGlobalIndex(int globalIndex)
    {
      var all = AllLevelIds;
      if (globalIndex < 1 || globalIndex > all.Count)
      {
        return null;
      }

      return all[globalIndex - 1];
    }

    public bool IsChapterComplete(string chapterId, Func<string, bool> isLevelComplete)
    {
      var chapter = Chapters.FirstOrDefault(c => c.Id == chapterId);
      if (chapter == null || chapter.Levels.Count == 0)
      {
        return false;
      }

      return chapter.Levels.All(isLevelComplete);
    }
  }
}
