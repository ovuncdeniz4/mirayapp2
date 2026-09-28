namespace Kut.Core.Objectives
{
  public enum ObjectiveType
  {
    MakeMatches,
    CascadeDepthInTurn,
    CollectElement,
    ClearObstacle,
    CreateSpecial,
    ActivateSpecial
  }

  public sealed class ObjectiveDefinition
  {
    public ObjectiveType Type { get; set; }
    public int Target { get; set; }
    public string? Element { get; set; }
    public string? Obstacle { get; set; }
    public string? Special { get; set; }
  }
}
