using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Kut.Core.Board;
using Kut.Core.Obstacles;
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
        EnableWindChime = !root.TryGetProperty("enableWindChime", out var wc) || wc.GetBoolean()
      };

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

      if (root.TryGetProperty("mudCells", out var mudCells) && mudCells.ValueKind == JsonValueKind.Array)
      {
        foreach (var cell in mudCells.EnumerateArray())
        {
          def.MudCells.Add(new MudCellDefinition
          {
            X = cell.GetProperty("x").GetInt32(),
            Y = cell.GetProperty("y").GetInt32(),
            Layers = cell.TryGetProperty("layers", out var layers) ? layers.GetInt32() : 1
          });
        }
      }

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
        EnableSpecialCreation = def.EnableWindChime
      };

      var engine = new BoardEngine(state, rules, def.SpawnTable);
      BoardGenerator.EnsureValidStart(state, new Random.DeterministicRandom(def.Seed), def.SpawnTable);
      return engine;
    }

    public static BoardEngine CreateEngineFromFile(string path)
    {
      return CreateEngine(LoadFromFile(path));
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
    }
  }
}
