using System.Collections;
using System.Collections.Generic;
using Kut.Core.Board;
using Kut.Unity.Design;
using UnityEngine;
using UnityEngine.UI;

namespace Kut.Unity.Presentation
{
  /// <summary>uGUI VFX pool (max 12) — Master Plan §22 without ParticleSystem module.</summary>
  public sealed class BoardVfxOverlay : MonoBehaviour
  {
    private readonly List<Image> _pool = new List<Image>();
    private BoardGridUi? _board;
    private int _active;
    private const int MaxActive = 12;

    public void Bind(BoardGridUi board)
    {
      _board = board;
      EnsurePool();
    }

    public IEnumerator PlayLineClear(bool isRow, int index, float duration)
    {
      if (_board == null || duration <= 0f)
      {
        yield break;
      }

      var img = Rent();
      if (img == null)
      {
        yield break;
      }

      img.color = new Color(0.83f, 0.69f, 0.22f, 0.85f);
      var rt = img.rectTransform;
      rt.sizeDelta = new Vector2(800, 12);
      rt.localRotation = isRow ? Quaternion.identity : Quaternion.Euler(0, 0, 90);
      var anchor = isRow ? new GridPos(0, index) : new GridPos(index, 0);
      PlaceAtCell(rt, anchor);
      yield return FadeScale(img, duration);
    }

    public IEnumerator PlayBombRings(GridPos center, float duration)
    {
      if (_board == null || duration <= 0f)
      {
        yield break;
      }

      for (var ring = 0; ring < 2; ring++)
      {
        var img = Rent();
        if (img == null)
        {
          continue;
        }

        img.color = new Color(1f, 0.42f, 0.17f, 0.7f - ring * 0.2f);
        var rt = img.rectTransform;
        rt.sizeDelta = new Vector2(40 + ring * 30, 40 + ring * 30);
        PlaceAtCell(rt, center);
        StartCoroutine(FadeScale(img, duration * 0.5f));
      }

      yield return new WaitForSeconds(duration);
    }

    public IEnumerator PlayDrumPulse(GridPos at, Color color, float duration)
    {
      var img = Rent();
      if (img == null || duration <= 0f)
      {
        yield break;
      }

      img.color = color;
      var rt = img.rectTransform;
      rt.sizeDelta = new Vector2(100, 100);
      PlaceAtCell(rt, at);
      yield return FadeScale(img, duration);
    }

    public IEnumerator PlayBurst(GridPos at, Color color, float duration)
    {
      var img = Rent();
      if (img == null)
      {
        yield break;
      }

      img.color = color;
      var rt = img.rectTransform;
      rt.sizeDelta = new Vector2(24, 24);
      PlaceAtCell(rt, at);
      yield return FadeScale(img, duration);
    }

    private void PlaceAtCell(RectTransform rt, GridPos pos)
    {
      var cell = _board!.GetCellRect(pos);
      if (cell == null)
      {
        return;
      }

      rt.SetParent(cell, false);
      rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
      rt.anchoredPosition = Vector2.zero;
      rt.localScale = Vector3.one * 0.3f;
    }

    private void EnsurePool()
    {
      if (_pool.Count > 0)
      {
        return;
      }

      for (var i = 0; i < MaxActive; i++)
      {
        var go = new GameObject($"Vfx_{i}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        img.enabled = false;
        _pool.Add(img);
      }
    }

    private Image? Rent()
    {
      if (_active >= MaxActive)
      {
        return null;
      }

      foreach (var img in _pool)
      {
        if (!img.enabled)
        {
          _active++;
          img.enabled = true;
          return img;
        }
      }

      return null;
    }

    private IEnumerator FadeScale(Image img, float duration)
    {
      var rt = img.rectTransform;
      var t = 0f;
      var start = rt.localScale.x;
      while (t < duration)
      {
        t += Time.deltaTime;
        var k = t / duration;
        rt.localScale = Vector3.one * Mathf.Lerp(start, 1.2f, k);
        var c = img.color;
        c.a = Mathf.Lerp(c.a, 0f, k);
        img.color = c;
        yield return null;
      }

      img.enabled = false;
      rt.SetParent(transform, false);
      _active = Mathf.Max(0, _active - 1);
    }
  }
}
