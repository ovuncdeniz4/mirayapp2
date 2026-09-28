using Kut.Core.Board;
using Kut.Core.Obstacles;

namespace Kut.Core.Levels
{
  /// <summary>
  /// Stage A prototype layout: 8x8 with a small mud cluster for R1 testing.
  /// </summary>
  public static class PrototypeLevel
  {
    public static void ApplyTo(BoardState board)
    {
      var mudPositions = new[]
      {
        new GridPos(3, 3),
        new GridPos(4, 3),
        new GridPos(3, 4),
        new GridPos(4, 4)
      };

      foreach (var p in mudPositions)
      {
        board.SetCell(p, Cell.FromObstacle(new ObstacleInstance(ObstacleType.Mud, 1)));
      }
    }
  }
}
