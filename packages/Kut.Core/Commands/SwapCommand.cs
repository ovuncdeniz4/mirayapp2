using Kut.Core.Board;

namespace Kut.Core.Commands
{
  public sealed class SwapCommand : IGameCommand
  {
    public string Name => "swap";

    public GridPos From { get; }
    public GridPos To { get; }

    public SwapCommand(GridPos from, GridPos to)
    {
      From = from;
      To = to;
    }
  }
}
