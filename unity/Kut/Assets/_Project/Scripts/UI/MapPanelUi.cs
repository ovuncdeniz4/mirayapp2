using System;
using Kut.Core.App;
using Kut.Core.Meta;
using Kut.Core.Save;
using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.UI;

namespace Kut.Unity.UI
{
  /// <summary>S-03 Harita — chapter cards, parallax, level nodes.</summary>
  public sealed class MapPanelUi : MonoBehaviour
  {
    private GameObject _root = null!;
    private Image _bg = null!;
    private Image _mist = null!;
    private ScrollRect _scroll = null!;
    private RectTransform _content = null!;

    public void Build(Transform parent, Action onBack)
    {
      _root = KutUiFactory.BorderedPanel(parent, "S03_Map");
      KutUiFactory.Title(_root.transform, "Harita", 48);

      var bgGo = new GameObject("MapBg", typeof(RectTransform), typeof(Image));
      bgGo.transform.SetParent(_root.transform, false);
      var bgRt = bgGo.GetComponent<RectTransform>();
      bgRt.anchorMin = Vector2.zero;
      bgRt.anchorMax = Vector2.one;
      bgRt.offsetMin = Vector2.zero;
      bgRt.offsetMax = Vector2.zero;
      _bg = bgGo.GetComponent<Image>();
      _bg.preserveAspect = false;
      _bg.raycastTarget = false;

      var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
      scrollGo.transform.SetParent(_root.transform, false);
      var srt = scrollGo.GetComponent<RectTransform>();
      srt.anchorMin = new Vector2(0.06f, 0.18f);
      srt.anchorMax = new Vector2(0.94f, 0.78f);
      srt.offsetMin = Vector2.zero;
      srt.offsetMax = Vector2.zero;
      scrollGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.15f);

      var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
      viewport.transform.SetParent(scrollGo.transform, false);
      var vrt = viewport.GetComponent<RectTransform>();
      vrt.anchorMin = Vector2.zero;
      vrt.anchorMax = Vector2.one;
      vrt.offsetMin = Vector2.zero;
      vrt.offsetMax = Vector2.zero;
      viewport.GetComponent<Image>().color = Color.clear;
      viewport.GetComponent<Mask>().showMaskGraphic = false;

      var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
      content.transform.SetParent(viewport.transform, false);
      _content = content.GetComponent<RectTransform>();
      _content.anchorMin = new Vector2(0, 1);
      _content.anchorMax = new Vector2(1, 1);
      _content.pivot = new Vector2(0.5f, 1f);
      var vlg = content.GetComponent<VerticalLayoutGroup>();
      vlg.spacing = 12;
      vlg.padding = new RectOffset(8, 8, 8, 8);
      vlg.childControlHeight = true;
      vlg.childForceExpandHeight = false;
      content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      _scroll = scrollGo.GetComponent<ScrollRect>();
      _scroll.viewport = vrt;
      _scroll.content = _content;
      _scroll.horizontal = false;
      _scroll.vertical = true;
      _scroll.onValueChanged.AddListener(OnScroll);

      var mistGo = new GameObject("Mist", typeof(RectTransform), typeof(Image));
      mistGo.transform.SetParent(_root.transform, false);
      var mrt = mistGo.GetComponent<RectTransform>();
      mrt.anchorMin = new Vector2(0, 0.72f);
      mrt.anchorMax = new Vector2(1, 0.78f);
      mrt.offsetMin = Vector2.zero;
      mrt.offsetMax = Vector2.zero;
      _mist = mistGo.GetComponent<Image>();
      _mist.raycastTarget = false;
      var mistSprite = KutArtCatalog.TryMapMist();
      if (mistSprite != null)
      {
        _mist.sprite = mistSprite;
        _mist.color = Color.white;
      }

      KutUiFactory.PrimaryButton(_root.transform, "Geri", new Vector2(0.2f, 0.06f), new Vector2(0.8f, 0.14f))
        .onClick.AddListener(() => onBack());
      _root.SetActive(false);
    }

    public void Show() => _root.SetActive(true);

    public void Hide() => _root.SetActive(false);

    public void Rebuild(SaveData save, GameContentBundle content, Action<string> onLevel)
    {
      var chapterHint = 1;
      foreach (var ch in content.Chapters.Chapters)
      {
        if (MetaProgression.CanAccessChapter(ch, save, content.Chapters))
        {
          chapterHint = ParseChapterIndex(ch.Id);
          break;
        }
      }

      var bg = KutArtCatalog.TryMapBackground(chapterHint);
      if (bg != null)
      {
        _bg.sprite = bg;
        _bg.color = Color.white;
      }

      foreach (Transform child in _content)
      {
        Destroy(child.gameObject);
      }

      var global = 0;
      foreach (var chapter in content.Chapters.Chapters)
      {
        var accessible = MetaProgression.CanAccessChapter(chapter, save, content.Chapters);
        if (!accessible)
        {
          AddLockedChapter(chapter.TitleTr);
          global += chapter.Levels.Count;
          continue;
        }

        AddChapterCard(chapter.TitleTr);
        foreach (var levelId in chapter.Levels)
        {
          global++;
          var idx = global;
          var locked = idx > save.HighestUnlockedLevel;
          var done = save.Levels.TryGetValue(levelId, out var e) && e.Completed;
          if (locked)
          {
            AddLockedLevel(idx, levelId);
            continue;
          }

          AddLevelRow(done, idx, levelId, () => onLevel(levelId));
        }
      }

      Canvas.ForceUpdateCanvases();
      LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
    }

    private void OnScroll(Vector2 pos)
    {
      if (_bg != null)
      {
        var y = (1f - pos.y) * 80f;
        _bg.rectTransform.anchoredPosition = new Vector2(0, y * 0.3f);
      }

      if (_mist != null)
      {
        _mist.rectTransform.anchoredPosition = new Vector2(0, (1f - pos.y) * 20f);
      }
    }

    private static int ParseChapterIndex(string id)
    {
      if (id.StartsWith("chapter_") && int.TryParse(id.Substring(8), out var n))
      {
        return n;
      }

      return 1;
    }

    private void AddLockedChapter(string title)
    {
      var go = new GameObject("LockedCh", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
      go.transform.SetParent(_content, false);
      go.GetComponent<LayoutElement>().minHeight = 48;
      var t = go.GetComponent<Text>();
      t.font = KutDesignTokens.UiFont;
      t.fontSize = 26;
      t.color = KutDesignTokens.TextMuted;
      t.text = $"🔒 {title}";
    }

    private void AddChapterCard(string title)
    {
      var card = new GameObject("Chapter", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
      card.transform.SetParent(_content, false);
      card.GetComponent<LayoutElement>().minHeight = 56;
      var img = card.GetComponent<Image>();
      var frame = KutArtCatalog.TryPanelFrame();
      if (frame != null)
      {
        img.sprite = frame;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
      }
      else
      {
        img.color = KutDesignTokens.PanelSurface;
      }

      var t = KutUiFactory.Body(card.transform, "Title", new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f), 30);
      t.text = title;
      t.alignment = TextAnchor.MiddleLeft;
    }

    private void AddLockedLevel(int idx, string levelId)
    {
      var go = new GameObject("Locked", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
      go.transform.SetParent(_content, false);
      go.GetComponent<LayoutElement>().minHeight = 56;
      var t = go.GetComponent<Text>();
      t.font = KutDesignTokens.UiFont;
      t.fontSize = 26;
      t.color = KutDesignTokens.TextMuted;
      t.text = $"  🔒 {idx,2}. {levelId}";
    }

    private void AddLevelRow(bool done, int idx, string levelId, Action onClick)
    {
      var go = new GameObject(levelId, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
      go.transform.SetParent(_content, false);
      go.GetComponent<LayoutElement>().minHeight = 72;
      var img = go.GetComponent<Image>();
      img.color = done ? new Color(0.3f, 0.67f, 0.42f, 0.25f) : KutDesignTokens.AccentGold;
      var label = $"{(done ? "✓" : "→")} {idx,2}. {levelId}";
      var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
      textGo.transform.SetParent(go.transform, false);
      var txt = textGo.GetComponent<Text>();
      txt.text = label;
      txt.font = KutDesignTokens.UiFont;
      txt.fontSize = 28;
      txt.color = KutDesignTokens.BackgroundDeep;
      txt.alignment = TextAnchor.MiddleCenter;
      var trt = textGo.GetComponent<RectTransform>();
      trt.anchorMin = Vector2.zero;
      trt.anchorMax = Vector2.one;
      trt.offsetMin = Vector2.zero;
      trt.offsetMax = Vector2.zero;
      go.GetComponent<Button>().onClick.AddListener(() => onClick());
    }
  }
}
