using System.Collections.Generic;
using System.Linq;
using Kut.Core.Board;
using Kut.Core.GameEvents;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;

namespace Kut.Core.Elements
{
  public static class ElementReactionSystem
  {
    public static void ApplyWaterMudReactions(
      BoardState board,
      IList<GridPos> waterFormerCells,
      IList<GameEvent> events)
    {
      var sources = waterFormerCells.OrderBy(p => p).ToList();
      foreach (var former in sources)
      {
        events.Add(new ReactionSourceEvent(former, Element.Water));
        TryCleanseAdjacentMud(board, former, events);
      }
    }

    private static void TryCleanseAdjacentMud(BoardState board, GridPos former, IList<GameEvent> events)
    {
      var dirs = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
      foreach (var (dx, dy) in dirs)
      {
        var adj = former.Offset(dx, dy);
        if (!board.Size.Contains(adj))
        {
          continue;
        }

        var cell = board.GetCell(adj);
        if (cell.Kind != CellKind.Obstacle || cell.Obstacle?.Type != ObstacleType.Mud)
        {
          continue;
        }

        cell.Obstacle.Layers--;
        events.Add(new MudCleansedEvent(adj, cell.Obstacle.Layers));
        if (cell.Obstacle.Layers <= 0)
        {
          board.SetCell(adj, Cell.Empty());
        }
      }
    }

    public static void ApplyEarthVineReactions(
      BoardState board,
      IList<GridPos> earthFormerCells,
      IList<GameEvent> events)
    {
      var sources = earthFormerCells.OrderBy(p => p).ToList();
      foreach (var former in sources)
      {
        events.Add(new ReactionSourceEvent(former, Element.Earth));
        TryBreakAdjacentVine(board, former, events);
      }
    }

    public static void ApplyFireBurnReactions(
      BoardState board,
      IList<GridPos> fireFormerCells,
      IList<GameEvent> events)
    {
      var sources = fireFormerCells.OrderBy(p => p).ToList();
      foreach (var former in sources)
      {
        events.Add(new ReactionSourceEvent(former, Element.Fire));
        TryBurnAdjacentMud(board, former, events);
        TryBreakAdjacentVine(board, former, events);
      }
    }

    private static void TryBurnAdjacentMud(BoardState board, GridPos former, IList<GameEvent> events)
    {
      var dirs = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
      foreach (var (dx, dy) in dirs)
      {
        var adj = former.Offset(dx, dy);
        if (!board.Size.Contains(adj))
        {
          continue;
        }

        var cell = board.GetCell(adj);
        if (cell.Kind != CellKind.Obstacle || cell.Obstacle?.Type != ObstacleType.Mud)
        {
          continue;
        }

        cell.Obstacle.Layers--;
        events.Add(new MudCleansedEvent(adj, cell.Obstacle.Layers));
        if (cell.Obstacle.Layers <= 0)
        {
          board.SetCell(adj, Cell.Empty());
        }
      }
    }

    private static void TryBreakAdjacentVine(BoardState board, GridPos former, IList<GameEvent> events)
    {
      var dirs = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
      foreach (var (dx, dy) in dirs)
      {
        var adj = former.Offset(dx, dy);
        if (!board.Size.Contains(adj))
        {
          continue;
        }

        var cell = board.GetCell(adj);
        if (cell.Kind != CellKind.Obstacle || cell.Obstacle?.Type != ObstacleType.Vine)
        {
          continue;
        }

        cell.Obstacle.Layers--;
        events.Add(new VineBrokenEvent(adj, cell.Obstacle.Layers));
        if (cell.Obstacle.Layers <= 0)
        {
          board.SetCell(adj, Cell.Empty());
        }
      }
    }
  }
}
