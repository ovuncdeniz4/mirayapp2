using System.IO;

namespace Kut.Core.App
{
  /// <summary>
  /// Resolves content root for CLI and Unity (StreamingAssets / output dir).
  /// </summary>
  public static class GameContentPaths
  {
    public static string LevelsDir(string contentRoot) => Path.Combine(contentRoot, "levels");

    public static string ChaptersFile(string contentRoot) => Path.Combine(contentRoot, "chapters.json");

    public static string CollectionCatalogFile(string contentRoot) =>
      Path.Combine(contentRoot, "collection", "catalog.json");

    public static string LevelFile(string contentRoot, string levelId) =>
      Path.Combine(LevelsDir(contentRoot), levelId + ".json");
  }
}
