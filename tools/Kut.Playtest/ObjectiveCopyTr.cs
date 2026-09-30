using Kut.Core.Objectives;

namespace Kut.Playtest
{
  internal static class ObjectiveCopyTr
  {
    public static string Label(ObjectiveDefinition d) =>
      d.Type switch
      {
        ObjectiveType.MakeMatches => "Eşleşme",
        ObjectiveType.CollectElement => d.Element switch
        {
          "water" => "Su topla",
          "fire" => "Ateş topla",
          "air" => "Rüzgar topla",
          _ => "Toprak topla"
        },
        ObjectiveType.CascadeDepthInTurn => "Zincir",
        ObjectiveType.CreateSpecial => "Özel oluştur",
        ObjectiveType.ActivateSpecial => "Özel kullan",
        ObjectiveType.ClearObstacle => d.Obstacle == "mud" ? "Çamur temizle" : "Sarmaşık kır",
        _ => d.Type.ToString()
      };
  }
}
