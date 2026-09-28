using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Levels;
using Kut.Core.Tiles;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class GravityCascadeTests
  {
    [Test]
    public void GravityDropsTileIntoEmptyCellBelow()
    {
      var board = new BoardState(new BoardSize(4, 4));
      board.SetCell(new GridPos(1, 2), Cell.FromTile(TileRegistry.Create("earth_moss")));
      var events = new System.Collections.Generic.List<GameEvent>();
      GravitySystem.Apply(board, events);

      Assert.That(board.GetCell(new GridPos(1, 2)).Kind, Is.EqualTo(CellKind.Empty));
      Assert.That(board.GetCell(new GridPos(1, 3)).Kind, Is.EqualTo(CellKind.Tile));
      Assert.That(events.OfType<GravityStepEvent>().Any(), Is.True);
    }

    [Test]
    public void ValidSwapCanProduceCascadeEndedEvent()
    {
      var board = new BoardState(new BoardSize(5, 5));
      board.SetCell(new GridPos(0, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(1, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(2, 0), Cell.FromTile(TileRegistry.Create("water_drop")));
      board.SetCell(new GridPos(2, 1), Cell.FromTile(TileRegistry.Create("earth_moss")));

      var engine = new BoardEngine(board, new LevelRules { Moves = 5, Seed = 7, EnableWindChime = false });
      var result = engine.Apply(new SwapCommand(new GridPos(2, 1), new GridPos(2, 0)));

      Assert.That(result.Events.OfType<CascadeEndedEvent>().Any(), Is.True);
    }
  }
}
