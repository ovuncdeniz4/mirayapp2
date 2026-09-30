using System;
using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;
using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.UI;

namespace Kut.Unity.Presentation
{
  /// <summary>
  /// Canvas-based 8×8 board per visual-design-system (touch select + swap / activate).
  /// </summary>
  public sealed class BoardGridUi : MonoBehaviour
  {
    private readonly List<Button> _cells = new List<Button>();
    private GridPos? _selected;
    private BoardState? _state;
    private int _width;
    private int _height;

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
          var cell = CreateCell(gridGo.transform, pos);
          _cells.Add(cell);
        }
      }

      Refresh(state);
    }

    public void Refresh(BoardState state)
    {
      _state = state;
      var idx = 0;
      for (var y = 0; y < _height; y++)
      {
        for (var x = 0; x < _width; x++)
        {
          var pos = new GridPos(x, y);
          var cell = state.GetCell(pos);
          ApplyCellVisual(_cells[idx].GetComponent<Image>(), cell);
          var highlight = _selected.HasValue && _selected.Value.Equals(pos);
          _cells[idx].GetComponent<Outline>().enabled = highlight;
          idx++;
        }
      }
    }

    private Button CreateCell(Transform parent, GridPos pos)
    {
      var go = new GameObject($"Cell_{pos.X}_{pos.Y}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
      go.transform.SetParent(parent, false);
      var img = go.GetComponent<Image>();
      img.color = KutDesignTokens.BackgroundDeep;
      var outline = go.GetComponent<Outline>();
      outline.effectColor = KutDesignTokens.AccentGold;
      outline.effectDistance = new Vector2(3, 3);
      outline.enabled = false;

      var btn = go.GetComponent<Button>();
      btn.onClick.AddListener(() => OnCellClicked(pos));
      return btn;
    }

    private void OnCellClicked(GridPos pos)
    {
      if (_state == null)
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
