using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class VineReactionTests
  {
    [Test]
    public void EarthClearBreaksAdjacentVine()
    {
      var board = new BoardState(new BoardSize(5, 5));
      board.SetCell(new GridPos(2, 2), Cell.FromObstacle(new ObstacleInstance(ObstacleType.Vine, 1)));
      board.SetCell(new GridPos(2, 1), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(1, 1), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(3, 1), Cell.FromTile(TileRegistry.Create("earth_moss")));

      var engine = new BoardEngine(board, new Kut.Core.Levels.LevelRules { Moves = 5, Seed = 1 });
      var result = engine.Apply(new SwapCommand(new GridPos(1, 1), new GridPos(2, 1)));

      Assert.That(result.Events.OfType<VineBrokenEvent>().Any(v => v.HpRemaining == 0), Is.True);
      var vineCell = board.GetCell(new GridPos(2, 2));
      Assert.That(
        vineCell.Kind == CellKind.Obstacle && vineCell.Obstacle?.Type == ObstacleType.Vine,
        Is.False,
        "Vine obstacle should be removed (cell may refill with a tile after gravity)");
    }
  }
}
