using UnityEngine;

namespace Kut.Unity.Services
{
  /// <summary>Procedural placeholder tones — replace clips via Resources later.</summary>
  public sealed class UnityAudioService : MonoBehaviour, IAudioService
  {
    private AudioSource _sfx = null!;
    private float _sfxVolume = 1f;

    private void Awake()
    {
      _sfx = gameObject.AddComponent<AudioSource>();
      _sfx.playOnAwake = false;
    }

    public void SetVolumes(float music, float sfx)
    {
      _sfxVolume = Mathf.Clamp01(sfx);
    }

    public void PlayUiClick() => PlayTone(440f, 0.04f);

    public void PlayMatch() => PlayTone(660f, 0.06f);

    public void PlayVictory() => PlayTone(523f, 0.12f);

    private void PlayTone(float freq, float duration)
    {
      if (_sfxVolume <= 0f)
      {
        return;
      }

      var clip = AudioClip.Create("tone", Mathf.CeilToInt(duration * 44100), 1, 44100, false);
      var data = new float[clip.samples];
      for (var i = 0; i < data.Length; i++)
      {
        data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / 44100f) * _sfxVolume * 0.25f;
      }

      clip.SetData(data, 0);
      _sfx.PlayOneShot(clip);
    }
  }
}
