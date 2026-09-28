using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;

namespace Kut.Unity.App
{
  /// <summary>
  /// Application layer: sole mutator of Kut.Core BoardEngine from gameplay commands.
  /// </summary>
  public sealed class GameplaySession
  {
    public BoardEngine Engine { get; }

    public GameplaySession(BoardEngine engine)
    {
      Engine = engine;
    }

    public static GameplaySession CreatePrototype()
    {
      return new GameplaySession(BoardEngine.CreatePrototype());
    }

    public CommandResult Submit(IGameCommand command)
    {
      return Engine.Apply(command);
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
