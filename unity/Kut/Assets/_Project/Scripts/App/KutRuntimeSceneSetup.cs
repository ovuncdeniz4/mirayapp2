using Kut.Unity.Design;
using UnityEngine;

namespace Kut.Unity.App
{
  /// <summary>
  /// Empty KUT scenes are UI-only; Unity still expects one Camera + AudioListener for Game view and 3D audio.
  /// </summary>
  internal static class KutRuntimeSceneSetup
  {
    public static void EnsureCameraAndAudioListener()
    {
      var listener = Object.FindObjectOfType<AudioListener>();
      var camera = Object.FindObjectOfType<Camera>();

      if (camera == null)
      {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camera = camGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = KutDesignTokens.BackgroundDeep;
        camera.orthographic = true;
        camera.depth = -10;
        Object.DontDestroyOnLoad(camGo);
      }

      if (listener == null)
      {
        listener = camera.gameObject.GetComponent<AudioListener>();
        if (listener == null)
        {
          camera.gameObject.AddComponent<AudioListener>();
        }
      }
    }
  }
}
