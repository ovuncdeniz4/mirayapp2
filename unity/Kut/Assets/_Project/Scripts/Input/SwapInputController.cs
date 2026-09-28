using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Unity.App;
using Kut.Unity.Presentation;
using UnityEngine;

namespace Kut.Unity.Input
{
  /// <summary>
  /// Stage A: maps swipe to SwapCommand via GameplaySession.
  /// </summary>
  public sealed class SwapInputController : MonoBehaviour
  {
    [SerializeField] private GameplaySessionHost sessionHost = null!;
    [SerializeField] private BoardView boardView = null!;

    private GridPos? _pressStart;

    public void OnCellPressed(int x, int y)
    {
      _pressStart = new GridPos(x, y);
    }

    public void OnCellReleased(int x, int y)
    {
      if (!_pressStart.HasValue || sessionHost == null || boardView == null)
      {
        return;
      }

      var end = new GridPos(x, y);
      if (_pressStart.Value == end)
      {
        _pressStart = null;
        return;
      }

      var result = sessionHost.Session.SubmitAndRemember(new SwapCommand(_pressStart.Value, end));
      boardView.ReplayEvents(result.Events);
      _pressStart = null;
    }
  }
}
