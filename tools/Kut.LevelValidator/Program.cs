using System;
using System.IO;
using System.Linq;
using Kut.Core.Levels;

namespace Kut.LevelValidator
{
  internal static class Program
  {
    public static int Main(string[] args)
    {
      var levelsDir = ResolveLevelsDir(args);
      if (!Directory.Exists(levelsDir))
      {
        Console.Error.WriteLine($"Levels directory not found: {levelsDir}");
        return 2;
      }

      var files = Directory
        .GetFiles(levelsDir, "level_*.json")
        .Where(f =>
        {
          var stem = Path.GetFileNameWithoutExtension(f);
          return stem.Length == 9 && stem.StartsWith("level_0", StringComparison.Ordinal);
        })
        .OrderBy(f => f, StringComparer.Ordinal)
        .ToArray();
      if (files.Length == 0)
      {
        Console.Error.WriteLine($"No JSON levels in {levelsDir}");
        return 2;
      }

      var errorCount = 0;
      foreach (var file in files)
      {
        var errors = Kut.Core.Levels.LevelValidator.ValidateFile(file);
        if (errors.Count == 0)
        {
          Console.WriteLine($"OK  {Path.GetFileName(file)}");
          continue;
        }

        errorCount += errors.Count;
        Console.WriteLine($"FAIL {Path.GetFileName(file)}");
        foreach (var err in errors)
        {
          Console.WriteLine($"  - {err}");
        }
      }

      return errorCount == 0 ? 0 : 1;
    }

    private static string ResolveLevelsDir(string[] args)
    {
      var flag = args.FirstOrDefault(a => a.StartsWith("--dir=", StringComparison.OrdinalIgnoreCase));
      if (flag != null)
      {
        return Path.GetFullPath(flag.Substring("--dir=".Length));
      }

      return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "levels"));
    }
  }
}
