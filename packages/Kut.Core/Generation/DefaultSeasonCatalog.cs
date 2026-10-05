using System.Collections.Generic;
using Kut.Core.Combat;
using Kut.Core.Zodiac;

namespace Kut.Core.Generation
{
  public static class DefaultSeasonCatalog
  {
    private static readonly string[] Names =
    {
      "season.1.name", "season.2.name", "season.3.name", "season.4.name", "season.5.name",
      "season.6.name", "season.7.name", "season.8.name", "season.9.name", "season.10.name"
    };

    private static readonly string[] Mechanics =
    {
      "dry_soil", "spirit_vine", "rusted_iron", "poison_mud", "ice_current",
      "soul_lock", "rising_lava", "illusion_tile", "storm_cloud", "dragon_seal"
    };

    public static IReadOnlyList<SeasonDefinition> Create()
    {
      var result = new List<SeasonDefinition>();
      for (var season = 1; season <= 10; season++)
      {
        var mechanic = Mechanics[season - 1];
        result.Add(new SeasonDefinition
        {
          SeasonNumber = season,
          FirstLevel = (season - 1) * 100 + 1,
          LastLevel = season * 100,
          NameKey = Names[season - 1],
          MapVisualSetId = $"season_{season:00}_map",
          GameplayVisualSetId = $"season_{season:00}_gameplay",
          MusicId = $"music_season_{season:00}",
          TilePool = new List<string> { "earth", "water", "fire", "air", "spirit", "metal" },
          MechanicIds = new List<string> { mechanic },
          Archetypes = CreateArchetypes(mechanic),
          Difficulty = new DifficultyCurve
          {
            BaseMoves = 25 - (season - 1) / 3,
            BaseObjectiveTarget = 8 + season,
            ObjectiveGrowthPerBand = 2
          },
          Boss = new BossDefinition
          {
            Id = season == 10 ? "erlik_han" : $"season_{season:00}_guardian",
            MaxHealth = season == 10 ? 20000 : 1200 + season * season * 180,
            DefenseBasisPoints = 400 + season * 100,
            Weakness = season % 3 == 0 ? DamageElement.Water : season % 2 == 0 ? DamageElement.Fire : DamageElement.Earth,
            PhaseThresholdPercent = new List<int> { 70, 35 }
          }
        });
      }
      return result;
    }

    private static List<LevelArchetype> CreateArchetypes(string mechanic) => new List<LevelArchetype>
    {
      new LevelArchetype { Id = "collect_runes", Weight = 4, ObjectiveType = "collect_runes" },
      new LevelArchetype { Id = "progress_obstacles", Weight = 4, ObjectiveType = "complete_obstacles", MechanicId = mechanic },
      new LevelArchetype { Id = "combo_trial", Weight = 2, ObjectiveType = "reach_cascade", MechanicId = mechanic },
      new LevelArchetype { Id = "special_trial", Weight = 2, ObjectiveType = "create_special", MechanicId = mechanic }
    };
  }
}
