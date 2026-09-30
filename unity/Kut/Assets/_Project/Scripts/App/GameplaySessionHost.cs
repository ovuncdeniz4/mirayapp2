using Kut.Unity.Presentation;
using UnityEngine;

namespace Kut.Unity.App
{
  /// <summary>
  /// Legacy Stage-A world-space board host. Canonical UX uses KutAppBootstrap + BoardGridUi (uGUI).
  /// </summary>
  public sealed class GameplaySessionHost : MonoBehaviour
  {
    [SerializeField] private TextAsset levelJson = null!;
    [SerializeField] private BoardView boardView = null!;

    public GameplaySession Session { get; private set; } = null!;

    private void Awake()
    {
      Session = levelJson != null
        ? GameplaySession.FromLevelJson(levelJson.text)
        : GameplaySession.CreatePrototype();

      if (boardView != null)
      {
        boardView.BindState(Session.LevelSession.Engine.State);
      }
    }

    public void RefreshBoardAfterCommand()
    {
      if (boardView != null)
      {
        boardView.BindState(Session.LevelSession.Engine.State);
      }
    }
  }
}
