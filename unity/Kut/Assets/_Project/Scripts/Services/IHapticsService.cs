namespace Kut.Unity.Services
{
  public interface IHapticsService
  {
    bool Enabled { get; set; }
    void Light();
    void Medium();
    void Heavy();
  }
}
