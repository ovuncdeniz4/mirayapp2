using Kut.Core.Meta;
using Kut.Core.Save;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class MetaProgressionTests
  {
    [Test]
    public void LevelTenUnlocksMetaTabsAndCollectionItem()
    {
      var chapters = ChapterCatalog.LoadFromFile(
        System.IO.Path.GetFullPath(System.IO.Path.Combine(
          TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "content", "chapters.json")));
      var collection = CollectionCatalog.LoadFromFile(
        System.IO.Path.GetFullPath(System.IO.Path.Combine(
          TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "content", "collection", "catalog.json")));
      var save = new SaveData();
      MetaProgression.ApplyLevelVictory(save, chapters, collection, "level_010");

      Assert.That(save.TotemTabUnlocked, Is.True);
      Assert.That(save.TotemTier, Is.EqualTo(1));
      Assert.That(save.UnlockedCollectionIds, Does.Contain("relic_shrine_ember"));
    }
  }
}
