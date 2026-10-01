using Kut.Core.Tiles;

namespace Kut.Unity.Services
{
  public interface IAudioService
  {
    void SetVolumes(float music, float sfx);
    void PlayUiClick();
    void PlayMatch(Element element);
    void PlaySpecial(SpecialType special, Element? resonance);
    void PlayVictory();
    void PlayMusicForChapter(int chapter);
  }
}
