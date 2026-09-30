using System;
using System.Collections;
using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;
using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kut.Unity.Presentation
{
  /// <summary>Canvas 8×8 board — tap, drag swap, animations (read-only state).</summary>
  public sealed class BoardGridUi : MonoBehaviour
  {
    private sealed class CellSlot
    {
      public GridPos Pos;
      public Button Button = null!;
      public Image Image = null!;
      public RectTransform Rect = null!;
      public Outline Outline = null!;
    }

    private readonly List<CellSlot> _cells = new List<CellSlot>();
    private GridPos? _selected;
    private BoardState? _state;
    private int _width;
    private int _height;
    private GridPos? _lastSwapFrom;
    private GridPos? _lastSwapTo;
    private GridPos? _pointerDown;
    private Transform? _gridRoot;

    public bool InputLocked { get; set; }

    public event Action<GridPos, GridPos>? SwapRequested;
    public event Action<GridPos>? ActivateRequested;

    public void Build(BoardState state)
    {
      for (var i = transform.childCount - 1; i >= 0; i--)
      {
        Destroy(transform.GetChild(i).gameObject);
      }

      _cells.Clear();
      _selected = null;
      _state = state;
      _width = state.Size.Width;
      _height = state.Size.Height;

      var tray = KutArtCatalog.TryBoardTray();
      if (tray != null)
      {
        var trayGo = new GameObject("BoardTray", typeof(RectTransform), typeof(Image));
        trayGo.transform.SetParent(transform, false);
        var trt = trayGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.02f, 0.08f);
        trt.anchorMax = new Vector2(0.98f, 0.92f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var img = trayGo.GetComponent<Image>();
        img.sprite = tray;
        img.color = Color.white;
        img.preserveAspect = false;
        img.raycastTarget = false;
      }

      var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
      gridGo.transform.SetParent(transform, false);
      _gridRoot = gridGo.transform;
      var rt = gridGo.GetComponent<RectTransform>();
      rt.anchorMin = new Vector2(0.06f, 0.12f);
      rt.anchorMax = new Vector2(0.94f, 0.88f);
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      var layout = gridGo.GetComponent<GridLayoutGroup>();
      layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
      layout.constraintCount = _width;
      layout.spacing = new Vector2(6, 6);
      layout.childAlignment = TextAnchor.MiddleCenter;
      var hostWidth = ((RectTransform)transform).rect.width;
      if (hostWidth < 100f)
      {
        hostWidth = 900f;
      }

      var cellSize = Mathf.Floor((hostWidth * 0.88f - layout.spacing.x * (_width - 1)) / _width);
      cellSize = Mathf.Clamp(cellSize, 48f, 120f);
      layout.cellSize = new Vector2(cellSize, cellSize);

      for (var y = 0; y < _height; y++)
      {
        for (var x = 0; x < _width; x++)
        {
          var pos = new GridPos(x, y);
          _cells.Add(CreateCell(gridGo.transform, pos));
        }
      }

      Refresh(state);
    }

    public void Refresh(BoardState state)
    {
      _state = state;
      for (var i = 0; i < _cells.Count; i++)
      {
        var slot = _cells[i];
        var cell = state.GetCell(slot.Pos);
        ApplyCellVisual(slot.Image, cell);
        slot.Outline.enabled = _selected.HasValue && _selected.Value.Equals(slot.Pos);
      }
    }

    public IEnumerator AnimateSwap(GridPos from, GridPos to, float duration)
    {
      _lastSwapFrom = from;
      _lastSwapTo = to;
      var a = GetSlot(from);
      var b = GetSlot(to);
      if (a == null || b == null || duration <= 0f)
      {
        yield break;
      }

      var posA = a.Rect.anchoredPosition;
      var posB = b.Rect.anchoredPosition;
      var t = 0f;
      while (t < duration)
      {
        t += Time.deltaTime;
        var k = Mathf.Clamp01(t / duration);
        a.Rect.anchoredPosition = Vector2.Lerp(posA, posB, k);
        b.Rect.anchoredPosition = Vector2.Lerp(posB, posA, k);
        yield return null;
      }

      a.Rect.anchoredPosition = posA;
      b.Rect.anchoredPosition = posB;
    }

    public IEnumerator AnimateSwapRevert(float duration)
    {
      if (!_lastSwapFrom.HasValue || !_lastSwapTo.HasValue)
      {
        yield break;
      }

      yield return AnimateSwap(_lastSwapTo.Value, _lastSwapFrom.Value, duration);
      yield return ShakeCells(_lastSwapFrom.Value, _lastSwapTo.Value, duration * 0.5f);
    }

    public IEnumerator FlashClear(IReadOnlyList<GridPos> cells, float duration, float stagger)
    {
      for (var i = 0; i < cells.Count; i++)
      {
        var slot = GetSlot(cells[i]);
        if (slot != null)
        {
          StartCoroutine(FlashCell(slot.Image, duration));
        }

        if (stagger > 0f)
        {
          yield return new WaitForSeconds(stagger);
        }
      }

      if (duration > 0f)
      {
        yield return new WaitForSeconds(duration);
      }
    }

    public void PlayInvalidSwapFeedback(GridPos a, GridPos b)
    {
      StartCoroutine(ShakeCells(a, b, 0.16f));
    }

    private IEnumerator FlashCell(Image img, float duration)
    {
      if (duration <= 0f)
      {
        yield break;
      }

      var orig = img.color;
      img.color = Color.white;
      yield return new WaitForSeconds(duration);
      img.color = orig;
    }

    private IEnumerator ShakeCells(GridPos a, GridPos b, float duration)
    {
      var sa = GetSlot(a);
      var sb = GetSlot(b);
      if (sa == null || sb == null)
      {
        yield break;
      }

      var ta = sa.Rect.anchoredPosition;
      var tb = sb.Rect.anchoredPosition;
      var t = 0f;
      while (t < duration)
      {
        t += Time.deltaTime;
        var n = Mathf.Sin(t * 40f) * 4f;
        sa.Rect.anchoredPosition = ta + new Vector2(n, 0);
        sb.Rect.anchoredPosition = tb + new Vector2(-n, 0);
        yield return null;
      }

      sa.Rect.anchoredPosition = ta;
      sb.Rect.anchoredPosition = tb;
    }

    private CellSlot CreateCell(Transform parent, GridPos pos)
    {
      var go = new GameObject($"Cell_{pos.X}_{pos.Y}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
      go.transform.SetParent(parent, false);
      var img = go.GetComponent<Image>();
      img.color = KutDesignTokens.BackgroundDeep;
      var outline = go.GetComponent<Outline>();
      outline.effectColor = KutDesignTokens.AccentGold;
      outline.effectDistance = new Vector2(3, 3);
      outline.enabled = false;

      var trigger = go.AddComponent<EventTrigger>();
      AddPointer(trigger, EventTriggerType.PointerDown, _ => OnPointerDown(pos));
      AddPointer(trigger, EventTriggerType.PointerUp, _ => OnPointerUp(pos));
      AddPointer(trigger, EventTriggerType.BeginDrag, _ => { });
      AddPointer(trigger, EventTriggerType.Drag, data => OnDrag(pos, data));

      var btn = go.GetComponent<Button>();
      btn.onClick.AddListener(() => OnCellClicked(pos));

      return new CellSlot
      {
        Pos = pos,
        Button = btn,
        Image = img,
        Rect = go.GetComponent<RectTransform>(),
        Outline = outline
      };
    }

    private static void AddPointer(EventTrigger trigger, EventTriggerType type, Action<BaseEventData> handler)
    {
      var entry = new EventTrigger.Entry { eventID = type };
      entry.callback.AddListener(handler.Invoke);
      trigger.triggers.Add(entry);
    }

    private void OnPointerDown(GridPos pos)
    {
      if (InputLocked)
      {
        return;
      }

      _pointerDown = pos;
    }

    private void OnPointerUp(GridPos pos)
    {
      _pointerDown = null;
    }

    private void OnDrag(GridPos origin, BaseEventData data)
    {
      if (InputLocked || !_pointerDown.HasValue || data is not PointerEventData ped)
      {
        return;
      }

      var delta = ped.position - ped.pressPosition;
      if (delta.magnitude < 32f)
      {
        return;
      }

      GridPos? target = null;
      if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
      {
        target = origin.Offset(delta.x > 0 ? 1 : -1, 0);
      }
      else
      {
        target = origin.Offset(0, delta.y > 0 ? -1 : 1);
      }

      if (target.HasValue && IsAdjacent(origin, target.Value))
      {
        _pointerDown = null;
        SwapRequested?.Invoke(origin, target.Value);
      }
    }

    private void OnCellClicked(GridPos pos)
    {
      if (InputLocked || _state == null)
      {
        return;
      }

      var cell = _state.GetCell(pos);
      if (!_selected.HasValue)
      {
        _selected = pos;
        Refresh(_state);
        return;
      }

      if (_selected.Value.Equals(pos))
      {
        if (cell.Kind == CellKind.Tile && cell.Tile?.Special != SpecialType.None)
        {
          ActivateRequested?.Invoke(pos);
        }

        _selected = null;
        Refresh(_state);
        return;
      }

      if (IsAdjacent(_selected.Value, pos))
      {
        SwapRequested?.Invoke(_selected.Value, pos);
      }

      _selected = null;
      Refresh(_state);
    }

    private CellSlot? GetSlot(GridPos pos)
    {
      for (var i = 0; i < _cells.Count; i++)
      {
        if (_cells[i].Pos.Equals(pos))
        {
          return _cells[i];
        }
      }

      return null;
    }

    private static bool IsAdjacent(GridPos a, GridPos b) =>
      (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y)) == 1;

    private static void ApplyCellVisual(Image img, Cell cell)
    {
      Sprite? sprite = null;
      switch (cell.Kind)
      {
        case CellKind.Tile:
          var tile = cell.Tile!;
          sprite = KutArtCatalog.TrySpecial(tile) ?? KutArtCatalog.TryTile(tile.TileId);
          break;
        case CellKind.Obstacle when cell.Obstacle != null:
          sprite = KutArtCatalog.TryObstacle(cell.Obstacle.Type);
          break;
        case CellKind.Blocker:
          sprite = KutArtCatalog.TryBlocker();
          break;
      }

      if (sprite != null)
      {
        img.sprite = sprite;
        img.color = Color.white;
        img.preserveAspect = true;
        return;
      }

      img.sprite = null;
      img.color = ColorForCell(cell);
    }

    private static Color ColorForCell(Cell cell)
    {
      switch (cell.Kind)
      {
        case CellKind.Empty:
          return KutDesignTokens.BackgroundDeep;
        case CellKind.Blocker:
          return Hex("#5C5C66");
        case CellKind.Obstacle:
          return cell.Obstacle?.Type == ObstacleType.Mud ? Hex("#6B4A2E") : Hex("#2E7D4A");
        case CellKind.Tile:
          var tile = cell.Tile!;
          if (tile.Special == SpecialType.WindChime)
          {
            return KutDesignTokens.AccentGold;
          }

          if (tile.Special == SpecialType.ShamanDrum)
          {
            return Hex("#9B3A6E");
          }

          if (tile.Special == SpecialType.FireBomb)
          {
            return Hex("#FF6B2C");
          }

          return tile.Element switch
          {
            Element.Water => Hex("#3B7BDB"),
            Element.Fire => Hex("#E85D3B"),
            Element.Air => Hex("#7EC8E3"),
            _ => Hex("#4CAF6A")
          };
        default:
          return Color.magenta;
      }
    }

    private static Color Hex(string hex)
    {
      ColorUtility.TryParseHtmlString(hex, out var c);
      return c;
    }
  }
}
