using System.IO;
using Kut.Core.Board;
using Kut.Core.Levels;
using Kut.Core.Obstacles;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class LevelLoaderTests
  {
    [Test]
    public void LoadsPrototypeJsonAndCreatesMud()
    {
      var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "level_prototype.json");
      var engine = LevelLoader.CreateEngineFromFile(path);

      Assert.That(engine.State.GetCell(new GridPos(3, 3)).Obstacle?.Type, Is.EqualTo(ObstacleType.Mud));
      Assert.That(engine.MovesRemaining, Is.EqualTo(30));
    }
  }
}
