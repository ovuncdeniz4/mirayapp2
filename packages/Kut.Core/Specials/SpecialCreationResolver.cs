using System.Collections.Generic;
using System.Linq;
using Kut.Core.Board;
using Kut.Core.Match;
using Kut.Core.Tiles;

namespace Kut.Core.Specials
{
  public static class SpecialCreationResolver
  {
    public static bool TryResolveWindChime(
      BoardState board,
      HashSet<GridPos> cluster,
      GridPos? playerSwapDestination,
      out GridPos spawnAt,
      out LineOrientation orientation)
    {
      spawnAt = default;
      orientation = LineOrientation.Horizontal;

      if (!MatchDetection.TryFindLineOfFour(board, cluster, out orientation))
      {
        return false;
      }

      spawnAt = SelectSpawnCell(cluster, playerSwapDestination);
      return true;
    }

    private static GridPos SelectSpawnCell(HashSet<GridPos> cluster, GridPos? playerSwapDestination)
    {
      if (playerSwapDestination.HasValue && cluster.Contains(playerSwapDestination.Value))
      {
        return playerSwapDestination.Value;
      }

      return cluster.OrderBy(p => p.Y).ThenBy(p => p.X).First();
    }
  }
}
