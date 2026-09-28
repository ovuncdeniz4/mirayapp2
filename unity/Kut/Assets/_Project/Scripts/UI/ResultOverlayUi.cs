using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.UI;

namespace Kut.Unity.UI
{
  /// <summary>
  /// S-04 victory / defeat overlay per ux-ui-spec.
  /// </summary>
  public sealed class ResultOverlayUi : MonoBehaviour
  {
    private GameObject _root = null!;
    private Text _title = null!;
    private Button _primary = null!;
    private Button _secondary = null!;

    public void Build(Transform parent)
    {
      _root = KutUiFactory.Panel(parent, "ResultOverlay");
      _root.SetActive(false);
      _title = KutUiFactory.Title(_root.transform, "", 56);
      _primary = KutUiFactory.PrimaryButton(_root.transform, "Devam",
        new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.3f));
      _secondary = KutUiFactory.PrimaryButton(_root.transform, "Harita",
        new Vector2(0.15f, 0.1f), new Vector2(0.85f, 0.18f));
      _secondary.GetComponent<Image>().color = KutDesignTokens.PanelSurface;
    }

    public void ShowVictory(System.Action onContinue)
    {
      _title.text = "ZAFER";
      _title.color = KutDesignTokens.AccentGold;
      _primary.onClick.RemoveAllListeners();
      _secondary.onClick.RemoveAllListeners();
      _primary.onClick.AddListener(() =>
      {
        Hide();
        onContinue();
      });
      _secondary.gameObject.SetActive(false);
      _root.SetActive(true);
    }

    public void ShowDefeat(System.Action onRetry, System.Action onMap)
    {
      _title.text = "Hamle kalmadı";
      _title.color = KutDesignTokens.Danger;
      _primary.onClick.RemoveAllListeners();
      _secondary.onClick.RemoveAllListeners();
      _primary.onClick.AddListener(() =>
      {
        Hide();
        onRetry();
      });
      _secondary.onClick.AddListener(() =>
      {
        Hide();
        onMap();
      });
      _secondary.gameObject.SetActive(true);
      _root.SetActive(true);
    }

    public void Hide() => _root.SetActive(false);
  }
}
