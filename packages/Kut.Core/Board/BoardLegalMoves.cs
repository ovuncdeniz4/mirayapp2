using System.Collections.Generic;
using Kut.Core.Match;
using Kut.Core.Random;
using Kut.Core.Tiles;

namespace Kut.Core.Board
{
  public static class BoardLegalMoves
  {
    public static bool HasLegalMove(BoardState board)
    {
      return TryFindLegalSwap(board, out _, out _) || HasActivatableSpecial(board);
    }

    public static bool TryFindLegalSwap(BoardState board, out GridPos from, out GridPos to)
    {
      for (var y = 0; y < board.Size.Height; y++)
      {
        for (var x = 0; x < board.Size.Width; x++)
        {
          var at = new GridPos(x, y);
          if (IsLegalSwap(board, at, at.Offset(1, 0)) ||
              IsLegalSwap(board, at, at.Offset(0, 1)))
          {
            from = at;
            to = IsLegalSwap(board, at, at.Offset(1, 0))
              ? at.Offset(1, 0)
              : at.Offset(0, 1);
            return true;
          }
        }
      }

      from = default;
      to = default;
      return false;
    }

    public static bool TryReshuffle(BoardState board, DeterministicRandom rng, int maxAttempts = 100)
    {
      var positions = new List<GridPos>();
      var originals = new List<TileInstance>();
      for (var y = 0; y < board.Size.Height; y++)
      {
        for (var x = 0; x < board.Size.Width; x++)
        {
          var pos = new GridPos(x, y);
          var tile = board.GetCell(pos).Tile;
          if (tile != null && tile.Special == SpecialType.None)
          {
            positions.Add(pos);
            originals.Add(tile.Clone());
          }
        }
      }

      if (positions.Count < 2)
      {
        return HasActivatableSpecial(board);
      }

      for (var attempt = 0; attempt < maxAttempts; attempt++)
      {
        var shuffled = new List<TileInstance>(originals.Count);
        foreach (var tile in originals)
        {
          shuffled.Add(tile.Clone());
        }

        for (var i = shuffled.Count - 1; i > 0; i--)
        {
          var j = rng.NextInt(i + 1);
          (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        for (var i = 0; i < positions.Count; i++)
        {
          board.SetCell(positions[i], Cell.FromTile(shuffled[i]));
        }

        if (MatchDetection.FindMatchedCells(board).Count == 0 && HasLegalMove(board))
        {
          return true;
        }
      }

      for (var i = 0; i < positions.Count; i++)
      {
        board.SetCell(positions[i], Cell.FromTile(originals[i]));
      }

      return false;
    }

    public static bool TryRegenerate(
      BoardState board,
      DeterministicRandom rng,
      IReadOnlyList<string> spawnTable,
      int maxAttempts = 100)
    {
      var positions = new List<GridPos>();
      for (var y = 0; y < board.Size.Height; y++)
      {
        for (var x = 0; x < board.Size.Width; x++)
        {
          var pos = new GridPos(x, y);
          var tile = board.GetCell(pos).Tile;
          if (tile != null && tile.Special == SpecialType.None)
          {
            positions.Add(pos);
          }
        }
      }

      foreach (var pos in positions)
      {
        board.SetCell(pos, Cell.Empty());
      }

      try
      {
        BoardGenerator.EnsureValidStart(board, rng, spawnTable, maxAttempts);
        return true;
      }
      catch (System.InvalidOperationException)
      {
        return false;
      }
    }

    private static bool IsLegalSwap(BoardState board, GridPos a, GridPos b)
    {
      if (!board.CanSwap(a, b))
      {
        return false;
      }

      board.SwapTiles(a, b);
      var createsMatch = MatchDetection.FindMatchedCells(board).Count > 0;
      board.SwapTiles(a, b);
      return createsMatch;
    }

    private static bool HasActivatableSpecial(BoardState board)
    {
      for (var y = 0; y < board.Size.Height; y++)
      {
        for (var x = 0; x < board.Size.Width; x++)
        {
          var tile = board.GetCell(new GridPos(x, y)).Tile;
          if (tile != null && tile.Special != SpecialType.None)
          {
            return true;
          }
        }
      }

      return false;
    }
  }
}
