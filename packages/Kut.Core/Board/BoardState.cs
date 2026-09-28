using System;
using Kut.Core.Obstacles;
using Kut.Core.Tiles;

namespace Kut.Core.Board
{
  public sealed class BoardState
  {
    private readonly Cell[,] _cells;

    public BoardSize Size { get; }

    public BoardState(BoardSize size)
    {
      Size = size;
      _cells = new Cell[size.Width, size.Height];
      for (var x = 0; x < size.Width; x++)
      {
        for (var y = 0; y < size.Height; y++)
        {
          _cells[x, y] = Cell.Empty();
        }
      }
    }

    public Cell GetCell(GridPos p) => _cells[p.X, p.Y];

    public void SetCell(GridPos p, Cell cell) => _cells[p.X, p.Y] = cell;

    public string? GetMatchGroup(GridPos p)
    {
      var cell = GetCell(p);
      if (cell.Kind != CellKind.Tile || cell.Tile == null)
      {
        return null;
      }

      return cell.Tile.MatchGroup;
    }

    public bool CanSwap(GridPos a, GridPos b)
    {
      if (!Size.Contains(a) || !Size.Contains(b))
      {
        return false;
      }

      if (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) != 1)
      {
        return false;
      }

      var ca = GetCell(a);
      var cb = GetCell(b);
      return ca.Kind == CellKind.Tile && cb.Kind == CellKind.Tile && ca.Tile != null && cb.Tile != null;
    }

    public void SwapTiles(GridPos a, GridPos b)
    {
      var ca = GetCell(a);
      var cb = GetCell(b);
      SetCell(a, Cell.FromTile(cb.Tile!));
      SetCell(b, Cell.FromTile(ca.Tile!));
    }
  }
}
