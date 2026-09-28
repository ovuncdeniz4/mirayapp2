using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;
using Element = Kut.Core.Tiles.Element;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class ReactionSourceTests
  {
    [Test]
    public void WaterClearAtFormerCellCleansAdjacentMud()
    {
      var board = new BoardState(new BoardSize(5, 5));
      board.SetCell(new GridPos(2, 2), Cell.FromObstacle(new ObstacleInstance(ObstacleType.Mud, 1)));
      board.SetCell(new GridPos(2, 1), Cell.FromTile(TileRegistry.Create("water_drop")));
      board.SetCell(new GridPos(1, 1), Cell.FromTile(TileRegistry.Create("water_drop")));
      board.SetCell(new GridPos(3, 1), Cell.FromTile(TileRegistry.Create("water_drop")));

      var rules = new Kut.Core.Levels.LevelRules { Moves = 10, Seed = 1 };
      var engine = new BoardEngine(board, rules);

      var result = engine.Apply(new SwapCommand(new GridPos(1, 1), new GridPos(2, 1)));

      Assert.That(result.Events.OfType<ReactionSourceEvent>().Any(e => e.Element == Element.Water), Is.True);
      Assert.That(result.Events.OfType<MudCleansedEvent>().Any(e => e.At == new GridPos(2, 2) && e.LayersRemaining == 0), Is.True);
    }
  }
}
