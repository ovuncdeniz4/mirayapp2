using System.Collections;
using System.Collections.Generic;
using Kut.Core.GameEvents;
using UnityEngine;

namespace Kut.Unity.Presentation
{
  /// <summary>
  /// Phase 2 stub: sequences presentation steps before board refresh (replace Logs with tweens/SFX).
  /// </summary>
  public sealed class PresentationAnimationQueue : MonoBehaviour
  {
    [SerializeField] private float stepDelaySeconds = 0.05f;

    public bool IsPlaying { get; private set; }

    public void Play(IReadOnlyList<GameEvent> events, System.Action onComplete)
    {
      StopAllCoroutines();
      StartCoroutine(PlayRoutine(events, onComplete));
    }

    private IEnumerator PlayRoutine(IReadOnlyList<GameEvent> events, System.Action onComplete)
    {
      IsPlaying = true;
      foreach (var evt in events)
      {
        Debug.Log($"[AnimQueue] {evt.EventType}");
        if (stepDelaySeconds > 0f)
        {
          yield return new WaitForSeconds(stepDelaySeconds);
        }
      }

      IsPlaying = false;
      onComplete?.Invoke();
    }
  }
}
