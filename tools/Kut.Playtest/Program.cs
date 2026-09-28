using System;
using System.IO;
using System.Linq;
using Kut.Core.Board;
using Kut.Core.Commands;
using Kut.Core.GameEvents;
using Kut.Core.Levels;
using Kut.Core.Playtest;

namespace Kut.Playtest
{
  internal static class Program
  {
    private static int Main(string[] args)
    {
      var levelPath = ResolveLevelPath(args);
      var engine = LevelLoader.CreateEngineFromFile(levelPath);
      Console.WriteLine($"KUT playtest — level: {Path.GetFileName(levelPath)} | moves: {engine.MovesRemaining}");
      Console.WriteLine(BoardAsciiRenderer.Render(engine.State));

      if (args.Any(a => a == "--demo"))
      {
        return RunDemo(engine);
      }

      return RunInteractive(engine);
    }

    private static string ResolveLevelPath(string[] args)
    {
      var fileArg = args.FirstOrDefault(a => a.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
      if (fileArg != null && File.Exists(fileArg))
      {
        return fileArg;
      }

      var local = Path.Combine(AppContext.BaseDirectory, "content", "levels", "level_prototype.json");
      if (File.Exists(local))
      {
        return local;
      }

      var repo = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "content", "levels", "level_prototype.json"));
      if (File.Exists(repo))
      {
        return repo;
      }

      throw new FileNotFoundException("Could not find level_prototype.json");
    }

    private static int RunDemo(BoardEngine engine)
    {
      // Scripted swaps on seeded board — may revert if no match; demonstrates event log.
      var commands = new (int x1, int y1, int x2, int y2)[]
      {
        (0, 0, 1, 0),
        (2, 1, 2, 0),
        (5, 3, 6, 3)
      };

      foreach (var (x1, y1, x2, y2) in commands)
      {
        Console.WriteLine($"> swap {x1} {y1} {x2} {y2}");
        var result = engine.Apply(new SwapCommand(new GridPos(x1, y1), new GridPos(x2, y2)));
        PrintEvents(result);
        Console.WriteLine(BoardAsciiRenderer.Render(engine.State));
        Console.WriteLine($"moves left: {engine.MovesRemaining}");
      }

      return 0;
    }

    private static int RunInteractive(BoardEngine engine)
    {
      Console.WriteLine("Commands: swap x1 y1 x2 y2 | activate x y | board | quit");
      while (true)
      {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (line == null || line.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
          return 0;
        }

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
          continue;
        }

        if (parts[0].Equals("board", StringComparison.OrdinalIgnoreCase))
        {
          Console.WriteLine(BoardAsciiRenderer.Render(engine.State));
          continue;
        }

        if (parts[0].Equals("swap", StringComparison.OrdinalIgnoreCase) && parts.Length == 5 &&
            int.TryParse(parts[1], out var x1) && int.TryParse(parts[2], out var y1) &&
            int.TryParse(parts[3], out var x2) && int.TryParse(parts[4], out var y2))
        {
          var result = engine.Apply(new SwapCommand(new GridPos(x1, y1), new GridPos(x2, y2)));
          PrintEvents(result);
          Console.WriteLine(BoardAsciiRenderer.Render(engine.State));
          Console.WriteLine($"moves left: {engine.MovesRemaining}");
          continue;
        }

        if (parts[0].Equals("activate", StringComparison.OrdinalIgnoreCase) && parts.Length == 3 &&
            int.TryParse(parts[1], out var ax) && int.TryParse(parts[2], out var ay))
        {
          var result = engine.Apply(new ActivateSpecialCommand(new GridPos(ax, ay)));
          PrintEvents(result);
          Console.WriteLine(BoardAsciiRenderer.Render(engine.State));
          continue;
        }

        Console.WriteLine("Unknown command.");
      }
    }

    private static void PrintEvents(CommandResult result)
    {
      foreach (var evt in result.Events)
      {
        switch (evt)
        {
          case SwapRevertedEvent:
            Console.WriteLine("  event: swap_reverted");
            break;
          case MoveConsumedEvent m:
            Console.WriteLine($"  event: move_consumed (remaining={m.RemainingMoves})");
            break;
          case MatchFoundEvent m:
            Console.WriteLine($"  event: match_found ({m.Cells.Count} cells)");
            break;
          case SpecialCreatedEvent s:
            Console.WriteLine($"  event: special_created {s.Special} @ ({s.At.X},{s.At.Y})");
            break;
          case MudCleansedEvent mud:
            Console.WriteLine($"  event: mud_cleansed @ ({mud.At.X},{mud.At.Y}) layers={mud.LayersRemaining}");
            break;
          case CascadeEndedEvent c:
            Console.WriteLine($"  event: cascade_ended chain={c.ChainIndex}");
            break;
          default:
            Console.WriteLine($"  event: {evt.EventType}");
            break;
        }
      }
    }
  }
}
