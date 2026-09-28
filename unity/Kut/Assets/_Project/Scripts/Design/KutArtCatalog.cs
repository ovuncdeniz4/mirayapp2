using Kut.Core.Obstacles;
using Kut.Core.Tiles;
using UnityEngine;

namespace Kut.Unity.Design
{
  /// <summary>
  /// Loads final art from Resources/Art/{category}/{spriteId} per docs/ART_DIRECTION.md.
  /// Falls back to null so callers keep token colors.
  /// </summary>
  public static class KutArtCatalog
  {
    private const string Root = "Art";

    public static Sprite? TryTile(string tileId)
    {
      var spriteId = tileId switch
      {
        "earth_moss" => "tile_earth_moss",
        "water_drop" => "tile_water_drop",
        "fire_ember" => "tile_fire_ember",
        "wind_gust" => "tile_air_wisp",
        "metal_ingot" => "tile_metal_ingot",
        _ => null
      };

      return spriteId == null ? null : Load("Tiles", spriteId);
    }

    public static Sprite? TrySpecial(TileInstance tile)
    {
      if (tile.Special == SpecialType.WindChime)
      {
        return Load("Specials", "special_wind_chime");
      }

      if (tile.Special == SpecialType.FireBomb)
      {
        return Load("Specials", "special_fire_bomb");
      }

      if (tile.Special == SpecialType.ShamanDrum)
      {
        var id = tile.ResonanceMatchGroup == "water"
          ? "special_shaman_drum_water"
          : "special_shaman_drum_earth";
        return Load("Specials", id);
      }

      return null;
    }

    public static Sprite? TryObstacle(ObstacleType type) =>
      type switch
      {
        ObstacleType.Mud => Load("Obstacles", "obstacle_mud"),
        ObstacleType.Vine => Load("Obstacles", "obstacle_vine"),
        _ => null
      };

    public static Sprite? TryBlocker() => Load("Obstacles", "obstacle_stone");

    public static Sprite? TryAnimal(string animalId) =>
      Load("Animals", $"animal_{animalId}");

    public static Sprite? TryTotemStage(int stage) =>
      Load("Totem", $"totem_stage_{Mathf.Clamp(stage, 1, 5)}");

    public static Sprite? TryRelic(string relicId) => Load("Relics", relicId);

    public static Sprite? TryBoardTray() => Load("UI", "ui_board_tray");

    private static Sprite? Load(string category, string spriteId)
    {
      var path = $"{Root}/{category}/{spriteId}";
      return Resources.Load<Sprite>(path);
    }
  }
}
