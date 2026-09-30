namespace Kut.Unity.Services
{
  public interface IAudioService
  {
    void SetVolumes(float music, float sfx);
    void PlayUiClick();
    void PlayMatch();
    void PlayVictory();
  }
}
