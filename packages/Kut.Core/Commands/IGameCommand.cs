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
    public bool Success { get; set; }
    public IReadOnlyList<GameEvent> Events { get; set; } = new List<GameEvent>();
  }
}
