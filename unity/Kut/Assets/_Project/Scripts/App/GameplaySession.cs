using System.Collections.Generic;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Levels;

namespace Kut.Unity.App
{
  /// <summary>
  /// Application layer: sole mutator of Kut.Core from gameplay commands.
  /// </summary>
  public sealed class GameplaySession
  {
    public LevelSession LevelSession { get; }

    public GameplaySession(LevelSession levelSession)
    {
      LevelSession = levelSession;
    }

    public static GameplaySession CreatePrototype()
    {
      return FromLevelJson(null);
    }

    public static GameplaySession FromLevelJson(string? json)
    {
      LevelDefinition def;
      if (string.IsNullOrWhiteSpace(json))
      {
        def = new LevelDefinition
        {
          Id = "level_prototype",
          Chapter = 1,
          Moves = 20,
          Seed = 42,
          EnableWindChime = true,
          EnableSpecialCreation = true
        };
        def.Objectives.Add(new Kut.Core.Objectives.ObjectiveDefinition
        {
          Type = Kut.Core.Objectives.ObjectiveType.MakeMatches,
          Target = 99
        });
      }
      else
      {
        def = LevelLoader.Parse(json);
      }

      return new GameplaySession(LevelSession.FromDefinition(def));
    }

    public CommandResult Submit(IGameCommand command)
    {
      return LevelSession.Submit(command);
    }

    public IReadOnlyList<GameEvent> LastEvents { get; private set; } = new List<GameEvent>();

    public CommandResult SubmitAndRemember(IGameCommand command)
    {
      var result = Submit(command);
      LastEvents = result.Events;
      return result;
    }
  }
}
