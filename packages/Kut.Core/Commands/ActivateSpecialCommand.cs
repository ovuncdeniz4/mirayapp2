using Kut.Core.Board;

namespace Kut.Core.Commands
{
  public sealed class ActivateSpecialCommand : IGameCommand
  {
    public string Name => "activate_special";

    public GridPos At { get; }

    public ActivateSpecialCommand(GridPos at)
    {
      At = at;
    }
  }
}
