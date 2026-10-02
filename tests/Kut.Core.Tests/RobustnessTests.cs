using System.IO;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.Levels;
using Kut.Core.Save;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public sealed class RobustnessTests
  {
    [Test]
    public void EveryShippedLevelStartsStableWithALegalMove()
    {
      var root = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "content", "levels"));
      foreach (var path in Directory.GetFiles(root, "level_*.json"))
      {
        var engine = LevelLoader.CreateEngineFromFile(path);
        Assert.That(Kut.Core.Match.MatchDetection.FindMatchedCells(engine.State), Is.Empty, Path.GetFileName(path));
        Assert.That(BoardLegalMoves.HasLegalMove(engine.State), Is.True, Path.GetFileName(path));
      }
    }

    [Test]
    public void CompletedSessionRejectsFurtherCommandsWithoutConsumingMoves()
    {
      var def = new LevelDefinition { Moves = 3, Seed = 11 };
      var session = LevelSession.FromDefinition(def);
      var first = session.Submit(new SwapCommand(new GridPos(0, 0), new GridPos(1, 0)));
      Assert.That(session.Outcome, Is.EqualTo(LevelOutcome.Victory));
      var moves = session.Engine.MovesRemaining;
      var result = session.Submit(new SwapCommand(new GridPos(0, 0), new GridPos(1, 0)));
      Assert.That(result.Status, Is.EqualTo(CommandStatus.RejectedSessionComplete));
      Assert.That(session.Engine.MovesRemaining, Is.EqualTo(moves));
      _ = first;
    }

    [Test]
    public void InvalidCoordinatesAreRejectedWithoutThrowing()
    {
      var engine = BoardEngine.CreatePrototype();
      var result = engine.Apply(new SwapCommand(new GridPos(-1, 0), new GridPos(0, 0)));
      Assert.That(result.Status, Is.EqualTo(CommandStatus.RejectedInvalidPosition));
    }

    [Test]
    public void CorruptPrimaryAndBackupReturnCleanSave()
    {
      var dir = Path.Combine(Path.GetTempPath(), "kut-tests", System.Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(dir);
      var path = Path.Combine(dir, "save.json");
      File.WriteAllText(path, "{");
      File.WriteAllText(path + ".bak", "not json");
      var save = SaveStore.Load(path);
      Assert.That(save.SchemaVersion, Is.EqualTo(4));
      Assert.That(save.HighestUnlockedLevel, Is.EqualTo(1));
    }
  }
}
