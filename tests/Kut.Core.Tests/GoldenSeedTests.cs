using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.Levels;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class GoldenSeedTests
  {
    [Test]
    public void Level001Seed101SwapIsDeterministicAcrossRuns()
    {
      var root = System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
      var json = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "content", "levels", "level_001.json"));
      var def = LevelLoader.Parse(json);

      static int EventHash(CommandResult result)
      {
        var hash = result.Events.Count;
        for (var i = 0; i < result.Events.Count; i++)
        {
          hash = unchecked(hash * 31 + result.Events[i].EventType.GetHashCode());
        }

        return hash;
      }

      var a = LevelSession.FromDefinition(def);
      var b = LevelSession.FromDefinition(def);
      var cmd = new SwapCommand(new GridPos(0, 0), new GridPos(1, 0));
      var ra = a.Submit(cmd);
      var rb = b.Submit(cmd);
      Assert.That(EventHash(ra), Is.EqualTo(EventHash(rb)));
      Assert.That(ra.Events.Count, Is.GreaterThan(0));
    }
  }
}
