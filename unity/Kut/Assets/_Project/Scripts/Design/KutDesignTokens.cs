using UnityEngine;

namespace Kut.Unity.Design
{
  /// <summary>
  /// Visual tokens from docs/visual-design-system.md (placeholder art phase).
  /// </summary>
  public static class KutDesignTokens
  {
    public static readonly Color BackgroundDeep = Hex("#1A1520");
    public static readonly Color PanelSurface = Hex("#2A2235");
    public static readonly Color AccentGold = Hex("#C9A227");
    public static readonly Color TextPrimary = Hex("#F5F0E8");
    public static readonly Color TextMuted = Hex("#A89FB0");
    public static readonly Color Danger = Hex("#E05252");

    public static readonly Vector2 ReferenceResolution = new Vector2(1080, 1920);

    public static Color AnimalAccent(string animalId) =>
      animalId switch
      {
        "wolf" => Hex("#8B9DAF"),
        "eagle" => Hex("#B89B4C"),
        "bear" => Hex("#6B5344"),
        "deer" => Hex("#C4A882"),
        "salamander" => Hex("#E85D3B"),
        _ => AccentGold
      };

    private static Color Hex(string hex)
    {
      ColorUtility.TryParseHtmlString(hex, out var c);
      return c;
    }
  }
}
