using System;
using System.Collections.Generic;
using Kut.Core.Progression;
using Kut.Core.Zodiac;

namespace Kut.Core.Board
{
  [Flags]
  public enum RuneTag
  {
    None = 0, Earth = 1, Water = 2, Fire = 4, Air = 8, Metal = 16, Spirit = 32,
    Seed = 64, Sun = 128, Slag = 256, Stone = 512, Apple = 1024, Sapphire = 2048
  }

  public sealed class RuneDefinition
  {
    public string Id { get; set; } = "";
    public RuneTag Tags { get; set; }
    public DamageElement Element { get; set; }
    public int BaseAttack { get; set; } = 10;
    public string MatchGroup { get; set; } = "";
    public string VisualId { get; set; } = "";
  }

  public sealed class MovableRune
  {
    public MovableRune(RuneDefinition definition, long instanceId)
    { Definition = definition; InstanceId = instanceId; }
    public RuneDefinition Definition { get; }
    public long InstanceId { get; }
    public SpecialTypeV2 Special { get; set; }
  }

  public enum SpecialTypeV2 { None, LineClear, RuneClear, RingBlast }

  public sealed class GroundLayerInstance
  {
    public ProgressiveObstacle? Progression { get; set; }
  }

  public sealed class OverlayObstacleInstance
  {
    public string Id { get; set; } = "";
    public int HitPoints { get; set; } = 1;
  }

  public sealed class FixedBlockerInstance
  {
    public string Id { get; set; } = "";
    public bool Damageable { get; set; }
  }

  public sealed class OccupantInstance
  {
    public string Id { get; set; } = "";
    public string GroupId { get; set; } = "";
    public bool BlocksMovement { get; set; } = true;
  }

  public sealed class LayeredBoardCell
  {
    public MovableRune? MovableTile { get; set; }
    public GroundLayerInstance? Ground { get; set; }
    public OverlayObstacleInstance? Overlay { get; set; }
    public FixedBlockerInstance? Blocker { get; set; }
    public OccupantInstance? Occupant { get; set; }
    public bool AcceptsMovableTile => Blocker == null && (Occupant == null || !Occupant.BlocksMovement);
  }

  public sealed class LayeredBoardState
  {
    private readonly LayeredBoardCell[,] _cells;
    public BoardSize Size { get; }

    public LayeredBoardState(BoardSize size)
    {
      Size = size;
      _cells = new LayeredBoardCell[size.Width, size.Height];
      for (var x = 0; x < size.Width; x++)
        for (var y = 0; y < size.Height; y++)
          _cells[x, y] = new LayeredBoardCell();
    }

    public LayeredBoardCell GetCell(GridPos position)
    {
      if (!Size.Contains(position)) throw new ArgumentOutOfRangeException(nameof(position));
      return _cells[position.X, position.Y];
    }

    public bool CanSwap(GridPos a, GridPos b)
    {
      if (!Size.Contains(a) || !Size.Contains(b) || Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) != 1) return false;
      var first = GetCell(a);
      var second = GetCell(b);
      return first.AcceptsMovableTile && second.AcceptsMovableTile && first.MovableTile != null && second.MovableTile != null &&
             first.Overlay == null && second.Overlay == null;
    }

    public void Swap(GridPos a, GridPos b)
    {
      if (!CanSwap(a, b)) throw new InvalidOperationException("Cells cannot be swapped.");
      var tile = GetCell(a).MovableTile;
      GetCell(a).MovableTile = GetCell(b).MovableTile;
      GetCell(b).MovableTile = tile;
    }

    public IEnumerable<(GridPos Position, LayeredBoardCell Cell)> Cells()
    {
      for (var y = 0; y < Size.Height; y++)
        for (var x = 0; x < Size.Width; x++)
        {
          var pos = new GridPos(x, y);
          yield return (pos, GetCell(pos));
        }
    }
  }
}
