using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kut.Core.Generation
{
  public sealed class SeasonCatalogDocument
  {
    public int Version { get; set; } = 1;
    public List<SeasonDefinition> Seasons { get; set; } = new List<SeasonDefinition>();
  }

  public static class SeasonCatalogLoader
  {
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static SeasonCatalogDocument Parse(string json)
    {
      if (string.IsNullOrWhiteSpace(json)) throw new ContentGenerationException("Season catalog is empty.");
      try
      {
        var document = JsonSerializer.Deserialize<SeasonCatalogDocument>(json, Options) ??
                       throw new ContentGenerationException("Season catalog could not be deserialized.");
        if (document.Version < 1) throw new ContentGenerationException("Season catalog version is invalid.");
        // Construction performs semantic validation and duplicate-season detection.
        _ = new LevelGenerator(document.Seasons);
        return document;
      }
      catch (ContentGenerationException) { throw; }
      catch (Exception ex) { throw new ContentGenerationException("Season catalog is malformed: " + ex.Message); }
    }

    public static SeasonCatalogDocument LoadFromFile(string path) => Parse(File.ReadAllText(path));

    private static JsonSerializerOptions CreateOptions()
    {
      var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
      options.Converters.Add(new JsonStringEnumConverter());
      return options;
    }
  }
}
