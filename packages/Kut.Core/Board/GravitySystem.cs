using System.Collections.Generic;
using Kut.Core.GameEvents;
using Kut.Core.Tiles;

namespace Kut.Core.Board
{
  public static class GravitySystem
  {
    public static void Apply(BoardState board, IList<GameEvent> events)
    {
      for (var x = 0; x < board.Size.Width; x++)
      {
        var writeY = 0;
        for (var y = 0; y < board.Size.Height; y++)
        {
          var pos = new GridPos(x, y);
          var cell = board.GetCell(pos);
          if (cell.Kind == CellKind.Blocker || cell.Kind == CellKind.Obstacle)
          {
            writeY = y + 1;
            continue;
          }

          if (cell.Kind == CellKind.Tile && cell.Tile != null)
          {
            if (y != writeY)
            {
              var target = new GridPos(x, writeY);
              board.SetCell(target, Cell.FromTile(cell.Tile));
              board.SetCell(pos, Cell.Empty());
              events.Add(new GravityStepEvent(pos, target));
            }

            writeY++;
          }
        }
      }
    }
  }
}
