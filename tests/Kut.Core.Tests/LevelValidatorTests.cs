using System;
using System.IO;
using Kut.Core.Levels;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class LevelValidatorTests
  {
    [Test]
    public void AllChapterLevelsPassValidation()
    {
      var dir = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "content", "levels"));
      foreach (var file in Directory.GetFiles(dir, "level_*.json"))
      {
        var name = Path.GetFileNameWithoutExtension(file);
        if (name.Length != 9 || !name.StartsWith("level_0", StringComparison.Ordinal))
        {
          continue;
        }

        var errors = LevelValidator.ValidateFile(file);
        Assert.That(errors, Is.Empty, string.Join("; ", errors));
      }
    }
  }
}
