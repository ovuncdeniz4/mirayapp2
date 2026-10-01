using System.Collections;
using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.GameEvents;
using Kut.Core.Save;
using Kut.Core.Tiles;
using Kut.Unity.Services;
using UnityEngine;

namespace Kut.Unity.Presentation
{
  /// <summary>Replays Kut.Core events with Master Plan §22 timing on BoardGridUi + VFX overlay.</summary>
  public sealed class PresentationAnimationQueue : MonoBehaviour
  {
    private BoardGridUi _board = null!;
    private BoardVfxOverlay? _vfx;
    private SaveData _save = null!;
    private IAudioService _audio = null!;
    private IHapticsService _haptics = null!;
    private int _cascadeDepth;

    public bool IsPlaying { get; private set; }

    public void Configure(BoardGridUi board, SaveData save, IAudioService audio, IHapticsService haptics, BoardVfxOverlay? vfx = null)
    {
      _board = board;
      _save = save;
      _audio = audio;
      _haptics = haptics;
      _vfx = vfx;
      if (_vfx != null)
      {
        _vfx.Bind(board);
      }
    }

    public void Play(IReadOnlyList<GameEvent> events, BoardState stateAfter, System.Action onComplete)
    {
      StopAllCoroutines();
      StartCoroutine(PlayRoutine(events, stateAfter, onComplete));
    }

    private IEnumerator PlayRoutine(IReadOnlyList<GameEvent> events, BoardState stateAfter, System.Action onComplete)
    {
      IsPlaying = true;
      _cascadeDepth = 0;
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
            _audio?.PlayMatch(Element.Earth);
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
              _audio?.PlaySpecial(SpecialType.FireBomb, null);
            }
            else if (sa.Special == SpecialType.ShamanDrum)
            {
              _haptics?.Medium();
              _audio?.PlaySpecial(SpecialType.ShamanDrum, Element.Earth);
            }
            else
            {
              _haptics?.Medium();
              _audio?.PlaySpecial(SpecialType.WindChime, null);
            }

            if (!reduced && _vfx != null)
            {
              if (sa.Special == SpecialType.ShamanDrum)
              {
                yield return _vfx.PlayDrumPulse(sa.At, new Color(0.3f, 0.67f, 0.42f, 0.8f), 0.35f);
              }
              else if (sa.Special == SpecialType.FireBomb)
              {
                yield return _vfx.PlayBombRings(sa.At, 0.3f);
              }
            }
            else if (!reduced)
            {
              yield return new WaitForSeconds(sa.Special == SpecialType.ShamanDrum ? 0.35f : 0.28f);
            }

            break;
          case LineClearEvent line:
            if (!reduced && _vfx != null)
            {
              yield return _vfx.PlayLineClear(line.IsRow, line.Index, 0.25f);
            }
            else if (!reduced)
            {
              yield return new WaitForSeconds(0.25f);
            }

            break;
          case AreaClearEvent area:
            if (!reduced && _vfx != null)
            {
              yield return _vfx.PlayBombRings(area.Center, 0.3f);
            }

            break;
          case MudCleansedEvent mud:
            if (!reduced && _vfx != null)
            {
              yield return _vfx.PlayBurst(mud.At, new Color(0.42f, 0.29f, 0.18f, 0.9f), 0.12f);
            }

            break;
          case VineBrokenEvent vine:
            if (!reduced && _vfx != null)
            {
              yield return _vfx.PlayBurst(vine.At, new Color(0.18f, 0.49f, 0.29f, 0.9f), 0.12f);
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
            _cascadeDepth++;
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

    public int LastCascadeDepth => _cascadeDepth;
  }
}
