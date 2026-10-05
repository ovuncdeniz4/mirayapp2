using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Kut.Core.Economy;
using Kut.Core.Zodiac;

namespace Kut.Core.Save
{
  public static class SaveStore
  {
    public static SaveData Load(string path)
    {
      try
      {
        return LoadCandidate(path);
      }
      catch
      {
        var bak = path + ".bak";
        try
        {
          return LoadCandidate(bak);
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

      Migrate(data);
      var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
      var checksum = ComputeChecksum(json);
      var temp = path + ".tmp";
      var checksumPath = path + ".sha256";
      var checksumTemp = checksumPath + ".tmp";
      File.WriteAllText(temp, json);
      File.WriteAllText(checksumTemp, checksum);
      if (File.Exists(path))
      {
        // Preserve only a known-good primary as backup. A corrupt primary must
        // never replace the last recoverable copy.
        try
        {
          _ = LoadCandidate(path);
          File.Copy(path, path + ".bak", true);
          File.WriteAllText(path + ".bak.sha256", ComputeChecksum(File.ReadAllText(path + ".bak")));
        }
        catch
        {
          // Keep the existing backup untouched.
        }

        ReplacePortable(temp, path);
      }
      else
      {
        File.Move(temp, path);
      }

      if (File.Exists(checksumPath)) ReplacePortable(checksumTemp, checksumPath);
      else File.Move(checksumTemp, checksumPath);
    }

    private static void ReplacePortable(string source, string destination)
    {
      try
      {
        File.Replace(source, destination, null);
      }
      catch (UnauthorizedAccessException)
      {
        // Some Unity targets and sandboxed Windows file systems do not expose
        // replace-file metadata operations. The validated backup still makes
        // this fallback recoverable if the copy is interrupted.
        File.Copy(source, destination, true);
        File.Delete(source);
      }
      catch (PlatformNotSupportedException)
      {
        File.Copy(source, destination, true);
        File.Delete(source);
      }
    }

    private static SaveData LoadCandidate(string path)
    {
      if (!File.Exists(path)) throw new FileNotFoundException("Save file does not exist.", path);
      var json = File.ReadAllText(path);
      var checksumPath = path + ".sha256";
      if (File.Exists(checksumPath))
      {
        var expected = File.ReadAllText(checksumPath).Trim();
        if (!string.Equals(expected, ComputeChecksum(json), System.StringComparison.OrdinalIgnoreCase))
          throw new InvalidDataException("Save checksum mismatch.");
      }

      var data = JsonSerializer.Deserialize<SaveData>(json) ?? throw new InvalidDataException("Save payload is empty.");
      Migrate(data);
      return data;
    }

    private static string ComputeChecksum(string value)
    {
      using var sha = SHA256.Create();
      var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
      var builder = new StringBuilder(hash.Length * 2);
      foreach (var b in hash) builder.Append(b.ToString("x2"));
      return builder.ToString();
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

      if (data.SchemaVersion < 5)
      {
        data.SelectedZodiac = ZodiacFromLegacyAnimal(data.AnimalId);
        // Existing players get one confirmation screen before this is made permanent.
        data.ZodiacSelectionComplete = false;
        data.GeneratorVersion = 1;
        data.SchemaVersion = 5;
      }

      data.Levels ??= new System.Collections.Generic.Dictionary<string, LevelSaveEntry>();
      data.UnlockedCollectionIds ??= new System.Collections.Generic.List<string>();
      data.CompletedTutorialIds ??= new System.Collections.Generic.List<string>();
      data.Boosters ??= new BoosterInventory();
      data.GeneratedLevels ??= new System.Collections.Generic.Dictionary<int, GeneratedLevelResult>();
      data.UnlockedCollectionIds = data.UnlockedCollectionIds
        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
      data.CompletedTutorialIds = data.CompletedTutorialIds
        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
      data.HighestUnlockedLevel = System.Math.Max(1, data.HighestUnlockedLevel);
      data.HighestUnlockedLevel = System.Math.Min(1000, data.HighestUnlockedLevel);
      data.Gold = System.Math.Max(0, data.Gold);
      data.UltimateUpgradeLevel = System.Math.Clamp(data.UltimateUpgradeLevel, 0, 10);
      data.GeneratorVersion = System.Math.Max(1, data.GeneratorVersion);
      data.Boosters.BlacksmithAxe = System.Math.Max(0, data.Boosters.BlacksmithAxe);
      data.Boosters.ShamanDrum = System.Math.Max(0, data.Boosters.ShamanDrum);
      data.Boosters.SkyShuffle = System.Math.Max(0, data.Boosters.SkyShuffle);
      if (!System.Enum.IsDefined(typeof(ZodiacType), data.SelectedZodiac)) data.SelectedZodiac = ZodiacType.None;
      data.TotemTier = System.Math.Clamp(data.TotemTier, 0, 5);
      data.MusicVolume = System.Math.Clamp(data.MusicVolume, 0f, 1f);
      data.SfxVolume = System.Math.Clamp(data.SfxVolume, 0f, 1f);
      data.Language = data.Language == "en" ? "en" : data.Language == "tr" ? "tr" : "";
      foreach (var entry in data.Levels.Values)
      {
        entry.BestStars = System.Math.Clamp(entry.BestStars, 0, 3);
        entry.BestMovesRemaining = System.Math.Max(0, entry.BestMovesRemaining);
      }
      foreach (var entry in data.GeneratedLevels.Values)
      {
        entry.BestStars = System.Math.Clamp(entry.BestStars, 0, 3);
        entry.BestScore = System.Math.Max(0, entry.BestScore);
        entry.BestMovesRemaining = System.Math.Max(0, entry.BestMovesRemaining);
        entry.FewestBoostersUsed = System.Math.Max(0, entry.FewestBoostersUsed);
      }
    }

    private static ZodiacType ZodiacFromLegacyAnimal(string? animalId) => animalId switch
    {
      "wolf" => ZodiacType.Bozkurt,
      "eagle" => ZodiacType.DemirKazik,
      "bear" => ZodiacType.OksuzBuga,
      "deer" => ZodiacType.AkSazan,
      "salamander" => ZodiacType.GokParsi,
      _ => ZodiacType.None
    };
  }
}
