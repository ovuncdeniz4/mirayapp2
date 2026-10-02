using System.Collections.Generic;
using System.Linq;
using Kut.Core.Commands;
using Kut.Core.Elements;
using Kut.Core.GameEvents;
using Kut.Core.Levels;
using Kut.Core.Match;
using Kut.Core.Random;
using Kut.Core.Specials;
using Kut.Core.Tiles;

namespace Kut.Core.Board
{
  public sealed class BoardEngine
  {
    private readonly List<string> _spawnTable;
    private readonly DeterministicRandom _rng;
    private GridPos? _lastSwapDestination;

    public BoardState State { get; }
    public LevelRules Rules { get; }
    public int MovesRemaining { get; private set; }

    public BoardEngine(BoardState state, LevelRules rules, IEnumerable<string>? spawnTable = null)
    {
      State = state;
      Rules = rules;
      MovesRemaining = rules.Moves;
      _spawnTable = spawnTable?.ToList() ?? TileRegistry.DefaultSpawnTable.ToList();
      _rng = new DeterministicRandom(rules.Seed);
    }

    public static BoardEngine CreatePrototype()
    {
      var def = new LevelDefinition
      {
        Id = "level_prototype",
        BoardWidth = 8,
        BoardHeight = 8,
        Moves = 30,
        Seed = 42,
        EnableWindChime = true
      };
      def.MudCells.Add(new MudCellDefinition { X = 3, Y = 3 });
      def.MudCells.Add(new MudCellDefinition { X = 4, Y = 3 });
      def.MudCells.Add(new MudCellDefinition { X = 3, Y = 4 });
      def.MudCells.Add(new MudCellDefinition { X = 4, Y = 4 });
      return LevelLoader.CreateEngine(def);
    }

    public CommandResult Apply(IGameCommand command)
    {
      var events = new List<GameEvent>();
      CommandStatus status;
      switch (command)
      {
        case SwapCommand swap:
          status = ApplySwap(swap, events);
          break;
        case ActivateSpecialCommand activate:
          status = ApplyActivateSpecial(activate, events);
          break;
        default:
          return new CommandResult { Status = CommandStatus.RejectedInvalidTarget, Events = events };
      }

      return new CommandResult { Status = status, Events = events };
    }

    private CommandStatus ApplySwap(SwapCommand swap, List<GameEvent> events)
    {
      events.Add(new SwapAttemptedEvent(swap.From, swap.To));
      if (!State.Size.Contains(swap.From) || !State.Size.Contains(swap.To))
      {
        return CommandStatus.RejectedInvalidPosition;
      }

      if (!State.CanSwap(swap.From, swap.To))
      {
        return CommandStatus.RejectedInvalidTarget;
      }

      State.SwapTiles(swap.From, swap.To);
      _lastSwapDestination = swap.To;

      if (MatchDetection.FindMatchedCells(State).Count == 0)
      {
        State.SwapTiles(swap.From, swap.To);
        _lastSwapDestination = null;
        events.Add(new SwapRevertedEvent());
        return CommandStatus.Reverted;
      }

      MovesRemaining--;
      events.Add(new MoveConsumedEvent(MovesRemaining));
      ResolveUntilStable(events);
      _lastSwapDestination = null;
      EnsurePlayableBoard(events);
      return CommandStatus.Applied;
    }

    private CommandStatus ApplyActivateSpecial(ActivateSpecialCommand activate, List<GameEvent> events)
    {
      if (!State.Size.Contains(activate.At))
      {
        return CommandStatus.RejectedInvalidPosition;
      }

      var cell = State.GetCell(activate.At);
      if (cell.Kind != CellKind.Tile || cell.Tile == null)
      {
        return CommandStatus.RejectedInvalidTarget;
      }

      if (cell.Tile.Special == SpecialType.WindChime)
      {
        WindChimeRules.Activate(State, activate.At, events, out var toClear);
        ClearTilesWithReactions(toClear, events);
      }
      else if (cell.Tile.Special == SpecialType.ShamanDrum)
      {
        ShamanDrumRules.Activate(State, activate.At, events, out var toClear);
        ClearTilesWithReactions(toClear, events);
      }
      else if (cell.Tile.Special == SpecialType.FireBomb)
      {
        FireBombRules.Activate(State, activate.At, events, out var toClear);
        ClearTilesWithReactions(toClear, events);
      }
      else
      {
        return CommandStatus.RejectedInvalidTarget;
      }

      MovesRemaining--;
      events.Add(new MoveConsumedEvent(MovesRemaining));
      GravitySystem.Apply(State, events);
      RefillSystem.RefillColumns(State, _rng, _spawnTable, events);
      ResolveUntilStable(events);
      EnsurePlayableBoard(events);
      return CommandStatus.Applied;
    }

    private void EnsurePlayableBoard(List<GameEvent> events)
    {
      if (BoardLegalMoves.HasLegalMove(State))
      {
        return;
      }

      if (!BoardLegalMoves.TryReshuffle(State, _rng) &&
          !BoardLegalMoves.TryRegenerate(State, _rng, _spawnTable))
      {
        throw new System.InvalidOperationException("Board has no legal move and could not be reshuffled.");
      }

      events.Add(new BoardReshuffledEvent());
    }

    public void AddBonusMoves(int amount)
    {
      if (amount > 0)
      {
        MovesRemaining += amount;
      }
    }

    private void ResolveUntilStable(List<GameEvent> events)
    {
      var chain = 0;
      while (true)
      {
        var matched = MatchDetection.FindMatchedCells(State);
        if (matched.Count == 0)
        {
          events.Add(new CascadeEndedEvent(chain));
          break;
        }

        chain++;
        var matchElement = matched.Select(p => State.GetCell(p).Tile?.Element)
          .FirstOrDefault(e => e.HasValue) ?? Element.Earth;
        events.Add(new MatchFoundEvent(matched.ToList(), matchElement));
        ProcessMatchClear(matched, events);
        GravitySystem.Apply(State, events);
        RefillSystem.RefillColumns(State, _rng, _spawnTable, events);
      }
    }

    private void ProcessMatchClear(HashSet<GridPos> matched, List<GameEvent> events)
    {
      GridPos? specialSpawn = null;
      LineOrientation? lineOrientation = null;
      SpecialCreationKind creationKind = SpecialCreationKind.None;
      string resonanceGroup = "";

      if (Rules.EnableSpecialCreation)
      {
        creationKind = SpecialCreationResolver.Resolve(
          State,
          matched,
          _lastSwapDestination,
          Rules.EnableShamanDrum,
          Rules.EnableWindChime,
          Rules.EnableFireBomb,
          out var spawn,
          out var orientation,
          out var resonance);
        if (creationKind != SpecialCreationKind.None)
        {
          specialSpawn = spawn;
          lineOrientation = orientation;
          resonanceGroup = resonance;
        }
      }

      TileInstance? spawnBase = null;
      if (specialSpawn.HasValue)
      {
        spawnBase = State.GetCell(specialSpawn.Value).Tile?.Clone();
      }

      var toClear = matched.Where(p => !specialSpawn.HasValue || p != specialSpawn.Value).ToList();
      ClearTilesWithReactions(toClear, events);

      if (specialSpawn.HasValue && creationKind == SpecialCreationKind.WindChime)
      {
        var tile = spawnBase ?? TileRegistry.Create("earth_moss");
        tile.Special = SpecialType.WindChime;
        tile.ChimeOrientation = lineOrientation;
        State.SetCell(specialSpawn.Value, Cell.FromTile(tile));
        events.Add(new SpecialCreatedEvent(specialSpawn.Value, SpecialType.WindChime, lineOrientation));
      }
      else if (specialSpawn.HasValue && creationKind == SpecialCreationKind.ShamanDrum)
      {
        var tile = spawnBase ?? TileRegistry.Create("earth_moss");
        tile.Special = SpecialType.ShamanDrum;
        tile.ResonanceMatchGroup = string.IsNullOrEmpty(resonanceGroup) ? tile.MatchGroup : resonanceGroup;
        State.SetCell(specialSpawn.Value, Cell.FromTile(tile));
        events.Add(new SpecialCreatedEvent(specialSpawn.Value, SpecialType.ShamanDrum, lineOrientation));
      }
      else if (specialSpawn.HasValue && creationKind == SpecialCreationKind.FireBomb)
      {
        var tile = spawnBase ?? TileRegistry.Create("fire_ember");
        tile.Special = SpecialType.FireBomb;
        State.SetCell(specialSpawn.Value, Cell.FromTile(tile));
        events.Add(new SpecialCreatedEvent(specialSpawn.Value, SpecialType.FireBomb, null));
      }
    }

    private void ClearTilesWithReactions(List<GridPos> positions, List<GameEvent> events)
    {
      if (positions.Count == 0)
      {
        return;
      }

      var waterSources = new List<GridPos>();
      var earthSources = new List<GridPos>();
      var fireSources = new List<GridPos>();
      var earthCount = 0;
      var waterCount = 0;
      var fireCount = 0;
      var windCount = 0;
      events.Add(new TilesClearedEvent(positions));

      foreach (var pos in positions)
      {
        var cell = State.GetCell(pos);
        if (cell.Kind != CellKind.Tile || cell.Tile == null)
        {
          continue;
        }

        if (cell.Tile.Element == Element.Water)
        {
          waterSources.Add(pos);
          waterCount++;
        }
        else if (cell.Tile.Element == Element.Earth)
        {
          earthSources.Add(pos);
          earthCount++;
        }
        else if (cell.Tile.Element == Element.Fire)
        {
          fireSources.Add(pos);
          fireCount++;
        }
        else if (cell.Tile.Element == Element.Air)
        {
          windCount++;
        }

        State.SetCell(pos, Cell.Empty());
      }

      if (earthCount > 0)
      {
        events.Add(new ElementCollectedEvent(Element.Earth, earthCount));
      }

      if (waterCount > 0)
      {
        events.Add(new ElementCollectedEvent(Element.Water, waterCount));
      }

      if (fireCount > 0)
      {
        events.Add(new ElementCollectedEvent(Element.Fire, fireCount));
      }

      if (windCount > 0)
      {
        events.Add(new ElementCollectedEvent(Element.Air, windCount));
      }

      ElementReactionSystem.ApplyWaterMudReactions(State, waterSources, events);
      ElementReactionSystem.ApplyEarthVineReactions(State, earthSources, events);
      ElementReactionSystem.ApplyFireBurnReactions(State, fireSources, events);
    }
  }
}
