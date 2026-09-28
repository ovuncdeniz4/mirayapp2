using System.IO;
using UnityEngine;

namespace Kut.Unity.App
{
  public static class UnitySavePaths
  {
    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, "kut_save.json");
  }
}
