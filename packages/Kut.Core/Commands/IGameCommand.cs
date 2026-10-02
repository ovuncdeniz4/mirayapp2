using System.Collections.Generic;
using Kut.Core.GameEvents;

namespace Kut.Core.Commands
{
  public interface IGameCommand
  {
    string Name { get; }
  }

  public sealed class CommandResult
  {
    public CommandStatus Status { get; set; }
    public bool Success => Status == CommandStatus.Applied;
    public IReadOnlyList<GameEvent> Events { get; set; } = new List<GameEvent>();
  }

  public enum CommandStatus
  {
    Applied,
    Reverted,
    RejectedInvalidPosition,
    RejectedInvalidTarget,
    RejectedSessionComplete,
    RejectedNoMoves
  }
}
