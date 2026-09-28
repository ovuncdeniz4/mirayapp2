using System.Collections.Generic;
using System.Linq;
using Kut.Core.GameEvents;
using Kut.Core.Tiles;

namespace Kut.Core.Objectives
{
  public sealed class ObjectiveTracker
  {
    private readonly List<ObjectiveDefinition> _definitions;
    private readonly int[] _progress;
    private int _turnCascadeDepth;

    public ObjectiveTracker(IEnumerable<ObjectiveDefinition> definitions)
    {
      _definitions = definitions.ToList();
      _progress = new int[_definitions.Count];
    }

    public IReadOnlyList<ObjectiveDefinition> Definitions => _definitions;

    public int GetProgress(int index) => _progress[index];

    public bool IsComplete => _definitions.Count == 0 || _definitions.Select((_, i) => IsObjectiveComplete(i)).All(x => x);

    public void BeginTurn() => _turnCascadeDepth = 0;

    public void ApplyEvents(IReadOnlyList<GameEvent> events)
    {
      foreach (var evt in events)
      {
        switch (evt)
        {
          case MatchFoundEvent:
            Increment(ObjectiveType.MakeMatches, 1);
            break;
          case CascadeEndedEvent cascade:
            _turnCascadeDepth = cascade.ChainIndex;
            UpdateCascadeDepthObjective();
            break;
          case ElementCollectedEvent collected:
            RegisterElementCollected(collected.Element, collected.Count);
            break;
          case MudCleansedEvent mud when mud.LayersRemaining <= 0:
            IncrementObstacle("mud", 1);
            break;
          case VineBrokenEvent vine when vine.HpRemaining <= 0:
            IncrementObstacle("vine", 1);
            break;
          case SpecialCreatedEvent created:
            IncrementSpecialObjective(ObjectiveType.CreateSpecial, created.Special);
            break;
          case SpecialActivatedEvent activated:
            IncrementSpecialObjective(ObjectiveType.ActivateSpecial, activated.Special);
            break;
        }
      }
    }

    private static SpecialType ParseSpecial(string? name) =>
      name switch
      {
        "wind_chime" => SpecialType.WindChime,
        "shaman_drum" => SpecialType.ShamanDrum,
        _ => SpecialType.None
      };

    private void IncrementSpecialObjective(ObjectiveType type, SpecialType special)
    {
      for (var i = 0; i < _definitions.Count; i++)
      {
        if (_definitions[i].Type != type)
        {
          continue;
        }

        var expected = ParseSpecial(_definitions[i].Special);
        if (expected == SpecialType.None || expected == special)
        {
          _progress[i]++;
        }
      }
    }

    private void IncrementObstacle(string obstacle, int amount)
    {
      for (var i = 0; i < _definitions.Count; i++)
      {
        if (_definitions[i].Type == ObjectiveType.ClearObstacle &&
            (_definitions[i].Obstacle ?? "mud") == obstacle)
        {
          _progress[i] += amount;
        }
      }
    }

    public void RegisterElementCollected(Element element, int count)
    {
      var name = element == Element.Water ? "water" : "earth";
      for (var i = 0; i < _definitions.Count; i++)
      {
        if (_definitions[i].Type == ObjectiveType.CollectElement && _definitions[i].Element == name)
        {
          _progress[i] += count;
        }
      }
    }

    private void UpdateCascadeDepthObjective()
    {
      for (var i = 0; i < _definitions.Count; i++)
      {
        if (_definitions[i].Type == ObjectiveType.CascadeDepthInTurn)
        {
          _progress[i] = System.Math.Max(_progress[i], _turnCascadeDepth);
        }
      }
    }

    private void Increment(ObjectiveType type, int amount)
    {
      for (var i = 0; i < _definitions.Count; i++)
      {
        if (_definitions[i].Type == type)
        {
          _progress[i] += amount;
        }
      }
    }

    public bool IsObjectiveComplete(int index)
    {
      return _progress[index] >= _definitions[index].Target;
    }
  }
}
