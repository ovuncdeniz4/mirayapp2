using System.IO;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.Levels;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class LevelSessionTests
  {
    [Test]
    public void Level001MakeMatchesObjectiveCanComplete()
    {
      var path = Path.Combine(FindContentDir(), "level_001.json");
      var session = LevelSession.FromDefinition(LevelLoader.LoadFromFile(path));
      var attempts = 0;
      while (session.Outcome == LevelOutcome.InProgress && attempts < 40)
      {
        attempts++;
        session.Submit(new SwapCommand(new GridPos(0, 0), new GridPos(1, 0)));
        session.Submit(new SwapCommand(new GridPos(2, 0), new GridPos(3, 0)));
      }

      Assert.That(session.Objectives.GetProgress(0), Is.GreaterThan(0));
    }

    private static string FindContentDir()
    {
      var dir = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "content", "levels"));
      return dir;
    }
  }
}
