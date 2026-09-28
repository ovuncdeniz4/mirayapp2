using System;
using System.Collections.Generic;

namespace Kut.Core.Animals
{
  public static class AnimalAssignment
  {
    private static readonly string[] DefaultRotation = { "wolf", "eagle", "bear", "deer", "salamander" };

    public static string Assign(int month, int day, int seasonOffset = 0, int assignmentVersion = 1, IReadOnlyList<string>? rotation = null)
    {
      if (month < 1 || month > 12)
      {
        throw new ArgumentOutOfRangeException(nameof(month));
      }

      var daysInMonth = DateTime.DaysInMonth(2024, month);
      var validDay = Math.Clamp(day, 1, daysInMonth);
      var dayIndex = DayOfYearIndex(month, validDay);
      var table = rotation ?? DefaultRotation;
      var slot = (dayIndex + seasonOffset) % table.Count;
      _ = assignmentVersion;
      return table[slot];
    }

    private static int DayOfYearIndex(int month, int day)
    {
      var index = 0;
      for (var m = 1; m < month; m++)
      {
        index += DateTime.DaysInMonth(2024, m);
      }

      return index + day;
    }

    public static string DisplayNameTr(string animalId) =>
      animalId switch
      {
        "wolf" => "Kurt",
        "eagle" => "Kartal",
        "bear" => "Ayı",
        "deer" => "Geyik",
        "salamander" => "Semender",
        _ => animalId
      };
  }
}
