using Kut.Core.Objectives;

namespace Kut.Unity.Design
{
  /// <summary>Turkish copy shared by Unity UI and documented in docs/ux-ui-spec.md.</summary>
  public static class KutCopyTr
  {
    public static string ObjectiveLabel(ObjectiveDefinition d) =>
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
