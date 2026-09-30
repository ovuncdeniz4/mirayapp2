using System.Collections;
using System.Collections.Generic;
using Kut.Core.GameEvents;
using Kut.Core.Save;
using Kut.Core.Tiles;
using Kut.Unity.Services;
using UnityEngine;

namespace Kut.Unity.Presentation
{
  /// <summary>Replays Kut.Core events with Master Plan §22 timing on BoardGridUi.</summary>
  public sealed class PresentationAnimationQueue : MonoBehaviour
  {
    private BoardGridUi _board = null!;
    private SaveData _save = null!;
    private IAudioService _audio = null!;
    private IHapticsService _haptics = null!;

    public bool IsPlaying { get; private set; }

    public void Configure(BoardGridUi board, SaveData save, IAudioService audio, IHapticsService haptics)
    {
      _board = board;
      _save = save;
      _audio = audio;
      _haptics = haptics;
    }

    public void Play(IReadOnlyList<GameEvent> events, Kut.Core.Board.BoardState stateAfter, System.Action onComplete)
    {
      StopAllCoroutines();
      StartCoroutine(PlayRoutine(events, stateAfter, onComplete));
    }

    private IEnumerator PlayRoutine(IReadOnlyList<GameEvent> events, Kut.Core.Board.BoardState stateAfter, System.Action onComplete)
    {
      IsPlaying = true;
      var reduced = _save != null && _save.ReducedMotion;
      var stagger = reduced ? 0f : 0.02f;

      foreach (var evt in events)
      {
        switch (evt)
        {
          case SwapAttemptedEvent swap:
            yield return _board.AnimateSwap(swap.From, swap.To, reduced ? 0f : 0.14f);
            _audio?.PlayUiClick();
            break;
          case SwapRevertedEvent:
            yield return _board.AnimateSwapRevert(reduced ? 0f : 0.16f);
            break;
          case MatchFoundEvent:
            _audio?.PlayMatch();
            _haptics?.Light();
            if (!reduced)
            {
              yield return new WaitForSeconds(0.08f);
            }

            break;
          case TilesClearedEvent cleared:
            yield return _board.FlashClear(cleared.Cells, reduced ? 0f : 0.12f, stagger);
            break;
          case SpecialCreatedEvent:
            _haptics?.Medium();
            if (!reduced)
            {
              yield return new WaitForSeconds(0.2f);
            }

            break;
          case SpecialActivatedEvent sa:
            if (sa.Special == SpecialType.FireBomb)
            {
              _haptics?.Heavy();
            }
            else
            {
              _haptics?.Medium();
            }

            if (!reduced)
            {
              yield return new WaitForSeconds(sa.Special == SpecialType.ShamanDrum ? 0.35f : 0.28f);
            }

            break;
          case MudCleansedEvent:
          case VineBrokenEvent:
            if (!reduced)
            {
              yield return new WaitForSeconds(0.12f);
            }

            break;
          case GravityStepEvent:
          case RefillEvent:
            if (!reduced)
            {
              yield return new WaitForSeconds(0.045f);
            }

            break;
          case CascadeEndedEvent:
            if (!reduced)
            {
              yield return new WaitForSeconds(0.06f);
            }

            break;
        }
      }

      _board.Refresh(stateAfter);
      IsPlaying = false;
      onComplete?.Invoke();
    }
  }
}
