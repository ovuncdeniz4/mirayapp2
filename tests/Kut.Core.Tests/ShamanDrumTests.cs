using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Tiles;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class ShamanDrumTests
  {
    [Test]
    public void StraightFiveCreatesDrumWhenEnabled()
    {
      var board = new BoardState(new BoardSize(8, 8));
      for (var x = 0; x < 5; x++)
      {
        board.SetCell(new GridPos(x, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      }

      board.SetCell(new GridPos(5, 0), Cell.FromTile(TileRegistry.Create("water_drop")));
      board.SetCell(new GridPos(5, 1), Cell.FromTile(TileRegistry.Create("earth_moss")));

      var rules = new Kut.Core.Levels.LevelRules
      {
        Moves = 5,
        Seed = 1,
        EnableShamanDrum = true,
        EnableWindChime = true,
        EnableSpecialCreation = true
      };
      var engine = new BoardEngine(board, rules);
      var result = engine.Apply(new SwapCommand(new GridPos(5, 1), new GridPos(5, 0)));

      Assert.That(result.Events.OfType<SpecialCreatedEvent>().Any(e => e.Special == SpecialType.ShamanDrum), Is.True);
    }

    [Test]
    public void DrumResonanceClearsMatchingElement()
    {
      var board = new BoardState(new BoardSize(4, 4));
      var drum = TileRegistry.Create("earth_moss");
      drum.Special = SpecialType.ShamanDrum;
      drum.ResonanceMatchGroup = "earth";
      board.SetCell(new GridPos(1, 1), Cell.FromTile(drum));
      board.SetCell(new GridPos(0, 0), Cell.FromTile(TileRegistry.Create("earth_moss")));
      board.SetCell(new GridPos(3, 3), Cell.FromTile(TileRegistry.Create("water_drop")));

      var engine = new BoardEngine(board, new Kut.Core.Levels.LevelRules { Moves = 3, Seed = 1 });
      engine.Apply(new ActivateSpecialCommand(new GridPos(1, 1)));

      Assert.That(board.GetCell(new GridPos(0, 0)).Kind, Is.EqualTo(CellKind.Empty));
      Assert.That(board.GetCell(new GridPos(3, 3)).Kind, Is.EqualTo(CellKind.Tile));
    }
  }
}
