namespace Kut.Core.Levels
{
  public sealed class LevelRules
  {
    public int Moves { get; set; } = 20;
    public bool EnableWindChime { get; set; } = true;
    public bool EnableShamanDrum { get; set; } = false;
    public bool EnableFireBomb { get; set; } = false;
    public bool EnableSpecialCreation { get; set; } = true;
    public int Seed { get; set; } = 1;
  }
}
