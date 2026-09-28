using System.Collections.Generic;
using Kut.Core.GameEvents;
using UnityEngine;

namespace Kut.Unity.Presentation
{
  /// <summary>
  /// Presentation-only board view. Never mutates BoardEngine — replays GameEvents.
  /// </summary>
  public sealed class BoardView : MonoBehaviour
  {
    [SerializeField] private int cellSize = 64;

    public int CellSize => cellSize;

    public void ReplayEvents(IReadOnlyList<GameEvent> events)
    {
      foreach (var evt in events)
      {
        Debug.Log($"[BoardView] {evt.EventType}");
      }
    }
  }
}
