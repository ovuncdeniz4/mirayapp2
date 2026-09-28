using System.Collections.Generic;

namespace Kut.Core.Tiles
{
  public static class TileRegistry
  {
    private static readonly Dictionary<string, TileInstance> Prototypes = new Dictionary<string, TileInstance>
    {
      ["earth_moss"] = new TileInstance("earth_moss", Element.Earth, "earth"),
      ["water_drop"] = new TileInstance("water_drop", Element.Water, "water"),
      ["fire_ember"] = new TileInstance("fire_ember", Element.Fire, "fire"),
      ["wind_gust"] = new TileInstance("wind_gust", Element.Air, "wind")
    };

    public static TileInstance Create(string tileId)
    {
      if (!Prototypes.TryGetValue(tileId, out var proto))
      {
        throw new KeyNotFoundException($"Unknown tile: {tileId}");
      }

      return proto.Clone();
    }

    public static IReadOnlyList<string> DefaultSpawnTable => new[] { "earth_moss", "water_drop" };

    public static bool IsKnownSpawnId(string tileId) => Prototypes.ContainsKey(tileId);
  }
}
