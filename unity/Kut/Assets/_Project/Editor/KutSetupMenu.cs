#if UNITY_EDITOR
using System.IO;
using Kut.Unity.App;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kut.Unity.EditorTools
{
  /// <summary>
  /// One-click Main scene for macOS 2022.3 setup (Menu: KUT → Create Main Scene).
  /// </summary>
  public static class KutSetupMenu
  {
    private const string ScenePath = "Assets/_Project/Scenes/Main.unity";

    [MenuItem("KUT/Create Main Scene And Open")]
    public static void CreateMainSceneAndOpen()
    {
      Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
      var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

      var bootstrap = new GameObject("KUT");
      bootstrap.AddComponent<KutAppBootstrap>();

      EditorSceneManager.SaveScene(scene, ScenePath);
      EditorSceneManager.OpenScene(ScenePath);
      EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

      Debug.Log("KUT: Main scene ready. Assign nothing else — press Play. Run ./tools/sync-core-to-unity.sh if Kut.Core.dll is missing.");
    }

    [MenuItem("KUT/Open Setup Doc")]
    public static void OpenSetupDoc()
    {
      var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/unity-setup.md"));
      if (File.Exists(path))
      {
        EditorUtility.RevealInFinder(path);
      }
    }
  }
}
#endif
