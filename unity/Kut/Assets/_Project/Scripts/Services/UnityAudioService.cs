using Kut.Core.Tiles;
using UnityEngine;

namespace Kut.Unity.Services
{
  /// <summary>Resources/Audio clips — optional AudioMixer at Audio/Mixer/KutMain.</summary>
  public sealed class UnityAudioService : MonoBehaviour, IAudioService
  {
    private AudioSource _sfx = null!;
    private AudioSource _music = null!;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;

    private void Awake()
    {
      _sfx = gameObject.AddComponent<AudioSource>();
      _sfx.playOnAwake = false;
      _music = gameObject.AddComponent<AudioSource>();
      _music.loop = true;
      _music.playOnAwake = false;
      var mixer = Resources.Load<AudioMixer>("Audio/Mixer/KutMain");
      if (mixer != null)
      {
        var sfxGroup = mixer.FindMatchingGroups("SFX");
        if (sfxGroup.Length > 0)
        {
          _sfx.outputAudioMixerGroup = sfxGroup[0];
        }

        var musicGroup = mixer.FindMatchingGroups("Music");
        if (musicGroup.Length > 0)
        {
          _music.outputAudioMixerGroup = musicGroup[0];
        }
      }
    }

    public void SetVolumes(float music, float sfx)
    {
      _musicVolume = Mathf.Clamp01(music);
      _sfxVolume = Mathf.Clamp01(sfx);
      _music.volume = _musicVolume;
    }

    public void PlayUiClick() => PlayClip("Audio/SFX/ui_click", 1f);

    public void PlayMatch(Element element)
    {
      var id = element switch
      {
        Element.Water => "match_water",
        Element.Fire => "match_fire",
        Element.Air => "match_air",
        _ => "match_earth"
      };
      PlayClip($"Audio/SFX/{id}", 0.9f);
    }

    public void PlaySpecial(SpecialType special, Element? resonance)
    {
      var id = special switch
      {
        SpecialType.WindChime => "special_chime",
        SpecialType.FireBomb => "special_bomb",
        SpecialType.ShamanDrum => resonance == Element.Water ? "special_drum_water" : "special_drum_earth",
        _ => "ui_click"
      };
      PlayClip($"Audio/SFX/{id}", 1f);
    }

    public void PlayVictory() => PlayClip("Audio/SFX/victory_stinger", 1f);

    public void PlayMusicForChapter(int chapter)
    {
      var clip = Resources.Load<AudioClip>(chapter <= 1 ? "Audio/Music/mus_ch1_ambient" : "Audio/Music/mus_ch2_ambient");
      if (clip == null)
      {
        return;
      }

      if (_music.clip == clip && _music.isPlaying)
      {
        return;
      }

      _music.clip = clip;
      _music.volume = _musicVolume;
      _music.Play();
    }

    private void PlayClip(string path, float scale)
    {
      if (_sfxVolume <= 0f)
      {
        return;
      }

      var clip = Resources.Load<AudioClip>(path);
      if (clip != null)
      {
        _sfx.PlayOneShot(clip, _sfxVolume * scale);
      }
    }
  }
}
