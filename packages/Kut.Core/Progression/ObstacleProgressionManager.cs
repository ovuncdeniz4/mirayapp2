using System;
using System.Collections.Generic;
using Kut.Core.Board;

namespace Kut.Core.Progression
{
  public enum ObstaclePlacementLayer { Ground, Overlay, Blocker, Occupant }
  public enum TriggerScope { SameCellMatch, AdjacentMatch, HitBySpecial, HitByUltimate, TurnElapsed, BossPhaseChanged }
  public enum GameplayEffectType { None, AreaBlast, LowerHazard, DirectBossDamage, Harvest }

  public sealed class TriggerRule
  {
    public TriggerScope Scope { get; set; }
    public string RequiredRuneTag { get; set; } = "";
    public int MinimumMatchSize { get; set; } = 3;
    public int RequiredHits { get; set; } = 1;
  }

  public sealed class GameplayEffect
  {
    public GameplayEffectType Type { get; set; }
    public int Radius { get; set; }
    public int Amount { get; set; }
  }

  public sealed class ObstacleStage
  {
    public string StateId { get; set; } = "";
    public TriggerRule Trigger { get; set; } = new TriggerRule();
    public string VisualId { get; set; } = "";
    public string TransitionVfxId { get; set; } = "";
    public GameplayEffect? CompletionEffect { get; set; }
  }

  public sealed class ObstacleRecipe
  {
    public string Id { get; set; } = "";
    public ObstaclePlacementLayer Layer { get; set; }
    public List<ObstacleStage> Stages { get; set; } = new List<ObstacleStage>();
  }

  public sealed class ProgressiveObstacle
  {
    public ProgressiveObstacle(string recipeId, GridPos position)
    {
      RecipeId = recipeId;
      Position = position;
    }
    public string RecipeId { get; }
    public GridPos Position { get; }
    public int StageIndex { get; internal set; }
    public int HitsInStage { get; internal set; }
    public bool Completed { get; internal set; }
  }

  public readonly struct ObstacleTrigger
  {
    public ObstacleTrigger(GridPos source, TriggerScope scope, string runeTag, int matchSize)
    { Source = source; Scope = scope; RuneTag = runeTag ?? ""; MatchSize = matchSize; }
    public GridPos Source { get; }
    public TriggerScope Scope { get; }
    public string RuneTag { get; }
    public int MatchSize { get; }
  }

  public sealed class ObstacleTransition
  {
    public GridPos Position { get; set; }
    public string FromStateId { get; set; } = "";
    public string ToStateId { get; set; } = "";
    public GameplayEffect? Effect { get; set; }
    public bool Completed { get; set; }
  }

  public sealed class ObstacleProgressionManager
  {
    private readonly Dictionary<string, ObstacleRecipe> _recipes;

    public ObstacleProgressionManager(IEnumerable<ObstacleRecipe> recipes)
    {
      _recipes = new Dictionary<string, ObstacleRecipe>(StringComparer.Ordinal);
      foreach (var recipe in recipes)
      {
        if (string.IsNullOrWhiteSpace(recipe.Id) || recipe.Stages.Count == 0)
          throw new ArgumentException("Obstacle recipes require an id and at least one stage.");
        _recipes.Add(recipe.Id, recipe);
      }
    }

    public ObstacleTransition? Apply(ProgressiveObstacle obstacle, ObstacleTrigger trigger)
    {
      if (obstacle.Completed || !_recipes.TryGetValue(obstacle.RecipeId, out var recipe)) return null;
      var stage = recipe.Stages[obstacle.StageIndex];
      if (!Matches(stage.Trigger, obstacle.Position, trigger)) return null;
      obstacle.HitsInStage++;
      if (obstacle.HitsInStage < Math.Max(1, stage.Trigger.RequiredHits)) return null;

      var from = stage.StateId;
      obstacle.HitsInStage = 0;
      obstacle.StageIndex++;
      obstacle.Completed = obstacle.StageIndex >= recipe.Stages.Count;
      var to = obstacle.Completed ? "complete" : recipe.Stages[obstacle.StageIndex].StateId;
      return new ObstacleTransition
      {
        Position = obstacle.Position,
        FromStateId = from,
        ToStateId = to,
        Effect = stage.CompletionEffect,
        Completed = obstacle.Completed
      };
    }

    private static bool Matches(TriggerRule rule, GridPos target, ObstacleTrigger trigger)
    {
      if (rule.Scope != trigger.Scope || trigger.MatchSize < rule.MinimumMatchSize) return false;
      if (!string.IsNullOrEmpty(rule.RequiredRuneTag) &&
          !string.Equals(rule.RequiredRuneTag, trigger.RuneTag, StringComparison.OrdinalIgnoreCase)) return false;
      var distance = Math.Abs(target.X - trigger.Source.X) + Math.Abs(target.Y - trigger.Source.Y);
      return rule.Scope == TriggerScope.AdjacentMatch ? distance == 1 :
        rule.Scope == TriggerScope.SameCellMatch ? distance == 0 : true;
    }
  }
}
