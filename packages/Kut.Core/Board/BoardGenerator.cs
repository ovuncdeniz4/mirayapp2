using System.Collections.Generic;
using Kut.Core.Match;
using Kut.Core.Random;
using Kut.Core.Tiles;

namespace Kut.Core.Board
{
  public static class BoardGenerator
  {
    public static void FillRandom(BoardState board, DeterministicRandom rng, IReadOnlyList<string> spawnTable)
    {
      for (var x = 0; x < board.Size.Width; x++)
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          var cell = board.GetCell(new GridPos(x, y));
          if (cell.Kind != CellKind.Empty)
          {
            continue;
          }

          var tileId = spawnTable[rng.NextInt(spawnTable.Count)];
          board.SetCell(new GridPos(x, y), Cell.FromTile(TileRegistry.Create(tileId)));
        }
      }
    }

    public static void EnsureValidStart(BoardState board, DeterministicRandom rng, IReadOnlyList<string> spawnTable, int maxAttempts = 50)
    {
      for (var attempt = 0; attempt < maxAttempts; attempt++)
      {
        FillEmptyTiles(board, rng, spawnTable);
        if (MatchDetection.FindMatchedCells(board).Count == 0 && CountTiles(board) > 0)
        {
          return;
        }

        ClearTilesOnly(board);
      }

      FillEmptyTiles(board, rng, spawnTable);
    }

    private static int CountTiles(BoardState board)
    {
      var count = 0;
      for (var x = 0; x < board.Size.Width; x++)
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          if (board.GetCell(new GridPos(x, y)).Kind == CellKind.Tile)
          {
            count++;
          }
        }
      }

      return count;
    }

    private static void FillEmptyTiles(BoardState board, DeterministicRandom rng, IReadOnlyList<string> spawnTable)
    {
      for (var x = 0; x < board.Size.Width; x++)
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          var pos = new GridPos(x, y);
          if (board.GetCell(pos).Kind == CellKind.Empty)
          {
            var tileId = spawnTable[rng.NextInt(spawnTable.Count)];
            board.SetCell(pos, Cell.FromTile(TileRegistry.Create(tileId)));
          }
        }
      }
    }

    private static void ClearTilesOnly(BoardState board)
    {
      for (var x = 0; x < board.Size.Width; x++)
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          var pos = new GridPos(x, y);
          if (board.GetCell(pos).Kind == CellKind.Tile)
          {
            board.SetCell(pos, Cell.Empty());
          }
        }
      }
    }
  }
}
