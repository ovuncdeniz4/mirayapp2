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
        Assert.That(BoardLegalMoves.TryFindLegalSwap(session.Engine.State, out var from, out var to), Is.True);
        session.Submit(new SwapCommand(from, to));
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
