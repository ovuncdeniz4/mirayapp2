using System;

namespace Kut.Core.Board
{
  public readonly struct GridPos : IEquatable<GridPos>, IComparable<GridPos>
  {
    public int X { get; }
    public int Y { get; }

    public GridPos(int x, int y)
    {
      X = x;
      Y = y;
    }

    public bool Equals(GridPos other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is GridPos p && Equals(p);

    public override int GetHashCode() => (X * 397) ^ Y;

    public int CompareTo(GridPos other)
    {
      var y = Y.CompareTo(other.Y);
      return y != 0 ? y : X.CompareTo(other.X);
    }

    public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);

    public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);

    public GridPos Offset(int dx, int dy) => new GridPos(X + dx, Y + dy);
  }
}
