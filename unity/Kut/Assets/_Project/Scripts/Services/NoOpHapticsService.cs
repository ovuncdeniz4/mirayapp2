namespace Kut.Unity.Services
{
  public sealed class NoOpHapticsService : IHapticsService
  {
    public bool Enabled { get; set; } = true;

    public void Light()
    {
    }

    public void Medium()
    {
    }

    public void Heavy()
    {
    }
  }
}
