using Kut.Core.Board;
using Kut.Core.Levels;
using Kut.Core.Obstacles;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class LevelLoaderObstacleTests
  {
    [Test]
    public void ParsesVineAndStoneCellsFromJson()
    {
      const string json = """
        {
          "id": "level_test",
          "chapter": 1,
          "boardSize": { "w": 8, "h": 8 },
          "moves": 20,
          "seed": 1,
          "vineCells": [{ "x": 1, "y": 2, "layers": 2 }],
          "stoneCells": [{ "x": 0, "y": 7 }],
          "objectives": [{ "type": "make_matches", "target": 1 }]
        }
        """;

      var def = LevelLoader.Parse(json);
      var engine = LevelLoader.CreateEngine(def);
      var state = engine.State;

      Assert.That(state.GetCell(new GridPos(1, 2)).Obstacle?.Type, Is.EqualTo(ObstacleType.Vine));
      Assert.That(state.GetCell(new GridPos(1, 2)).Obstacle?.Layers, Is.EqualTo(2));
      Assert.That(state.GetCell(new GridPos(0, 7)).Kind, Is.EqualTo(CellKind.Blocker));
    }
  }
}
