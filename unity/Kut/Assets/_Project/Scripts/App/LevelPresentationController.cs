using System;
using Kut.Core.Commands;
using Kut.Core.Levels;
using Kut.Core.Save;
using Kut.Unity.Presentation;
using Kut.Unity.Services;
using UnityEngine;

namespace Kut.Unity.App
{
  /// <summary>Single path: submit command → animate events → refresh board + HUD.</summary>
  public sealed class LevelPresentationController : MonoBehaviour
  {
    private PresentationAnimationQueue _animationQueue = null!;
    private BoardGridUi _boardGrid = null!;
    private SaveData _save = null!;
    private IAudioService _audio = null!;
    private IHapticsService _haptics = null!;

    public bool IsPlaying => _animationQueue != null && _animationQueue.IsPlaying;

    public void Init(
      BoardGridUi board,
      PresentationAnimationQueue queue,
      SaveData save,
      IAudioService audio,
      IHapticsService haptics)
    {
      _boardGrid = board;
      _animationQueue = queue;
      _save = save;
      _audio = audio;
      _haptics = haptics;
      var vfx = board.GetComponent<BoardVfxOverlay>();
      _animationQueue.Configure(board, save, audio, haptics, vfx);
    }

    public event Action? InvalidSwap;

    public void Submit(LevelSession session, IGameCommand command, Action onComplete)
    {
      if (session == null || _boardGrid == null || _animationQueue == null)
      {
        return;
      }

      _boardGrid.InputLocked = true;
      var result = session.Submit(command);
      var hadRevert = false;
      foreach (var evt in result.Events)
      {
        if (evt.EventType == "swap_reverted")
        {
          hadRevert = true;
        }
      }

      if (command is SwapCommand swap && hadRevert)
      {
        _boardGrid.PlayInvalidSwapFeedback(swap.From, swap.To);
      }

      _animationQueue.Play(result.Events, session.Engine.State, () =>
      {
        _boardGrid.Refresh(session.Engine.State);
        _boardGrid.InputLocked = false;
        if (hadRevert)
        {
          InvalidSwap?.Invoke();
        }

        onComplete?.Invoke();
      });
    }
  }
}
