using UnityEngine;

namespace Kut.Unity.App
{
  /// <summary>
  /// MonoBehaviour host for prototype Stage A scene wiring.
  /// </summary>
  public sealed class GameplaySessionHost : MonoBehaviour
  {
    public GameplaySession Session { get; private set; } = null!;

    private void Awake()
    {
      Session = GameplaySession.CreatePrototype();
    }
  }
}
