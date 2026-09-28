using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.Match;
using Kut.Core.Tiles;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class MatchDetectionTests
  {
    [Test]
    public void FindsHorizontalMatchOfThree()
    {
      var board = new BoardState(new BoardSize(8, 8));
      board.SetCell(new GridPos(0, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(1, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(2, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));

      var matched = MatchDetection.FindMatchedCells(board);
      Assert.That(matched.Count, Is.EqualTo(3));
    }

    [Test]
    public void InvalidSwapRevertsWithoutMove()
    {
      var board = new BoardState(new BoardSize(4, 4));
      board.SetCell(new GridPos(0, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(0, 1), Cell.FromTile(TileRegistry.Create("water_drop")));
      var engine = new BoardEngine(board, new Kut.Core.Levels.LevelRules { Moves = 5, Seed = 1 });
      var startMoves = engine.MovesRemaining;
      var result = engine.Apply(new SwapCommand(new GridPos(0, 0), new GridPos(0, 1)));

      Assert.That(result.Events.Any(e => e is Kut.Core.GameEvents.SwapRevertedEvent), Is.True);
      Assert.That(engine.MovesRemaining, Is.EqualTo(startMoves));
    }
  }
}
