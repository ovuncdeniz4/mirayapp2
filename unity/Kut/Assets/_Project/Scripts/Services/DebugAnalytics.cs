using System.IO;
using UnityEngine;

namespace Kut.Unity.Services
{
  /// <summary>Master Plan §26 — local log only.</summary>
  public static class DebugAnalytics
  {
    private static string LogPath => Path.Combine(Application.persistentDataPath, "analytics.log");

    public static void LogEvent(string name, string detail = "")
    {
      var line = $"{System.DateTime.UtcNow:o}\t{name}\t{detail}\n";
      File.AppendAllText(LogPath, line);
    }
  }
}
