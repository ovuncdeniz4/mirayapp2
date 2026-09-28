using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.GameEvents;
using Kut.Core.Tiles;

namespace Kut.Core.Specials
{
  public static class WindChimeRules
  {
    public static List<GridPos> GetLineCells(BoardState board, GridPos chimePos, LineOrientation orientation)
    {
      var cells = new List<GridPos>();
      if (orientation == LineOrientation.Horizontal)
      {
        for (var x = 0; x < board.Size.Width; x++)
        {
          cells.Add(new GridPos(x, chimePos.Y));
        }
      }
      else
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          cells.Add(new GridPos(chimePos.X, y));
        }
      }

      return cells;
    }

    public static void Activate(
      BoardState board,
      GridPos at,
      IList<GameEvent> events,
      out List<GridPos> tilesToClear)
    {
      tilesToClear = new List<GridPos>();
      var cell = board.GetCell(at);
      if (cell.Kind != CellKind.Tile || cell.Tile?.Special != SpecialType.WindChime)
      {
        return;
      }

      var orientation = cell.Tile.ChimeOrientation ?? LineOrientation.Horizontal;
      events.Add(new SpecialActivatedEvent(at, SpecialType.WindChime));
      events.Add(new LineClearEvent(orientation == LineOrientation.Horizontal, orientation == LineOrientation.Horizontal ? at.Y : at.X));

      var line = GetLineCells(board, at, orientation);
      foreach (var p in line)
      {
        var c = board.GetCell(p);
        if (c.Kind == CellKind.Tile && c.Tile != null)
        {
          tilesToClear.Add(p);
        }
      }

      board.SetCell(at, Cell.Empty());
    }
  }
}
