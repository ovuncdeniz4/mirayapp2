#if DEVELOPMENT_BUILD || UNITY_EDITOR
using Kut.Core.Commands;
using Kut.Core.Levels;
using UnityEngine;

namespace Kut.Unity.App
{
  /// <summary>DEV: bonus moves and force win via keyboard.</summary>
  public sealed class DevDebugOverlay : MonoBehaviour
  {
    private KutAppBootstrap _bootstrap = null!;

    public void Bind(KutAppBootstrap bootstrap) => _bootstrap = bootstrap;

    private void Update()
    {
      if (_bootstrap == null)
      {
        return;
      }

      if (UnityEngine.Input.GetKeyDown(KeyCode.M))
      {
        _bootstrap.DevAddMoves(5);
      }

      if (UnityEngine.Input.GetKeyDown(KeyCode.W))
      {
        _bootstrap.DevForceWin();
      }
    }
  }
}
#endif
