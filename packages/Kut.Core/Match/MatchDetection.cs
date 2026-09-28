using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.Tiles;

namespace Kut.Core.Match
{
  public static class MatchDetection
  {
    public static HashSet<GridPos> FindMatchedCells(BoardState board)
    {
      var matched = new HashSet<GridPos>();

      for (var y = 0; y < board.Size.Height; y++)
      {
        ScanLine(board, matched, horizontal: true, fixedIndex: y);
      }

      for (var x = 0; x < board.Size.Width; x++)
      {
        ScanLine(board, matched, horizontal: false, fixedIndex: x);
      }

      return matched;
    }

    private static void ScanLine(BoardState board, HashSet<GridPos> matched, bool horizontal, int fixedIndex)
    {
      var length = horizontal ? board.Size.Width : board.Size.Height;
      var runGroup = "";
      var runStart = 0;
      var runLength = 0;

      void flushRun()
      {
        if (runLength >= 3 && runGroup.Length > 0)
        {
          for (var i = 0; i < runLength; i++)
          {
            var idx = runStart + i;
            var pos = horizontal ? new GridPos(idx, fixedIndex) : new GridPos(fixedIndex, idx);
            matched.Add(pos);
          }
        }
      }

      for (var i = 0; i < length; i++)
      {
        var pos = horizontal ? new GridPos(i, fixedIndex) : new GridPos(fixedIndex, i);
        var group = board.GetMatchGroup(pos);
        if (group == null)
        {
          flushRun();
          runGroup = "";
          runLength = 0;
          continue;
        }

        if (group == runGroup)
        {
          runLength++;
        }
        else
        {
          flushRun();
          runGroup = group;
          runStart = i;
          runLength = 1;
        }
      }

      flushRun();
    }

    public static bool TryFindLineOfFive(BoardState board, HashSet<GridPos> cluster, out LineOrientation orientation)
    {
      orientation = LineOrientation.Horizontal;
      foreach (var pos in cluster)
      {
        if (IsStraightRun(board, cluster, pos, horizontal: true, length: 5))
        {
          orientation = LineOrientation.Horizontal;
          return true;
        }

        if (IsStraightRun(board, cluster, pos, horizontal: false, length: 5))
        {
          orientation = LineOrientation.Vertical;
          return true;
        }
      }

      return false;
    }

    public static bool TryFindLineOfFour(BoardState board, HashSet<GridPos> cluster, out LineOrientation orientation)
    {
      orientation = LineOrientation.Horizontal;

      foreach (var pos in cluster)
      {
        if (IsStraightRun(board, cluster, pos, horizontal: true, length: 4))
        {
          orientation = LineOrientation.Horizontal;
          return true;
        }

        if (IsStraightRun(board, cluster, pos, horizontal: false, length: 4))
        {
          orientation = LineOrientation.Vertical;
          return true;
        }
      }

      return false;
    }

    private static bool IsStraightRun(
      BoardState board,
      HashSet<GridPos> cluster,
      GridPos start,
      bool horizontal,
      int length)
    {
      var group = board.GetMatchGroup(start);
      if (group == null)
      {
        return false;
      }

      for (var i = 0; i < length; i++)
      {
        var pos = horizontal ? new GridPos(start.X + i, start.Y) : new GridPos(start.X, start.Y + i);
        if (!board.Size.Contains(pos) || !cluster.Contains(pos) || board.GetMatchGroup(pos) != group)
        {
          return false;
        }
      }

      return true;
    }
  }
}
