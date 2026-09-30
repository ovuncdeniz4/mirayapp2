using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kut.Unity.UI
{
  internal static class KutUiFactory
  {
    public static Canvas CreateRootCanvas(string name)
    {
      var go = new GameObject(name);
      var canvas = go.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      var scaler = go.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = KutDesignTokens.ReferenceResolution;
      scaler.matchWidthOrHeight = 0.5f;
      go.AddComponent<GraphicRaycaster>();
      Object.DontDestroyOnLoad(go);
      return canvas;
    }

    public static GameObject Panel(Transform parent, string name, float heightFraction = 1f)
    {
      var go = new GameObject(name, typeof(RectTransform), typeof(Image));
      go.transform.SetParent(parent, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = Vector2.zero;
      rt.anchorMax = Vector2.one;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      var img = go.GetComponent<Image>();
      img.color = KutDesignTokens.BackgroundDeep;
      if (heightFraction < 1f)
      {
        rt.anchorMin = new Vector2(0, 1f - heightFraction);
      }

      return go;
    }

    public static Text Title(Transform parent, string text, int size = 52)
    {
      var go = new GameObject("Title", typeof(RectTransform), typeof(Text));
      go.transform.SetParent(parent, false);
      var t = go.GetComponent<Text>();
      t.text = text;
      t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      t.fontSize = size;
      t.color = KutDesignTokens.TextPrimary;
      t.alignment = TextAnchor.UpperCenter;
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = new Vector2(0.05f, 0.82f);
      rt.anchorMax = new Vector2(0.95f, 0.95f);
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      return t;
    }

    public static Button PrimaryButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
      var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
      go.transform.SetParent(parent, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = anchorMin;
      rt.anchorMax = anchorMax;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      go.GetComponent<Image>().color = KutDesignTokens.AccentGold;

      var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
      textGo.transform.SetParent(go.transform, false);
      var txt = textGo.GetComponent<Text>();
      txt.text = label;
      txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      txt.fontSize = 36;
      txt.color = KutDesignTokens.BackgroundDeep;
      txt.alignment = TextAnchor.MiddleCenter;
      var trt = textGo.GetComponent<RectTransform>();
      trt.anchorMin = Vector2.zero;
      trt.anchorMax = Vector2.one;
      trt.offsetMin = Vector2.zero;
      trt.offsetMax = Vector2.zero;

      return go.GetComponent<Button>();
    }

    public static Image SpriteSlot(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
      var go = new GameObject(name, typeof(RectTransform), typeof(Image));
      go.transform.SetParent(parent, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = anchorMin;
      rt.anchorMax = anchorMax;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      var img = go.GetComponent<Image>();
      img.color = Color.white;
      img.preserveAspect = true;
      img.raycastTarget = false;
      return img;
    }

    public static Text Body(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int size = 32)
    {
      var go = new GameObject(name, typeof(RectTransform), typeof(Text));
      go.transform.SetParent(parent, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = anchorMin;
      rt.anchorMax = anchorMax;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      var t = go.GetComponent<Text>();
      t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      t.fontSize = size;
      t.color = KutDesignTokens.TextPrimary;
      t.alignment = TextAnchor.UpperLeft;
      return t;
    }
  }
}
