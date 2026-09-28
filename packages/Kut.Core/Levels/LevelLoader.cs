using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Kut.Core.Board;
using Kut.Core.Obstacles;
using Kut.Core.Objectives;
using Kut.Core.Tiles;

namespace Kut.Core.Levels
{
  public static class LevelLoader
  {
    public static LevelDefinition Parse(string json)
    {
      using var doc = JsonDocument.Parse(json);
      var root = doc.RootElement;

      var def = new LevelDefinition
      {
        Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? "level_unknown" : "level_unknown",
        Chapter = root.TryGetProperty("chapter", out var ch) ? ch.GetInt32() : 0,
        Moves = root.TryGetProperty("moves", out var moves) ? moves.GetInt32() : 20,
        Seed = root.TryGetProperty("seed", out var seed) ? seed.GetInt32() : 1,
        EnableWindChime = !root.TryGetProperty("enableWindChime", out var wc) || wc.GetBoolean(),
        EnableShamanDrum = root.TryGetProperty("enableShamanDrum", out var sd) && sd.GetBoolean(),
        EnableFireBomb = root.TryGetProperty("enableFireBomb", out var fb) && fb.GetBoolean()
      };

      if (root.TryGetProperty("enableSpecialCreation", out var sc))
      {
        def.EnableSpecialCreation = sc.GetBoolean();
      }
      else
      {
        def.EnableSpecialCreation = def.EnableWindChime;
      }

      if (root.TryGetProperty("boardSize", out var size))
      {
        def.BoardWidth = size.TryGetProperty("w", out var w) ? w.GetInt32() : 8;
        def.BoardHeight = size.TryGetProperty("h", out var h) ? h.GetInt32() : 8;
      }

      if (root.TryGetProperty("spawnTable", out var spawn) && spawn.ValueKind == JsonValueKind.Array)
      {
        foreach (var item in spawn.EnumerateArray())
        {
          var tileId = item.GetString();
          if (!string.IsNullOrEmpty(tileId))
          {
            def.SpawnTable.Add(tileId);
          }
        }
      }

      if (def.SpawnTable.Count == 0)
      {
        def.SpawnTable.AddRange(TileRegistry.DefaultSpawnTable);
      }

      if (root.TryGetProperty("objectives", out var objectives) && objectives.ValueKind == JsonValueKind.Array)
      {
        foreach (var obj in objectives.EnumerateArray())
        {
          var typeStr = obj.GetProperty("type").GetString() ?? "";
          def.Objectives.Add(new ObjectiveDefinition
          {
            Type = ParseObjectiveType(typeStr),
            Target = obj.GetProperty("target").GetInt32(),
            Element = obj.TryGetProperty("element", out var el) ? el.GetString() : null,
            Obstacle = obj.TryGetProperty("obstacle", out var obs) ? obs.GetString() : null,
            Special = obj.TryGetProperty("special", out var sp) ? sp.GetString() : null
          });
        }
      }

      ParseLayeredObstacleCells(root, "mudCells", def.MudCells);
      ParseLayeredObstacleCells(root, "vineCells", def.VineCells);
      ParseGridCells(root, "stoneCells", def.StoneCells);

      return def;
    }

    public static LevelDefinition LoadFromFile(string path)
    {
      return Parse(File.ReadAllText(path));
    }

    public static BoardEngine CreateEngine(LevelDefinition def)
    {
      var size = new BoardSize(def.BoardWidth, def.BoardHeight);
      var state = new BoardState(size);
      ApplyObstacles(state, def);

      var rules = new LevelRules
      {
        Moves = def.Moves,
        Seed = def.Seed,
        EnableWindChime = def.EnableWindChime,
        EnableShamanDrum = def.EnableShamanDrum,
        EnableFireBomb = def.EnableFireBomb,
        EnableSpecialCreation = def.EnableSpecialCreation
      };

      var engine = new BoardEngine(state, rules, def.SpawnTable);
      BoardGenerator.EnsureValidStart(state, new Random.DeterministicRandom(def.Seed), def.SpawnTable);
      return engine;
    }

    public static BoardEngine CreateEngineFromFile(string path)
    {
      return CreateEngine(LoadFromFile(path));
    }

    private static ObjectiveType ParseObjectiveType(string type) =>
      type switch
      {
        "make_matches" => ObjectiveType.MakeMatches,
        "cascade_depth" => ObjectiveType.CascadeDepthInTurn,
        "collect_element" => ObjectiveType.CollectElement,
        "clear_obstacle" => ObjectiveType.ClearObstacle,
        "create_special" => ObjectiveType.CreateSpecial,
        "activate_special" => ObjectiveType.ActivateSpecial,
        _ => ObjectiveType.MakeMatches
      };

    private static void ParseLayeredObstacleCells(
      JsonElement root,
      string propertyName,
      List<MudCellDefinition> target)
    {
      if (!root.TryGetProperty(propertyName, out var cells) || cells.ValueKind != JsonValueKind.Array)
      {
        return;
      }

      foreach (var cell in cells.EnumerateArray())
      {
        target.Add(new MudCellDefinition
        {
          X = cell.GetProperty("x").GetInt32(),
          Y = cell.GetProperty("y").GetInt32(),
          Layers = cell.TryGetProperty("layers", out var layers) ? layers.GetInt32() : 1
        });
      }
    }

    private static void ParseGridCells(JsonElement root, string propertyName, List<GridCellDefinition> target)
    {
      if (!root.TryGetProperty(propertyName, out var cells) || cells.ValueKind != JsonValueKind.Array)
      {
        return;
      }

      foreach (var cell in cells.EnumerateArray())
      {
        target.Add(new GridCellDefinition
        {
          X = cell.GetProperty("x").GetInt32(),
          Y = cell.GetProperty("y").GetInt32()
        });
      }
    }

    private static void ApplyObstacles(BoardState state, LevelDefinition def)
    {
      foreach (var mud in def.MudCells)
      {
        var pos = new GridPos(mud.X, mud.Y);
        if (!state.Size.Contains(pos))
        {
          throw new InvalidOperationException($"Mud cell out of bounds: ({mud.X},{mud.Y})");
        }

        state.SetCell(pos, Cell.FromObstacle(new ObstacleInstance(ObstacleType.Mud, mud.Layers)));
      }

      foreach (var vine in def.VineCells)
      {
        var pos = new GridPos(vine.X, vine.Y);
        if (!state.Size.Contains(pos))
        {
          throw new InvalidOperationException($"Vine cell out of bounds: ({vine.X},{vine.Y})");
        }

        state.SetCell(pos, Cell.FromObstacle(new ObstacleInstance(ObstacleType.Vine, vine.Layers)));
      }

      foreach (var stone in def.StoneCells)
      {
        var pos = new GridPos(stone.X, stone.Y);
        if (!state.Size.Contains(pos))
        {
          throw new InvalidOperationException($"Stone cell out of bounds: ({stone.X},{stone.Y})");
        }

        state.SetCell(pos, Cell.Blocker());
      }
    }
  }
}
