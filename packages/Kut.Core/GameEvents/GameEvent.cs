using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.Tiles;

namespace Kut.Core.GameEvents
{
  public abstract class GameEvent
  {
    public string EventType { get; protected set; } = "";
  }

  public sealed class SwapAttemptedEvent : GameEvent
  {
    public SwapAttemptedEvent(GridPos from, GridPos to)
    {
      EventType = "swap_attempted";
      From = from;
      To = to;
    }

    public GridPos From { get; }
    public GridPos To { get; }
  }

  public sealed class SwapRevertedEvent : GameEvent
  {
    public SwapRevertedEvent()
    {
      EventType = "swap_reverted";
    }
  }

  public sealed class MoveConsumedEvent : GameEvent
  {
    public MoveConsumedEvent(int remaining)
    {
      EventType = "move_consumed";
      RemainingMoves = remaining;
    }

    public int RemainingMoves { get; }
  }

  public sealed class MatchFoundEvent : GameEvent
  {
    public MatchFoundEvent(IReadOnlyList<GridPos> cells)
    {
      EventType = "match_found";
      Cells = cells;
    }

    public IReadOnlyList<GridPos> Cells { get; }
  }

  public sealed class TilesClearedEvent : GameEvent
  {
    public TilesClearedEvent(IReadOnlyList<GridPos> cells)
    {
      EventType = "tiles_cleared";
      Cells = cells;
    }

    public IReadOnlyList<GridPos> Cells { get; }
  }

  public sealed class SpecialCreatedEvent : GameEvent
  {
    public SpecialCreatedEvent(GridPos at, SpecialType special, LineOrientation? orientation)
    {
      EventType = "special_created";
      At = at;
      Special = special;
      Orientation = orientation;
    }

    public GridPos At { get; }
    public SpecialType Special { get; }
    public LineOrientation? Orientation { get; }
  }

  public sealed class SpecialActivatedEvent : GameEvent
  {
    public SpecialActivatedEvent(GridPos at, SpecialType special)
    {
      EventType = "special_activated";
      At = at;
      Special = special;
    }

    public GridPos At { get; }
    public SpecialType Special { get; }
  }

  public sealed class LineClearEvent : GameEvent
  {
    public LineClearEvent(bool isRow, int index)
    {
      EventType = "line_clear";
      IsRow = isRow;
      Index = index;
    }

    public bool IsRow { get; }
    public int Index { get; }
  }

  public sealed class ReactionSourceEvent : GameEvent
  {
    public ReactionSourceEvent(GridPos formerCell, Element element)
    {
      EventType = "reaction_source";
      FormerCell = formerCell;
      Element = element;
    }

    public GridPos FormerCell { get; }
    public Element Element { get; }
  }

  public sealed class MudCleansedEvent : GameEvent
  {
    public MudCleansedEvent(GridPos at, int layersRemaining)
    {
      EventType = "mud_cleansed";
      At = at;
      LayersRemaining = layersRemaining;
    }

    public GridPos At { get; }
    public int LayersRemaining { get; }
  }

  public sealed class GravityStepEvent : GameEvent
  {
    public GravityStepEvent(GridPos from, GridPos to)
    {
      EventType = "gravity_step";
      From = from;
      To = to;
    }

    public GridPos From { get; }
    public GridPos To { get; }
  }

  public sealed class RefillEvent : GameEvent
  {
    public RefillEvent(GridPos at, string tileId)
    {
      EventType = "refill";
      At = at;
      TileId = tileId;
    }

    public GridPos At { get; }
    public string TileId { get; }
  }

  public sealed class CascadeEndedEvent : GameEvent
  {
    public CascadeEndedEvent(int chainIndex)
    {
      EventType = "cascade_ended";
      ChainIndex = chainIndex;
    }

    public int ChainIndex { get; }
  }
}
