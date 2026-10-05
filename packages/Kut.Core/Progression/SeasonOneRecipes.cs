using System.Collections.Generic;

namespace Kut.Core.Progression
{
  public static class SeasonOneRecipes
  {
    public static ObstacleRecipe DrySoil() => new ObstacleRecipe
    {
      Id = "dry_soil",
      Layer = ObstaclePlacementLayer.Ground,
      Stages = new List<ObstacleStage>
      {
        new ObstacleStage { StateId = "dry_soil", VisualId = "obstacle_dry_soil", TransitionVfxId = "vfx_soil_wet", Trigger = SameCell("water") },
        new ObstacleStage { StateId = "wet_soil", VisualId = "obstacle_wet_soil", TransitionVfxId = "vfx_seed_plant", Trigger = SameCell("seed") },
        new ObstacleStage
        {
          StateId = "planted_seed", VisualId = "obstacle_planted_seed", TransitionVfxId = "vfx_giant_flower",
          Trigger = SameCell("sun"),
          CompletionEffect = new GameplayEffect { Type = GameplayEffectType.AreaBlast, Radius = 1, Amount = 1 }
        }
      }
    };

    private static TriggerRule SameCell(string tag) => new TriggerRule
    {
      Scope = TriggerScope.SameCellMatch,
      RequiredRuneTag = tag,
      MinimumMatchSize = 3,
      RequiredHits = 1
    };
  }
}
