namespace Kut.Core.Objectives
{
  public enum ObjectiveType
  {
    MakeMatches,
    CascadeDepthInTurn,
    CollectElement
  }

  public sealed class ObjectiveDefinition
  {
    public ObjectiveType Type { get; set; }
    public int Target { get; set; }
    public string? Element { get; set; }
  }
}
