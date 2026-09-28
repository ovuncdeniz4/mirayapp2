using System;
using System.IO;
using System.Linq;
using Kut.Core.Animals;
using Kut.Core.Commands;
using Kut.Core.Levels;
using Kut.Core.Meta;
using Kut.Core.Playtest;
using Kut.Core.Save;

namespace Kut.Playtest
{
  internal static class VerticalSliceApp
  {
    public static int Run(string[] args)
    {
      var savePath = ResolveSavePath(args);
      var contentRoot = ResolveContentRoot();
      var levelsDir = Path.Combine(contentRoot, "levels");
      var chapters = ChapterCatalog.LoadFromFile(Path.Combine(contentRoot, "chapters.json"));
      var collection = CollectionCatalog.LoadFromFile(Path.Combine(contentRoot, "collection", "catalog.json"));
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
        Console.WriteLine($"Bonus: {AnimalBonus.DescriptionTr(save.AnimalId)}");
        Console.WriteLine($"İlerleme: seviye 1–{save.HighestUnlockedLevel} / {chapters.AllLevelIds.Count} açık");
        if (save.TotemTier > 0)
        {
          Console.WriteLine($"Totem uyanışı: katman {save.TotemTier}");
        }

        if (save.Chapter2Complete)
        {
          Console.WriteLine("Bölüm 2 tamamlandı — Beş Element mührü toplandı.");
        }

        Console.WriteLine("[1] Devam et  [2] Harita  [3] Hayvan  [4] Çıkış");
        if (save.TotemTabUnlocked)
        {
          Console.WriteLine("[5] Totem  [6] Koleksiyon");
        }

        Console.Write("> ");
        var choice = Console.ReadLine()?.Trim();
        switch (choice)
        {
          case "1":
          case "2":
            var levelId = chapters.LevelIdAtGlobalIndex(Math.Min(save.HighestUnlockedLevel, chapters.AllLevelIds.Count))
                          ?? chapters.AllLevelIds[0];
            if (choice == "2")
            {
              ShowMap(save, chapters);
              Console.Write($"Oyna (1-{save.HighestUnlockedLevel}): ");
              if (int.TryParse(Console.ReadLine(), out var pick) && pick >= 1 && pick <= save.HighestUnlockedLevel)
              {
                levelId = chapters.LevelIdAtGlobalIndex(pick) ?? levelId;
              }
            }

            PlayLevel(Path.Combine(levelsDir, levelId + ".json"), save, savePath, chapters, collection);
            break;
          case "3":
            Console.WriteLine($"Kalıcı ruh eşleşmen: {AnimalAssignment.DisplayNameTr(save.AnimalId)}");
            Console.WriteLine($"Savaş bonusu: {AnimalBonus.DescriptionTr(save.AnimalId)}");
            break;
          case "5":
            if (save.TotemTabUnlocked)
            {
              ShowTotemTab(save);
            }
            else
            {
              Console.WriteLine("Totem henüz uyanmadı (Bölüm 1, seviye 10).");
            }

            break;
          case "6":
            if (save.CollectionTabUnlocked)
            {
              ShowCollectionTab(save, collection, chapters);
            }
            else
            {
              Console.WriteLine("Koleksiyon henüz açılmadı (Bölüm 1, seviye 10).");
            }

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
      if (!int.TryParse(Console.ReadLine(), out var month))
      {
        month = 1;
      }

      Console.Write("Doğum günün (1-31): ");
      if (!int.TryParse(Console.ReadLine(), out var day))
      {
        day = 1;
      }

      save.AnimalId = AnimalAssignment.Assign(month, day);
      save.AnimalAssignmentVersion = 1;
      save.OnboardingComplete = true;
      save.HighestUnlockedLevel = 1;
      SaveStore.Save(savePath, save);
      Console.WriteLine($"Ruh hayvanın: {AnimalAssignment.DisplayNameTr(save.AnimalId)}");
    }

    private static void ShowMap(SaveData save, ChapterCatalog chapters)
    {
      var global = 0;
      foreach (var chapter in chapters.Chapters)
      {
        var accessible = MetaProgression.CanAccessChapter(chapter, save, chapters);
        Console.WriteLine($"— {chapter.TitleTr} {(accessible ? "" : "(kilitli)")}");
        if (!accessible)
        {
          global += chapter.Levels.Count;
          continue;
        }

        foreach (var levelId in chapter.Levels)
        {
          global++;
          var locked = global > save.HighestUnlockedLevel;
          var done = save.Levels.TryGetValue(levelId, out var entry) && entry.Completed;
          var mark = done ? "✓" : locked ? "🔒" : "→";
          Console.WriteLine($"  {mark} {global,2}. {levelId}");
        }
      }
    }

    private static void ShowTotemTab(SaveData save)
    {
      Console.WriteLine("=== Totem ===");
      Console.WriteLine($"Hayvan: {AnimalAssignment.DisplayNameTr(save.AnimalId)}");
      Console.WriteLine($"Totem katmanı: {save.TotemTier} (Bölüm 1 → 1, Bölüm 2 → 2)");
      Console.WriteLine("Ruh yolu: Toprak/Su (B1) → Ateş/Rüzgar (B2) → Ruh (meta, ileride).");
    }

    private static void ShowCollectionTab(SaveData save, CollectionCatalog catalog, ChapterCatalog chapters)
    {
      Console.WriteLine("=== Koleksiyon ===");
      var completed = save.Levels.Count(kv => kv.Value.Completed);
      Console.WriteLine($"Tamamlanan seviyeler: {completed}/{chapters.AllLevelIds.Count}");
      Console.WriteLine($"Relikler: {save.UnlockedCollectionIds.Count}/{catalog.Items.Count}");
      foreach (var item in catalog.Items)
      {
        var owned = save.UnlockedCollectionIds.Contains(item.Id);
        Console.WriteLine($"  {(owned ? "★" : "·")} [{item.Category}] {item.TitleTr}");
      }
    }

    private static void PlayLevel(
      string path,
      SaveData save,
      string savePath,
      ChapterCatalog chapters,
      CollectionCatalog collection)
    {
      var def = LevelLoader.LoadFromFile(path);
      var session = LevelSession.FromDefinition(def);
      var bonus = AnimalBonus.BonusMovesAtLevelStart(save.AnimalId);
      if (bonus > 0)
      {
        session.Engine.AddBonusMoves(bonus);
      }

      Console.WriteLine($"--- {def.Id} (Bölüm {def.Chapter}) | moves: {session.Engine.MovesRemaining} ---");
      if (bonus > 0)
      {
        Console.WriteLine($"(Hayvan bonusu +{bonus} hamle)");
      }

      PrintObjectives(session);
      Console.WriteLine(BoardAsciiRenderer.Render(session.Engine.State));
      Console.WriteLine("Legend: E W F A M V C D B # ·");

      while (session.Outcome == LevelOutcome.InProgress)
      {
        Console.Write($"[{session.Engine.MovesRemaining} hamle] swap | activate | board | objectives | quit > ");
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

        if (line.StartsWith("objectives", StringComparison.OrdinalIgnoreCase))
        {
          PrintObjectives(session);
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
          continue;
        }

        if (parts.Length == 3 && parts[0] == "activate" &&
            int.TryParse(parts[1], out var ax) && int.TryParse(parts[2], out var ay))
        {
          session.Submit(new ActivateSpecialCommand(new Kut.Core.Board.GridPos(ax, ay)));
          PrintObjectives(session);
          Console.WriteLine(BoardAsciiRenderer.Render(session.Engine.State));
        }
      }

      if (session.Outcome == LevelOutcome.Victory)
      {
        Console.WriteLine("*** ZAFER ***");
        MetaProgression.ApplyLevelVictory(save, chapters, collection, def.Id);
        if (def.Id == "level_010")
        {
          Console.WriteLine("*** Totem ve Koleksiyon sekmeleri açıldı ***");
        }

        if (def.Id == "level_020")
        {
          Console.WriteLine("*** Bölüm 2 tamamlandı — Totem katmanı 2 ***");
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
        var done = session.Objectives.IsObjectiveComplete(i) ? "✓" : " ";
        Console.WriteLine($"  [{done}] {def.Type}: {session.Objectives.GetProgress(i)}/{def.Target}");
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

    private static string ResolveContentRoot()
    {
      var local = Path.Combine(AppContext.BaseDirectory, "content");
      if (Directory.Exists(Path.Combine(local, "levels")))
      {
        return local;
      }

      return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "content"));
    }
  }
}
