using System;
using System.Collections.Generic;
using System.Linq;
using Kut.Core.Board;

namespace Kut.Core.Match
{
  public enum MatchShape { LineThree, LineFour, LineFiveOrMore, LShape, TShape }

  public sealed class MatchGroupResult
  {
    public string MatchGroup { get; set; } = "";
    public MatchShape Shape { get; set; }
    public IReadOnlyList<GridPos> Cells { get; set; } = Array.Empty<GridPos>();
  }

  public static class MatchClassifier
  {
    public static MatchGroupResult Classify(string matchGroup, IEnumerable<GridPos> source)
    {
      var cells = source.Distinct().OrderBy(p => p.Y).ThenBy(p => p.X).ToArray();
      if (cells.Length < 3) throw new ArgumentException("A match requires at least three unique cells.");
      var byRow = cells.GroupBy(p => p.Y).Select(g => g.OrderBy(p => p.X).ToArray()).ToArray();
      var byColumn = cells.GroupBy(p => p.X).Select(g => g.OrderBy(p => p.Y).ToArray()).ToArray();
      var horizontalMax = byRow.Max(ConsecutiveX);
      var verticalMax = byColumn.Max(ConsecutiveY);
      var intersections = cells.Where(p => byRow.Any(r => r.Count(q => q.Y == p.Y) >= 3) && byColumn.Any(c => c.Count(q => q.X == p.X) >= 3)).ToArray();
      MatchShape shape;
      if (intersections.Length > 0)
      {
        var pivot = intersections[0];
        var directions = new[]
        {
          cells.Any(p => p.X < pivot.X && p.Y == pivot.Y), cells.Any(p => p.X > pivot.X && p.Y == pivot.Y),
          cells.Any(p => p.Y < pivot.Y && p.X == pivot.X), cells.Any(p => p.Y > pivot.Y && p.X == pivot.X)
        }.Count(x => x);
        shape = directions >= 3 ? MatchShape.TShape : MatchShape.LShape;
      }
      else
      {
        var longest = Math.Max(horizontalMax, verticalMax);
        shape = longest >= 5 ? MatchShape.LineFiveOrMore : longest == 4 ? MatchShape.LineFour : MatchShape.LineThree;
      }
      return new MatchGroupResult { MatchGroup = matchGroup, Shape = shape, Cells = cells };
    }

    private static int ConsecutiveX(GridPos[] points) => Consecutive(points.Select(p => p.X));
    private static int ConsecutiveY(GridPos[] points) => Consecutive(points.Select(p => p.Y));
    private static int Consecutive(IEnumerable<int> source)
    {
      var best = 0; var run = 0; var previous = int.MinValue;
      foreach (var value in source)
      {
        run = value == previous + 1 ? run + 1 : 1;
        if (run > best) best = run;
        previous = value;
      }
      return best;
    }
  }
}
