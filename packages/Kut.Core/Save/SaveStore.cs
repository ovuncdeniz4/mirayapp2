using System.IO;
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
        if (File.Exists(bak))
        {
          var json = File.ReadAllText(bak);
          var data = JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
          Migrate(data);
          return data;
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

      if (File.Exists(path))
      {
        File.Copy(path, path + ".bak", true);
      }

      var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(path, json);
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
    }
  }
}
