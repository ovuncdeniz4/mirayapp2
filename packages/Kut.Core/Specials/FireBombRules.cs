using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.GameEvents;
using Kut.Core.Tiles;

namespace Kut.Core.Specials
{
  public static class FireBombRules
  {
    public static void Activate(
      BoardState board,
      GridPos at,
      IList<GameEvent> events,
      out List<GridPos> tilesToClear)
    {
      tilesToClear = new List<GridPos>();
      var cell = board.GetCell(at);
      if (cell.Kind != CellKind.Tile || cell.Tile?.Special != SpecialType.FireBomb)
      {
        return;
      }

      events.Add(new SpecialActivatedEvent(at, SpecialType.FireBomb));
      events.Add(new AreaClearEvent(at, 2));

      for (var dx = -2; dx <= 2; dx++)
      {
        for (var dy = -2; dy <= 2; dy++)
        {
          if (System.Math.Abs(dx) + System.Math.Abs(dy) > 2)
          {
            continue;
          }

          var pos = at.Offset(dx, dy);
          if (!board.Size.Contains(pos))
          {
            continue;
          }

          var c = board.GetCell(pos);
          if (c.Kind == CellKind.Tile && c.Tile != null)
          {
            tilesToClear.Add(pos);
          }
        }
      }

      board.SetCell(at, Cell.Empty());
    }
  }
}
