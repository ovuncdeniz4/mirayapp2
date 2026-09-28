namespace Kut.Core.Tiles
{
  public sealed class TileInstance
  {
    public string TileId { get; }
    public Element Element { get; }
    public string MatchGroup { get; }
    public SpecialType Special { get; set; }
    public LineOrientation? ChimeOrientation { get; set; }
    public string? ResonanceMatchGroup { get; set; }

    public TileInstance(string tileId, Element element, string matchGroup)
    {
      TileId = tileId;
      Element = element;
      MatchGroup = matchGroup;
      Special = SpecialType.None;
    }

    public bool IsSwappable => Special != SpecialType.None || true;

    public TileInstance Clone()
    {
      var copy = new TileInstance(TileId, Element, MatchGroup)
      {
        Special = Special,
        ChimeOrientation = ChimeOrientation,
        ResonanceMatchGroup = ResonanceMatchGroup
      };
      return copy;
    }
  }
}
