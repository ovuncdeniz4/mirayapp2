using System;
using System.IO;
using System.Linq;
using Kut.Core.Animals;
using Kut.Core.Commands;
using Kut.Core.Levels;
using Kut.Core.Playtest;
using Kut.Core.Save;

namespace Kut.Playtest
{
  internal static class VerticalSliceApp
  {
    private static readonly string[] LevelIds = { "level_001", "level_002", "level_003" };

    public static int Run(string[] args)
    {
      var savePath = ResolveSavePath(args);
      var contentDir = ResolveContentDir();
      var save = SaveStore.Load(savePath);

      if (!save.OnboardingComplete)
      {
        RunOnboarding(save, savePath);
      }

      while (true)
      {
        Console.WriteLine();
        Console.WriteLine("=== KUT — Ana Ekran ===");
        Console.WriteLine($"Ruh hayvanın: {AnimalAssignment.DisplayNameTr(save.AnimalId)} ({save.AnimalId})");
        Console.WriteLine($"Harita: seviye 1–{save.HighestUnlockedLevel} açık");
        Console.WriteLine("[1] Devam et  [2] Harita  [3] Hayvan  [4] Çıkış");
        Console.Write("> ");
        var choice = Console.ReadLine()?.Trim();
        switch (choice)
        {
          case "1":
          case "2":
            var levelId = LevelIds[Math.Min(save.HighestUnlockedLevel, LevelIds.Length) - 1];
            if (choice == "2")
            {
              ShowMap(save);
              Console.Write("Oyna (1-3): ");
              if (int.TryParse(Console.ReadLine(), out var pick) && pick >= 1 && pick <= save.HighestUnlockedLevel)
              {
                levelId = LevelIds[pick - 1];
              }
            }

            PlayLevel(Path.Combine(contentDir, levelId + ".json"), save, savePath);
            break;
          case "3":
            Console.WriteLine($"Kalıcı ruh eşleşmen: {AnimalAssignment.DisplayNameTr(save.AnimalId)}");
            break;
          case "4":
            return 0;
          default:
            Console.WriteLine("Geçersiz seçim.");
            break;
        }
      }
    }

    private static void RunOnboarding(SaveData save, string savePath)
    {
      Console.WriteLine("=== KUT — Ruh Töreni ===");
      Console.WriteLine("(Kurgusal oyun sistemi — tarihsel şaman geleneği değildir.)");
      Console.Write("Doğum ayın (1-12): ");
      _ = int.TryParse(Console.ReadLine(), out var month);
      Console.Write("Doğum günün (1-31): ");
      _ = int.TryParse(Console.ReadLine(), out var day);
      save.AnimalId = AnimalAssignment.Assign(month, day);
      save.AnimalAssignmentVersion = 1;
      save.OnboardingComplete = true;
      save.HighestUnlockedLevel = 1;
      SaveStore.Save(savePath, save);
      Console.WriteLine($"Ruh hayvanın: {AnimalAssignment.DisplayNameTr(save.AnimalId)}");
    }

    private static void ShowMap(SaveData save)
    {
      for (var i = 0; i < LevelIds.Length; i++)
      {
        var locked = i + 1 > save.HighestUnlockedLevel;
        var done = save.Levels.TryGetValue(LevelIds[i], out var entry) && entry.Completed;
        var mark = done ? "✓" : locked ? "🔒" : "→";
        Console.WriteLine($"  {mark} {i + 1}. {LevelIds[i]}");
      }
    }

    private static void PlayLevel(string path, SaveData save, string savePath)
    {
      var def = LevelLoader.LoadFromFile(path);
      var session = LevelSession.FromDefinition(def);
      Console.WriteLine($"--- {def.Id} | moves: {session.Engine.MovesRemaining} ---");
      PrintObjectives(session);
      Console.WriteLine(BoardAsciiRenderer.Render(session.Engine.State));

      while (session.Outcome == LevelOutcome.InProgress)
      {
        Console.Write("swap x1 y1 x2 y2 | board | quit > ");
        var line = Console.ReadLine();
        if (line == null || line.StartsWith("quit", StringComparison.OrdinalIgnoreCase))
        {
          return;
        }

        if (line.StartsWith("board", StringComparison.OrdinalIgnoreCase))
        {
          Console.WriteLine(BoardAsciiRenderer.Render(session.Engine.State));
          continue;
        }

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 5 && parts[0] == "swap" &&
            int.TryParse(parts[1], out var x1) && int.TryParse(parts[2], out var y1) &&
            int.TryParse(parts[3], out var x2) && int.TryParse(parts[4], out var y2))
        {
          session.Submit(new SwapCommand(new Kut.Core.Board.GridPos(x1, y1), new Kut.Core.Board.GridPos(x2, y2)));
          PrintObjectives(session);
          Console.WriteLine(BoardAsciiRenderer.Render(session.Engine.State));
          Console.WriteLine($"moves: {session.Engine.MovesRemaining}");
        }
      }

      if (session.Outcome == LevelOutcome.Victory)
      {
        Console.WriteLine("*** ZAFER ***");
        save.Levels[def.Id] = new LevelSaveEntry { Completed = true };
        var idx = Array.IndexOf(LevelIds, def.Id);
        if (idx + 1 < LevelIds.Length && save.HighestUnlockedLevel < idx + 2)
        {
          save.HighestUnlockedLevel = idx + 2;
        }

        SaveStore.Save(savePath, save);
      }
      else
      {
        Console.WriteLine("--- yenilgi — tekrar dene ---");
      }
    }

    private static void PrintObjectives(LevelSession session)
    {
      for (var i = 0; i < session.Objectives.Definitions.Count; i++)
      {
        var def = session.Objectives.Definitions[i];
        Console.WriteLine($"  objective {def.Type}: {session.Objectives.GetProgress(i)}/{def.Target}");
      }
    }

    private static string ResolveSavePath(string[] args)
    {
      var arg = args.FirstOrDefault(a => a.StartsWith("--save=", StringComparison.OrdinalIgnoreCase));
      if (arg != null)
      {
        return arg.Substring("--save=".Length);
      }

      return Path.Combine(Environment.CurrentDirectory, "kut_save.json");
    }

    private static string ResolveContentDir()
    {
      var local = Path.Combine(AppContext.BaseDirectory, "content", "levels");
      if (Directory.Exists(local))
      {
        return local;
      }

      return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "content", "levels"));
    }
  }
}
