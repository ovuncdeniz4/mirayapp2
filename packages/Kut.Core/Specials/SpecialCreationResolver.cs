using System.Collections.Generic;
using System.Linq;
using Kut.Core.Board;
using Kut.Core.Match;
using Kut.Core.Tiles;

namespace Kut.Core.Specials
{
  public enum SpecialCreationKind
  {
    None,
    WindChime,
    ShamanDrum
  }

  public static class SpecialCreationResolver
  {
    public static SpecialCreationKind Resolve(
      BoardState board,
      HashSet<GridPos> cluster,
      GridPos? playerSwapDestination,
      bool allowDrum,
      bool allowChime,
      out GridPos spawnAt,
      out LineOrientation orientation,
      out string resonanceMatchGroup)
    {
      spawnAt = default;
      orientation = LineOrientation.Horizontal;
      resonanceMatchGroup = "";

      if (allowDrum && MatchDetection.TryFindLineOfFive(board, cluster, out orientation))
      {
        spawnAt = SelectSpawnCell(cluster, playerSwapDestination);
        resonanceMatchGroup = board.GetMatchGroup(spawnAt) ?? "earth";
        return SpecialCreationKind.ShamanDrum;
      }

      if (allowChime && MatchDetection.TryFindLineOfFour(board, cluster, out orientation))
      {
        spawnAt = SelectSpawnCell(cluster, playerSwapDestination);
        return SpecialCreationKind.WindChime;
      }

      return SpecialCreationKind.None;
    }

    public static bool TryResolveWindChime(
      BoardState board,
      HashSet<GridPos> cluster,
      GridPos? playerSwapDestination,
      out GridPos spawnAt,
      out LineOrientation orientation)
    {
      var kind = Resolve(board, cluster, playerSwapDestination, false, true, out spawnAt, out orientation, out _);
      return kind == SpecialCreationKind.WindChime;
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
