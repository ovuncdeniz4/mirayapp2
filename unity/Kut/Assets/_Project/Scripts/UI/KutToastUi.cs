using System.Collections;
using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.UI;

namespace Kut.Unity.UI
{
  /// <summary>
  /// Short on-screen feedback (invalid swap, locked tab).
  /// </summary>
  public sealed class KutToastUi : MonoBehaviour
  {
    private Text? _label;
    private Coroutine? _hideRoutine;

    public static KutToastUi Create(Transform canvasRoot)
    {
      var go = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(KutToastUi));
      go.transform.SetParent(canvasRoot, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = new Vector2(0.1f, 0.04f);
      rt.anchorMax = new Vector2(0.9f, 0.1f);
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      go.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.14f, 0.92f);

      var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
      textGo.transform.SetParent(go.transform, false);
      var trt = textGo.GetComponent<RectTransform>();
      trt.anchorMin = Vector2.zero;
      trt.anchorMax = Vector2.one;
      trt.offsetMin = new Vector2(12, 4);
      trt.offsetMax = new Vector2(-12, -4);
      var t = textGo.GetComponent<Text>();
      t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      t.fontSize = 28;
      t.color = KutDesignTokens.TextPrimary;
      t.alignment = TextAnchor.MiddleCenter;

      var toast = go.GetComponent<KutToastUi>();
      toast._label = t;
      go.SetActive(false);
      return toast;
    }

    public void Show(string message, float seconds = 2f)
    {
      if (_label == null)
      {
        return;
      }

      gameObject.SetActive(true);
      _label.text = message;
      if (_hideRoutine != null)
      {
        StopCoroutine(_hideRoutine);
      }

      _hideRoutine = StartCoroutine(HideAfter(seconds));
    }

    private IEnumerator HideAfter(float seconds)
    {
      yield return new WaitForSeconds(seconds);
      gameObject.SetActive(false);
      _hideRoutine = null;
    }
  }
}
