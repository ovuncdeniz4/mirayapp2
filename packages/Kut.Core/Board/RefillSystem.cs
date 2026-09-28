using System.Collections.Generic;
using Kut.Core.GameEvents;
using Kut.Core.Random;
using Kut.Core.Tiles;

namespace Kut.Core.Board
{
  public static class RefillSystem
  {
    public static void RefillColumns(BoardState board, DeterministicRandom rng, IReadOnlyList<string> spawnTable, IList<GameEvent> events)
    {
      for (var x = 0; x < board.Size.Width; x++)
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          var pos = new GridPos(x, y);
          if (board.GetCell(pos).Kind != CellKind.Empty)
          {
            continue;
          }

          var tileId = spawnTable[rng.NextInt(spawnTable.Count)];
          board.SetCell(pos, Cell.FromTile(TileRegistry.Create(tileId)));
          events.Add(new RefillEvent(pos, tileId));
        }
      }
    }
  }
}
