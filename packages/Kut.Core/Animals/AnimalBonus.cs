namespace Kut.Core.Animals
{
  /// <summary>
  /// Small deterministic per-animal level-start bonus (CLI + future Unity HUD).
  /// </summary>
  public static class AnimalBonus
  {
    public static int BonusMovesAtLevelStart(string animalId) =>
      animalId switch
      {
        "bear" => 1,
        "eagle" => 0,
        "wolf" => 0,
        "deer" => 1,
        "salamander" => 2,
        _ => 0
      };

    public static string DescriptionTr(string animalId) =>
      animalId switch
      {
        "bear" => "+1 hamle (dayanıklılık)",
        "eagle" => "Standart (keskin göz)",
        "wolf" => "Standart (sürü)",
        "deer" => "+1 hamle (çeviklik)",
        "salamander" => "+2 hamle (ateş ruhu)",
        _ => "Standart"
      };
  }
}
