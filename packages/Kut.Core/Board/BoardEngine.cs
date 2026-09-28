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
      switch (command)
      {
        case SwapCommand swap:
          ApplySwap(swap, events);
          break;
        case ActivateSpecialCommand activate:
          ApplyActivateSpecial(activate, events);
          break;
        default:
          return new CommandResult { Success = false, Events = events };
      }

      return new CommandResult { Success = true, Events = events };
    }

    private void ApplySwap(SwapCommand swap, List<GameEvent> events)
    {
      events.Add(new SwapAttemptedEvent(swap.From, swap.To));
      if (!State.CanSwap(swap.From, swap.To))
      {
        return;
      }

      State.SwapTiles(swap.From, swap.To);
      _lastSwapDestination = swap.To;

      if (MatchDetection.FindMatchedCells(State).Count == 0)
      {
        State.SwapTiles(swap.From, swap.To);
        _lastSwapDestination = null;
        events.Add(new SwapRevertedEvent());
        return;
      }

      MovesRemaining--;
      events.Add(new MoveConsumedEvent(MovesRemaining));
      ResolveUntilStable(events);
      _lastSwapDestination = null;
    }

    private void ApplyActivateSpecial(ActivateSpecialCommand activate, List<GameEvent> events)
    {
      var cell = State.GetCell(activate.At);
      if (cell.Kind != CellKind.Tile || cell.Tile?.Special != SpecialType.WindChime)
      {
        return;
      }

      WindChimeRules.Activate(State, activate.At, events, out var toClear);
      ClearTilesWithReactions(toClear, events);
      ResolveUntilStable(events);
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
        events.Add(new MatchFoundEvent(matched.ToList()));
        ProcessMatchClear(matched, events);
        GravitySystem.Apply(State, events);
        RefillSystem.RefillColumns(State, _rng, _spawnTable, events);
      }
    }

    private void ProcessMatchClear(HashSet<GridPos> matched, List<GameEvent> events)
    {
      GridPos? specialSpawn = null;
      LineOrientation? chimeOrientation = null;

      if (Rules.EnableSpecialCreation && Rules.EnableWindChime &&
          SpecialCreationResolver.TryResolveWindChime(State, matched, _lastSwapDestination, out var spawn, out var orientation))
      {
        specialSpawn = spawn;
        chimeOrientation = orientation;
      }

      TileInstance? spawnBase = null;
      if (specialSpawn.HasValue)
      {
        spawnBase = State.GetCell(specialSpawn.Value).Tile?.Clone();
      }

      var toClear = matched.Where(p => !specialSpawn.HasValue || p != specialSpawn.Value).ToList();
      ClearTilesWithReactions(toClear, events);

      if (specialSpawn.HasValue)
      {
        var tile = spawnBase ?? TileRegistry.Create("earth_moss");
        tile.Special = SpecialType.WindChime;
        tile.ChimeOrientation = chimeOrientation;
        State.SetCell(specialSpawn.Value, Cell.FromTile(tile));
        events.Add(new SpecialCreatedEvent(specialSpawn.Value, SpecialType.WindChime, chimeOrientation));
      }
    }

    private void ClearTilesWithReactions(List<GridPos> positions, List<GameEvent> events)
    {
      if (positions.Count == 0)
      {
        return;
      }

      var waterSources = new List<GridPos>();
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
        }

        State.SetCell(pos, Cell.Empty());
      }

      ElementReactionSystem.ApplyWaterMudReactions(State, waterSources, events);
    }
  }
}
