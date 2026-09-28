using System;
using System.Collections.Generic;
using System.IO;
using Kut.Core.Board;
using Kut.Core.Objectives;
using Kut.Core.Tiles;

namespace Kut.Core.Levels
{
  /// <summary>
  /// Validates level JSON semantics before shipping content or running playtests.
  /// </summary>
  public static class LevelValidator
  {
    public static IReadOnlyList<string> ValidateFile(string path)
    {
      var errors = new List<string>();
      if (!File.Exists(path))
      {
        errors.Add($"File not found: {path}");
        return errors;
      }

      LevelDefinition def;
      try
      {
        def = LevelLoader.LoadFromFile(path);
      }
      catch (Exception ex)
      {
        errors.Add($"{Path.GetFileName(path)}: parse failed — {ex.Message}");
        return errors;
      }

      var fileStem = Path.GetFileNameWithoutExtension(path);
      errors.AddRange(ValidateDefinition(def, fileStem));
      return errors;
    }

    public static IReadOnlyList<string> ValidateDefinition(LevelDefinition def, string? expectedIdStem = null)
    {
      var errors = new List<string>();
      var prefix = string.IsNullOrEmpty(def.Id) ? "level" : def.Id;

      if (def.BoardWidth < 3 || def.BoardHeight < 3)
      {
        errors.Add($"{prefix}: boardSize must be at least 3×3");
      }

      if (def.Moves < 1)
      {
        errors.Add($"{prefix}: moves must be >= 1");
      }

      if (!string.IsNullOrEmpty(expectedIdStem) &&
          expectedIdStem.StartsWith("level_", StringComparison.Ordinal) &&
          def.Id != expectedIdStem)
      {
        errors.Add($"{prefix}: id '{def.Id}' does not match filename '{expectedIdStem}'");
      }

      foreach (var tileId in def.SpawnTable)
      {
        if (!TileRegistry.IsKnownSpawnId(tileId))
        {
          errors.Add($"{prefix}: unknown spawnTable tile '{tileId}'");
        }
      }

      ValidateObstacleList(def, def.MudCells, "mud", errors);
      ValidateObstacleList(def, def.VineCells, "vine", errors);
      ValidateStoneList(def, errors);

      if (def.Objectives.Count == 0)
      {
        errors.Add($"{prefix}: at least one objective is required");
      }

      foreach (var obj in def.Objectives)
      {
        ValidateObjective(def, obj, errors);
      }

      if (errors.Count == 0)
      {
        try
        {
          LevelLoader.CreateEngine(def);
        }
        catch (Exception ex)
        {
          errors.Add($"{prefix}: engine bootstrap failed — {ex.Message}");
        }
      }

      return errors;
    }

    private static void ValidateObstacleList(
      LevelDefinition def,
      List<MudCellDefinition> cells,
      string label,
      List<string> errors)
    {
      foreach (var cell in cells)
      {
        if (cell.Layers < 1)
        {
          errors.Add($"{def.Id}: {label} at ({cell.X},{cell.Y}) needs layers >= 1");
        }

        if (!InBounds(def, cell.X, cell.Y))
        {
          errors.Add($"{def.Id}: {label} out of bounds ({cell.X},{cell.Y})");
        }
      }
    }

    private static void ValidateStoneList(LevelDefinition def, List<string> errors)
    {
      foreach (var cell in def.StoneCells)
      {
        if (!InBounds(def, cell.X, cell.Y))
        {
          errors.Add($"{def.Id}: stone out of bounds ({cell.X},{cell.Y})");
        }
      }
    }

    private static void ValidateObjective(LevelDefinition def, ObjectiveDefinition obj, List<string> errors)
    {
      if (obj.Target < 1)
      {
        errors.Add($"{def.Id}: objective {obj.Type} target must be >= 1");
      }

      switch (obj.Type)
      {
        case ObjectiveType.CollectElement:
          if (string.IsNullOrEmpty(obj.Element))
          {
            errors.Add($"{def.Id}: collect_element requires element");
          }

          break;
        case ObjectiveType.ClearObstacle:
          if (obj.Obstacle != "mud" && obj.Obstacle != "vine")
          {
            errors.Add($"{def.Id}: clear_obstacle requires obstacle mud|vine");
          }

          break;
        case ObjectiveType.CreateSpecial:
        case ObjectiveType.ActivateSpecial:
          if (obj.Special != "wind_chime" && obj.Special != "shaman_drum")
          {
            errors.Add($"{def.Id}: special objectives require wind_chime|shaman_drum");
          }

          break;
      }
    }

    private static bool InBounds(LevelDefinition def, int x, int y) =>
      x >= 0 && y >= 0 && x < def.BoardWidth && y < def.BoardHeight;
  }
}
