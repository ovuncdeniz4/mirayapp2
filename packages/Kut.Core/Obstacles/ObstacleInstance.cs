namespace Kut.Core.Obstacles
{
  public sealed class ObstacleInstance
  {
    public ObstacleType Type { get; }
    public int Layers { get; set; }

    public ObstacleInstance(ObstacleType type, int layers)
    {
      Type = type;
      Layers = layers;
    }
  }
}
