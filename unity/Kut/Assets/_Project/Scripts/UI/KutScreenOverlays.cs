using System;
using Kut.Core.Save;
using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.UI;

namespace Kut.Unity.UI
{
  public sealed class SplashOverlayUi : MonoBehaviour
  {
    private GameObject _root = null!;
    private float _timer;

    public void Build(Transform parent)
    {
      _root = KutUiFactory.BorderedPanel(parent, "Splash");
      KutUiFactory.Title(_root.transform, "KUT — Beş Element", 56);
      KutUiFactory.Body(_root.transform, "Loading", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.5f)).text =
        "Yükleniyor…";
      _root.SetActive(true);
    }

    public bool Tick(float dt, float durationSeconds = 1.5f)
    {
      _timer += dt;
      return _timer >= durationSeconds;
    }

    public void Hide() => _root.SetActive(false);
  }

  public sealed class PauseOverlayUi : MonoBehaviour
  {
    private GameObject _root = null!;

    public void Build(Transform parent, Action onResume, Action onMap, Action onRetry)
    {
      _root = KutUiFactory.BorderedPanel(parent, "Pause");
      KutUiFactory.Title(_root.transform, "Duraklat", 48);
      KutUiFactory.PrimaryButton(_root.transform, "Devam", new Vector2(0.15f, 0.42f), new Vector2(0.85f, 0.52f))
        .onClick.AddListener(() => { Hide(); onResume(); });
      KutUiFactory.PrimaryButton(_root.transform, "Harita", new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.4f))
        .onClick.AddListener(() => { Hide(); onMap(); });
      KutUiFactory.PrimaryButton(_root.transform, "Tekrar dene", new Vector2(0.15f, 0.18f), new Vector2(0.85f, 0.28f))
        .onClick.AddListener(() => { Hide(); onRetry(); });
      _root.SetActive(false);
    }

    public void Show() => _root.SetActive(true);
    public void Hide() => _root.SetActive(false);
  }

  public sealed class ConfirmDialogUi : MonoBehaviour
  {
    private GameObject _root = null!;

    public void Build(Transform parent)
    {
      _root = KutUiFactory.BorderedPanel(parent, "Confirm");
      _root.SetActive(false);
    }

    public void Show(string message, Action onConfirm, Action onCancel)
    {
      foreach (Transform c in _root.transform)
      {
        if (c.name != "Frame")
        {
          Destroy(c.gameObject);
        }
      }

      KutUiFactory.Body(_root.transform, "Msg", new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.75f), 30).text = message;
      KutUiFactory.PrimaryButton(_root.transform, "Evet", new Vector2(0.12f, 0.2f), new Vector2(0.45f, 0.32f))
        .onClick.AddListener(() => { _root.SetActive(false); onConfirm(); });
      KutUiFactory.PrimaryButton(_root.transform, "İptal", new Vector2(0.55f, 0.2f), new Vector2(0.88f, 0.32f))
        .onClick.AddListener(() => { _root.SetActive(false); onCancel(); });
      _root.SetActive(true);
    }
  }

  public sealed class SettingsPanelUi : MonoBehaviour
  {
    private GameObject _root = null!;
    private SaveData _save = null!;
    private Action _onChanged = null!;

    public void Build(Transform parent, SaveData save, Action onChanged, Action onClose, Action onReset)
    {
      _save = save;
      _onChanged = onChanged;
      _root = KutUiFactory.BorderedPanel(parent, "Settings");
      KutUiFactory.Title(_root.transform, "Ayarlar", 48);
      AddSlider("Müzik", 0.72f, v => { _save.MusicVolume = v; _onChanged(); }, _save.MusicVolume);
      AddSlider("Ses efektleri", 0.58f, v => { _save.SfxVolume = v; _onChanged(); }, _save.SfxVolume);
      AddToggle("Titreşim", 0.46f, _save.HapticsEnabled, v => { _save.HapticsEnabled = v; _onChanged(); });
      AddToggle("Azaltılmış hareket", 0.36f, _save.ReducedMotion, v => { _save.ReducedMotion = v; _onChanged(); });
      KutUiFactory.PrimaryButton(_root.transform, "Kaydı sıfırla", new Vector2(0.12f, 0.14f), new Vector2(0.88f, 0.22f))
        .onClick.AddListener(onReset);
      KutUiFactory.PrimaryButton(_root.transform, "Geri", new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.12f))
        .onClick.AddListener(() => { _root.SetActive(false); onClose(); });
      _root.SetActive(false);
    }

    public void Show() => _root.SetActive(true);

    private void AddSlider(string label, float y, Action<float> set, float value)
    {
      KutUiFactory.Body(_root.transform, label, new Vector2(0.08f, y), new Vector2(0.92f, y + 0.06f)).text = label;
      var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
      go.transform.SetParent(_root.transform, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = new Vector2(0.1f, y - 0.08f);
      rt.anchorMax = new Vector2(0.9f, y - 0.02f);
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      var s = go.GetComponent<Slider>();
      s.minValue = 0f;
      s.maxValue = 1f;
      s.value = value;
      s.onValueChanged.AddListener(v => set(v));
    }

    private void AddToggle(string label, float y, bool value, Action<bool> set)
    {
      var go = new GameObject(label, typeof(RectTransform), typeof(Toggle), typeof(Text));
      go.transform.SetParent(_root.transform, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = new Vector2(0.08f, y - 0.04f);
      rt.anchorMax = new Vector2(0.92f, y + 0.04f);
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      var t = go.GetComponent<Toggle>();
      t.isOn = value;
      t.onValueChanged.AddListener(set);
      var txt = go.GetComponent<Text>();
      txt.text = "  " + label;
      txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      txt.fontSize = 28;
      txt.color = KutDesignTokens.TextPrimary;
    }
  }

  public sealed class TutorialOverlayUi : MonoBehaviour
  {
    private GameObject _root = null!;
    private Text _body = null!;

    public void Build(Transform parent)
    {
      _root = KutUiFactory.BorderedPanel(parent, "Tutorial");
      KutUiFactory.Title(_root.transform, "İpucu", 40);
      _body = KutUiFactory.Body(_root.transform, "Body", new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.7f), 30);
      KutUiFactory.PrimaryButton(_root.transform, "Atla", new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.22f));
      _root.SetActive(false);
    }

    public void Show(string title, string body, bool canSkip, Action onDismiss)
    {
      var titleT = _root.transform.Find("Title")?.GetComponent<Text>();
      if (titleT != null)
      {
        titleT.text = title;
      }
      _body.text = body;
      var skip = _root.GetComponentInChildren<Button>();
      skip.onClick.RemoveAllListeners();
      skip.onClick.AddListener(() => { _root.SetActive(false); onDismiss(); });
      skip.gameObject.SetActive(canSkip);
      _root.SetActive(true);
    }
  }

  public sealed class CeremonyOverlayUi : MonoBehaviour
  {
    private GameObject _root = null!;
    private float _timer;

    public void Build(Transform parent)
    {
      _root = KutUiFactory.BorderedPanel(parent, "Ceremony");
      KutUiFactory.Title(_root.transform, "Ruh töreni", 44);
      KutUiFactory.Body(_root.transform, "Glyphs", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f)).text =
        "Kut işaretleri dönüyor…";
      _root.SetActive(false);
    }

    public void Play(Action onDone)
    {
      _timer = 0f;
      _root.SetActive(true);
      StartCoroutine(Run(onDone));
    }

    private System.Collections.IEnumerator Run(Action onDone)
    {
      while (_timer < 2.5f)
      {
        _timer += Time.deltaTime;
        _root.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(_timer * 3f) * 8f);
        yield return null;
      }

      _root.SetActive(false);
      onDone();
    }
  }
}
