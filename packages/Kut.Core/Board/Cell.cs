using Kut.Core.Obstacles;
using Kut.Core.Tiles;

namespace Kut.Core.Board
{
  public sealed class Cell
  {
    public CellKind Kind { get; set; }
    public TileInstance? Tile { get; set; }
    public ObstacleInstance? Obstacle { get; set; }

    public static Cell Empty() => new Cell { Kind = CellKind.Empty };

    public static Cell FromTile(TileInstance tile) =>
      new Cell { Kind = CellKind.Tile, Tile = tile };

    public static Cell FromObstacle(ObstacleInstance obstacle) =>
      new Cell { Kind = CellKind.Obstacle, Obstacle = obstacle };

    public static Cell Blocker() => new Cell { Kind = CellKind.Blocker };
  }
}
