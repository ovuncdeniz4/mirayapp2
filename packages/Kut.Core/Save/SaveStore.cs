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
      return JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
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
  }
}
