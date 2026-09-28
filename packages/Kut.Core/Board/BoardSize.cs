namespace Kut.Core.Board
{
  public readonly struct BoardSize
  {
    public int Width { get; }
    public int Height { get; }

    public BoardSize(int width, int height)
    {
      Width = width;
      Height = height;
    }

    public bool Contains(GridPos p) => p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
  }
}
