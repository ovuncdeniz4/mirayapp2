using System.IO;
using Kut.Core.App;
using Kut.Core.Meta;
using UnityEngine;

namespace Kut.Unity.App
{
  /// <summary>
  /// Loads chapter/collection/level JSON from Resources/Content (synced from repo content/).
  /// </summary>
  public static class UnityContentLoader
  {
    private const string ResourcesPrefix = "Content";

    public static GameContentBundle LoadBundle()
    {
      var chaptersText = LoadText("chapters");
      var collectionText = LoadText("collection/catalog");

      var tempRoot = Path.Combine(Application.persistentDataPath, "kut_content_cache");
      Directory.CreateDirectory(tempRoot);
      Directory.CreateDirectory(Path.Combine(tempRoot, "collection"));
      Directory.CreateDirectory(Path.Combine(tempRoot, "levels"));

      File.WriteAllText(Path.Combine(tempRoot, "chapters.json"), chaptersText);
      File.WriteAllText(Path.Combine(tempRoot, "collection", "catalog.json"), collectionText);

      SyncLevels(tempRoot);

      return GameContentBundle.LoadFromDirectory(tempRoot);
    }

    public static string LoadLevelJson(string levelId)
    {
      var asset = Resources.Load<TextAsset>($"{ResourcesPrefix}/levels/{levelId}");
      return asset != null ? asset.text : "{}";
    }

    private static void SyncLevels(string tempRoot)
    {
      var levels = Resources.LoadAll<TextAsset>($"{ResourcesPrefix}/levels");
      foreach (var lv in levels)
      {
        File.WriteAllText(Path.Combine(tempRoot, "levels", lv.name + ".json"), lv.text);
      }
    }

    private static string LoadText(string pathWithoutExtension)
    {
      var asset = Resources.Load<TextAsset>($"{ResourcesPrefix}/{pathWithoutExtension}");
      if (asset == null)
      {
        Debug.LogError($"Missing Resources/{ResourcesPrefix}/{pathWithoutExtension}");
        return "{}";
      }

      return asset.text;
    }
  }
}
