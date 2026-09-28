using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.GameEvents;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;
using UnityEngine;

namespace Kut.Unity.Presentation
{
  /// <summary>
  /// Presentation-only board view. Never mutates BoardEngine — refreshes from read-only state.
  /// </summary>
  public sealed class BoardView : MonoBehaviour
  {
    [SerializeField] private int cellSize = 64;
    [SerializeField] private PresentationAnimationQueue animationQueue = null!;
    [SerializeField] private Transform cellRoot = null!;

    private readonly List<SpriteRenderer> _cells = new List<SpriteRenderer>();
    private BoardState? _lastState;

    public int CellSize => cellSize;

    public void BindState(BoardState state)
    {
      _lastState = state;
      EnsureGrid(state);
      RefreshFromState(state);
    }

    public void ReplayEvents(IReadOnlyList<GameEvent> events, BoardState stateAfter)
    {
      _lastState = stateAfter;
      if (animationQueue != null)
      {
        animationQueue.Play(events, () => RefreshFromState(stateAfter));
        return;
      }

      foreach (var evt in events)
      {
        Debug.Log($"[BoardView] {evt.EventType}");
      }

      RefreshFromState(stateAfter);
    }

    private void EnsureGrid(BoardState state)
    {
      if (_cells.Count > 0)
      {
        return;
      }

      if (cellRoot == null)
      {
        cellRoot = transform;
      }

      for (var y = 0; y < state.Size.Height; y++)
      {
        for (var x = 0; x < state.Size.Width; x++)
        {
          var go = new GameObject($"Cell_{x}_{y}", typeof(SpriteRenderer));
          go.transform.SetParent(cellRoot, false);
          go.transform.localPosition = new Vector3(x * cellSize, -y * cellSize, 0f);
          var sr = go.GetComponent<SpriteRenderer>();
          sr.sprite = CreateWhiteSprite();
          sr.drawMode = SpriteDrawMode.Sliced;
          sr.size = new Vector2(cellSize - 4, cellSize - 4);
          _cells.Add(sr);
        }
      }
    }

    private void RefreshFromState(BoardState state)
    {
      EnsureGrid(state);
      var idx = 0;
      for (var y = 0; y < state.Size.Height; y++)
      {
        for (var x = 0; x < state.Size.Width; x++)
        {
          _cells[idx].color = ColorForCell(state.GetCell(new GridPos(x, y)));
          idx++;
        }
      }
    }

    private static Color ColorForCell(Cell cell)
    {
      switch (cell.Kind)
      {
        case CellKind.Empty:
          return new Color(0.15f, 0.15f, 0.2f);
        case CellKind.Blocker:
          return new Color(0.35f, 0.35f, 0.4f);
        case CellKind.Obstacle:
          if (cell.Obstacle?.Type == ObstacleType.Mud)
          {
            return new Color(0.45f, 0.32f, 0.18f);
          }

          return cell.Obstacle?.Type == ObstacleType.Vine
            ? new Color(0.2f, 0.55f, 0.25f)
            : new Color(0.5f, 0.5f, 0.5f);
        case CellKind.Tile:
          var tile = cell.Tile!;
          if (tile.Special == SpecialType.WindChime)
          {
            return new Color(0.85f, 0.75f, 0.2f);
          }

          if (tile.Special == SpecialType.ShamanDrum)
          {
            return new Color(0.65f, 0.25f, 0.55f);
          }

          return tile.Element == Element.Water
            ? new Color(0.25f, 0.45f, 0.95f)
            : new Color(0.35f, 0.7f, 0.35f);
        default:
          return Color.magenta;
      }
    }

    private static Sprite? _whiteSprite;

    private static Sprite CreateWhiteSprite()
    {
      if (_whiteSprite != null)
      {
        return _whiteSprite;
      }

      var tex = new Texture2D(1, 1);
      tex.SetPixel(0, 0, Color.white);
      tex.Apply();
      _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
      return _whiteSprite;
    }
  }
}
