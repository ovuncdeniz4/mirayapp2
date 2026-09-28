using System.Collections.Generic;
using Kut.Core.Objectives;

namespace Kut.Core.Levels
{
  public sealed class LevelDefinition
  {
    public string Id { get; set; } = "level_unknown";
    public int Chapter { get; set; }
    public int BoardWidth { get; set; } = 8;
    public int BoardHeight { get; set; } = 8;
    public int Moves { get; set; } = 20;
    public int Seed { get; set; } = 1;
    public List<string> SpawnTable { get; set; } = new List<string>();
    public bool EnableWindChime { get; set; } = true;
    public bool EnableShamanDrum { get; set; } = false;
    public bool EnableSpecialCreation { get; set; } = true;
    public List<MudCellDefinition> MudCells { get; set; } = new List<MudCellDefinition>();
    public List<MudCellDefinition> VineCells { get; set; } = new List<MudCellDefinition>();
    public List<GridCellDefinition> StoneCells { get; set; } = new List<GridCellDefinition>();
    public List<ObjectiveDefinition> Objectives { get; set; } = new List<ObjectiveDefinition>();
  }

  public sealed class MudCellDefinition
  {
    public int X { get; set; }
    public int Y { get; set; }
    public int Layers { get; set; } = 1;
  }

  public sealed class GridCellDefinition
  {
    public int X { get; set; }
    public int Y { get; set; }
  }
}
