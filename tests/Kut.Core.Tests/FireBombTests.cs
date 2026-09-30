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
  public class FireBombTests
  {
    [Test]
    public void ResolverCreatesFireBombFromTwoByTwoInCluster()
    {
      var board = new BoardState(new BoardSize(4, 4));
      for (var x = 0; x <= 1; x++)
      {
        for (var y = 0; y <= 1; y++)
        {
          board.SetCell(new GridPos(x, y), Cell.FromTile(TileRegistry.Create("fire_ember")));
        }
      }

      board.SetCell(new GridPos(2, 0), Cell.FromTile(TileRegistry.Create("fire_ember")));
      var cluster = new System.Collections.Generic.HashSet<GridPos>
      {
        new GridPos(0, 0), new GridPos(1, 0), new GridPos(0, 1), new GridPos(1, 1), new GridPos(2, 0)
      };

      var kind = Kut.Core.Specials.SpecialCreationResolver.Resolve(
        board,
        cluster,
        new GridPos(1, 1),
        allowDrum: false,
        allowChime: false,
        allowFireBomb: true,
        out var spawn,
        out _,
        out _);

      Assert.That(kind, Is.EqualTo(Kut.Core.Specials.SpecialCreationKind.FireBomb));
      Assert.That(spawn, Is.EqualTo(new GridPos(1, 1)));
    }

    [Test]
    public void ActivateFireBombClearsManhattanRadiusTwoThirteenCellsOnFullBoard()
    {
      var board = new BoardState(new BoardSize(8, 8));
      for (var x = 0; x < 8; x++)
      {
        for (var y = 0; y < 8; y++)
        {
          if (x == 4 && y == 4)
          {
            var bomb = TileRegistry.Create("fire_ember");
            bomb.Special = SpecialType.FireBomb;
            board.SetCell(new GridPos(x, y), Cell.FromTile(bomb));
          }
          else
          {
            board.SetCell(new GridPos(x, y), Cell.FromTile(TileRegistry.Create("earth_moss")));
          }
        }
      }

      var events = new System.Collections.Generic.List<GameEvent>();
      Kut.Core.Specials.FireBombRules.Activate(board, new GridPos(4, 4), events, out var toClear);
      Assert.That(events.OfType<AreaClearEvent>().Any(e => e.Radius == 2), Is.True);
      Assert.That(toClear.Count, Is.EqualTo(13));

      var engineBoard = new BoardState(new BoardSize(8, 8));
      for (var x = 0; x < 8; x++)
      {
        for (var y = 0; y < 8; y++)
        {
          if (x == 4 && y == 4)
          {
            var bomb = TileRegistry.Create("fire_ember");
            bomb.Special = SpecialType.FireBomb;
            engineBoard.SetCell(new GridPos(x, y), Cell.FromTile(bomb));
          }
          else
          {
            engineBoard.SetCell(new GridPos(x, y), Cell.FromTile(TileRegistry.Create("earth_moss")));
          }
        }
      }

      var engine = new BoardEngine(engineBoard, new LevelRules { Moves = 5, Seed = 1 });
      var result = engine.Apply(new ActivateSpecialCommand(new GridPos(4, 4)));
      Assert.That(result.Events.OfType<SpecialActivatedEvent>().Any(e => e.Special == SpecialType.FireBomb), Is.True);
      Assert.That(engine.MovesRemaining, Is.EqualTo(4));
    }

    [Test]
    public void ActivateFireBombClampsAtBoardEdges()
    {
      var board = new BoardState(new BoardSize(5, 5));
      for (var x = 0; x < 5; x++)
      {
        for (var y = 0; y < 5; y++)
        {
          if (x == 2 && y == 2)
          {
            var bomb = TileRegistry.Create("fire_ember");
            bomb.Special = SpecialType.FireBomb;
            board.SetCell(new GridPos(x, y), Cell.FromTile(bomb));
          }
          else
          {
            board.SetCell(new GridPos(x, y), Cell.FromTile(TileRegistry.Create("earth_moss")));
          }
        }
      }

      var engine = new BoardEngine(board, new LevelRules { Moves = 5, Seed = 1 });
      var result = engine.Apply(new ActivateSpecialCommand(new GridPos(2, 2)));
      Assert.That(result.Events.OfType<SpecialActivatedEvent>().Any(e => e.Special == SpecialType.FireBomb), Is.True);
      Assert.That(result.Events.OfType<TilesClearedEvent>().Any(), Is.True);
      Assert.That(engine.MovesRemaining, Is.EqualTo(4));
    }

    private static int CountTiles(BoardState board)
    {
      var n = 0;
      for (var x = 0; x < board.Size.Width; x++)
      {
        for (var y = 0; y < board.Size.Height; y++)
        {
          if (board.GetCell(new GridPos(x, y)).Kind == CellKind.Tile)
          {
            n++;
          }
        }
      }

      return n;
    }
  }
}
