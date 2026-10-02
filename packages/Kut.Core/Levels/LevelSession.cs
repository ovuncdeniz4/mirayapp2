using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Objectives;

namespace Kut.Core.Levels
{
  public sealed class LevelSession
  {
    public BoardEngine Engine { get; }
    public LevelDefinition Definition { get; }
    public ObjectiveTracker Objectives { get; }

    public LevelOutcome Outcome { get; private set; } = LevelOutcome.InProgress;

    public LevelSession(BoardEngine engine, LevelDefinition definition)
    {
      Engine = engine;
      Definition = definition;
      Objectives = new ObjectiveTracker(definition.Objectives);
    }

    public static LevelSession FromDefinition(LevelDefinition def)
    {
      return new LevelSession(LevelLoader.CreateEngine(def), def);
    }

    public CommandResult Submit(IGameCommand command)
    {
      if (Outcome != LevelOutcome.InProgress)
      {
        return new CommandResult { Status = CommandStatus.RejectedSessionComplete };
      }

      if (Engine.MovesRemaining <= 0)
      {
        EvaluateOutcome();
        return new CommandResult { Status = CommandStatus.RejectedNoMoves };
      }

      Objectives.BeginTurn();
      var result = Engine.Apply(command);
      Objectives.ApplyEvents(result.Events);
      EvaluateOutcome();
      return result;
    }

    private void EvaluateOutcome()
    {
      if (Objectives.IsComplete)
      {
        Outcome = LevelOutcome.Victory;
        return;
      }

      if (Engine.MovesRemaining <= 0)
      {
        Outcome = LevelOutcome.Defeat;
      }
    }
  }

  public enum LevelOutcome
  {
    InProgress,
    Victory,
    Defeat
  }
}
