using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Tiles;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class WindChimeTests
  {
    [Test]
    public void FourMatchCreatesWindChime()
    {
      var board = new BoardState(new BoardSize(8, 8));
      for (var x = 0; x < 4; x++)
      {
        board.SetCell(new GridPos(x, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      }

      board.SetCell(new GridPos(3, 0), Cell.FromTile(TileRegistry.Create("water_drop")));
      board.SetCell(new GridPos(3, 1), Cell.FromTile(TileRegistry.Create("earth_moss")));

      var engine = new BoardEngine(board, new Kut.Core.Levels.LevelRules { Moves = 5, Seed = 99 });
      var result = engine.Apply(new SwapCommand(new GridPos(3, 1), new GridPos(3, 0)));

      Assert.That(result.Events.OfType<SpecialCreatedEvent>().Any(), Is.True);
      Assert.That(board.GetCell(new GridPos(3, 0)).Tile?.Special, Is.EqualTo(SpecialType.WindChime));
    }

    [Test]
    public void WindChimeActivationClearsRow()
    {
      var board = new BoardState(new BoardSize(8, 8));
      var chime = TileRegistry.Create("earth_moss");
      chime.Special = SpecialType.WindChime;
      chime.ChimeOrientation = LineOrientation.Horizontal;
      board.SetCell(new GridPos(3, 2), Cell.FromTile(chime));
      board.SetCell(new GridPos(0, 2), Cell.FromTile(TileRegistry.Create("water_drop")));
      board.SetCell(new GridPos(7, 2), Cell.FromTile(TileRegistry.Create("earth_moss")));

      var engine = new BoardEngine(board, new Kut.Core.Levels.LevelRules { Moves = 5, Seed = 1 });
      engine.Apply(new ActivateSpecialCommand(new GridPos(3, 2)));

      Assert.That(board.GetCell(new GridPos(0, 2)).Kind, Is.EqualTo(CellKind.Empty));
      Assert.That(board.GetCell(new GridPos(7, 2)).Kind, Is.EqualTo(CellKind.Empty));
    }
  }
}
