using UnityEngine;

namespace Kut.Unity.UI
{
  [RequireComponent(typeof(RectTransform))]
  public sealed class SafeAreaPanel : MonoBehaviour
  {
    private Rect _lastSafeArea;
    private Vector2Int _lastScreen;

    private void OnEnable() => Apply();

    private void Update()
    {
      if (_lastSafeArea != Screen.safeArea || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
      {
        Apply();
      }
    }

    private void Apply()
    {
      if (Screen.width <= 0 || Screen.height <= 0)
      {
        return;
      }

      _lastSafeArea = Screen.safeArea;
      _lastScreen = new Vector2Int(Screen.width, Screen.height);
      var min = _lastSafeArea.position;
      var max = _lastSafeArea.position + _lastSafeArea.size;
      min.x /= Screen.width;
      min.y /= Screen.height;
      max.x /= Screen.width;
      max.y /= Screen.height;
      var rt = (RectTransform)transform;
      rt.anchorMin = min;
      rt.anchorMax = max;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
    }
  }
}
