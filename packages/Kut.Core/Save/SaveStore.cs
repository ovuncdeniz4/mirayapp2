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

      var json = File.ReadAllText(path);
      var data = JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
      Migrate(data);
      return data;
    }

    public static void Save(string path, SaveData data)
    {
      var dir = Path.GetDirectoryName(path);
      if (!string.IsNullOrEmpty(dir))
      {
        Directory.CreateDirectory(dir);
      }

      var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(path, json);
    }

    private static void Migrate(SaveData data)
    {
      if (data.SchemaVersion >= 2)
      {
        return;
      }

      data.UnlockedCollectionIds ??= new System.Collections.Generic.List<string>();

      if (data.TotemTabUnlocked && data.TotemTier < 1)
      {
        data.TotemTier = 1;
      }

      data.SchemaVersion = 2;
    }
  }
}
