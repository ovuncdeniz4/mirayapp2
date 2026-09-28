using Kut.Core.Animals;
using NUnit.Framework;

namespace Kut.Core.Tests
{
  [TestFixture]
  public class AnimalAssignmentTests
  {
    [Test]
    public void SameDateSameAnimal()
    {
      var a = AnimalAssignment.Assign(3, 15);
      var b = AnimalAssignment.Assign(3, 15);
      Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void DifferentDatesCanDiffer()
    {
      var a = AnimalAssignment.Assign(1, 1);
      var b = AnimalAssignment.Assign(7, 20);
      Assert.That(a, Is.Not.Empty);
      Assert.That(b, Is.Not.Empty);
    }
  }
}
