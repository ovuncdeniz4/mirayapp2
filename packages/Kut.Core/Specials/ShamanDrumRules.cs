using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.GameEvents;
using Kut.Core.Tiles;

namespace Kut.Core.Specials
{
  public static class ShamanDrumRules
  {
    public static void Activate(
      BoardState board,
      GridPos at,
      IList<GameEvent> events,
      out List<GridPos> tilesToClear)
    {
      tilesToClear = new List<GridPos>();
      var cell = board.GetCell(at);
      if (cell.Kind != CellKind.Tile || cell.Tile?.Special != SpecialType.ShamanDrum)
      {
        return;
      }

      var group = cell.Tile.ResonanceMatchGroup ?? cell.Tile.MatchGroup;
      events.Add(new SpecialActivatedEvent(at, SpecialType.ShamanDrum));

      for (var x = 0; x < board.Size.Width; x++)
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          var pos = new GridPos(x, y);
          var c = board.GetCell(pos);
          if (c.Kind == CellKind.Tile && c.Tile != null && c.Tile.Special == SpecialType.None &&
              c.Tile.MatchGroup == group)
          {
            tilesToClear.Add(pos);
          }
        }
      }

      board.SetCell(at, Cell.Empty());
    }
  }
}
