using System.Collections.Generic;
using System.Text;
using Kut.Core.Board;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;

namespace Kut.Core.Playtest
{
  public static class BoardAsciiRenderer
  {
    public static string Render(BoardState board)
    {
      var sb = new StringBuilder();
      sb.AppendLine("   " + string.Join(" ", ColumnLabels(board.Size.Width)));
      for (var y = 0; y < board.Size.Height; y++)
      {
        sb.Append(y).Append(' ');
        for (var x = 0; x < board.Size.Width; x++)
        {
          sb.Append(' ').Append(Symbol(board.GetCell(new GridPos(x, y))));
        }

        sb.AppendLine();
      }

      return sb.ToString();
    }

    private static IEnumerable<string> ColumnLabels(int width)
    {
      for (var x = 0; x < width; x++)
      {
        yield return x.ToString();
      }
    }

    private static string Symbol(Cell cell)
    {
      switch (cell.Kind)
      {
        case CellKind.Empty:
          return "·";
        case CellKind.Blocker:
          return "#";
        case CellKind.Obstacle:
          if (cell.Obstacle?.Type == ObstacleType.Mud)
          {
            return "M";
          }

          return cell.Obstacle?.Type == ObstacleType.Vine ? "V" : "O";
        case CellKind.Tile:
          var tile = cell.Tile!;
          if (tile.Special == SpecialType.WindChime)
          {
            return "C";
          }

          if (tile.Special == SpecialType.ShamanDrum)
          {
            return "D";
          }

          return tile.Element == Element.Water ? "W" : "E";
        default:
          return "?";
      }
    }
  }
}
