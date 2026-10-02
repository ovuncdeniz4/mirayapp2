using System.IO;
using System.Linq;
using System.Text.Json;

namespace Kut.Core.Save
{
  public static class SaveStore
  {
    public static SaveData Load(string path)
    {
      if (!File.Exists(path))
      {
        return new SaveData();
      }

      try
      {
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
        Migrate(data);
        return data;
      }
      catch
      {
        var bak = path + ".bak";
        try
        {
          if (File.Exists(bak))
          {
            var json = File.ReadAllText(bak);
            var data = JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
            Migrate(data);
            return data;
          }
        }
        catch
        {
          // Both copies are unreadable. Return a clean save rather than blocking startup.
        }

        return new SaveData();
      }
    }

    public static void Save(string path, SaveData data)
    {
      var dir = Path.GetDirectoryName(path);
      if (!string.IsNullOrEmpty(dir))
      {
        Directory.CreateDirectory(dir);
      }

      var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
      var temp = path + ".tmp";
      File.WriteAllText(temp, json);
      if (File.Exists(path))
      {
        File.Copy(path, path + ".bak", true);
        File.Replace(temp, path, null);
      }
      else
      {
        File.Move(temp, path);
      }
    }

    private static void Migrate(SaveData data)
    {
      if (data.SchemaVersion < 2)
      {
        data.UnlockedCollectionIds ??= new System.Collections.Generic.List<string>();
        if (data.TotemTabUnlocked && data.TotemTier < 1)
        {
          data.TotemTier = 1;
        }

        data.SchemaVersion = 2;
      }

      if (data.SchemaVersion < 3)
      {
        data.CompletedTutorialIds ??= new System.Collections.Generic.List<string>();
        if (data.MusicVolume <= 0f && data.SfxVolume <= 0f)
        {
          data.MusicVolume = 1f;
          data.SfxVolume = 1f;
        }

        data.SchemaVersion = 3;
      }

      if (data.SchemaVersion < 4)
      {
        data.Language ??= "";
        data.SchemaVersion = 4;
      }

      data.Levels ??= new System.Collections.Generic.Dictionary<string, LevelSaveEntry>();
      data.UnlockedCollectionIds ??= new System.Collections.Generic.List<string>();
      data.CompletedTutorialIds ??= new System.Collections.Generic.List<string>();
      data.UnlockedCollectionIds = data.UnlockedCollectionIds
        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
      data.CompletedTutorialIds = data.CompletedTutorialIds
        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
      data.HighestUnlockedLevel = System.Math.Max(1, data.HighestUnlockedLevel);
      data.TotemTier = System.Math.Clamp(data.TotemTier, 0, 5);
      data.MusicVolume = System.Math.Clamp(data.MusicVolume, 0f, 1f);
      data.SfxVolume = System.Math.Clamp(data.SfxVolume, 0f, 1f);
      data.Language = data.Language == "en" ? "en" : data.Language == "tr" ? "tr" : "";
      foreach (var entry in data.Levels.Values)
      {
        entry.BestStars = System.Math.Clamp(entry.BestStars, 0, 3);
        entry.BestMovesRemaining = System.Math.Max(0, entry.BestMovesRemaining);
      }
    }
  }
}
