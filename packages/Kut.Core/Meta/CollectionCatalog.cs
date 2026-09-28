using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Kut.Core.Meta
{
  public sealed class CollectionItemDefinition
  {
    public string Id { get; set; } = "";
    public string TitleTr { get; set; } = "";
    public string UnlockOnLevelComplete { get; set; } = "";
    public string Category { get; set; } = "relic";
  }

  public sealed class CollectionCatalog
  {
    public List<CollectionItemDefinition> Items { get; set; } = new List<CollectionItemDefinition>();

    public static CollectionCatalog LoadFromFile(string path)
    {
      using var doc = JsonDocument.Parse(File.ReadAllText(path));
      var root = doc.RootElement;
      var catalog = new CollectionCatalog();
      if (!root.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
      {
        return catalog;
      }

      foreach (var item in items.EnumerateArray())
      {
        catalog.Items.Add(new CollectionItemDefinition
        {
          Id = item.GetProperty("id").GetString() ?? "",
          TitleTr = item.TryGetProperty("titleTr", out var t) ? t.GetString() ?? "" : "",
          UnlockOnLevelComplete = item.GetProperty("unlockOnLevelComplete").GetString() ?? "",
          Category = item.TryGetProperty("category", out var c) ? c.GetString() ?? "relic" : "relic"
        });
      }

      return catalog;
    }
  }
}
