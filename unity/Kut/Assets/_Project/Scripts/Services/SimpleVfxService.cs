using UnityEngine;

namespace Kut.Unity.Services
{
  /// <summary>Lightweight burst VFX; capped for mobile.</summary>
  public sealed class SimpleVfxService : MonoBehaviour
  {
    private int _active;
    private const int MaxActive = 12;

    public void Burst(Vector3 worldPos, Color color)
    {
      if (_active >= MaxActive)
      {
        return;
      }

      var go = new GameObject("VfxBurst", typeof(ParticleSystem));
      go.transform.position = worldPos;
      var ps = go.GetComponent<ParticleSystem>();
      var main = ps.main;
      main.startColor = color;
      main.startLifetime = 0.35f;
      main.startSpeed = 2f;
      main.maxParticles = 8;
      ps.Emit(6);
      _active++;
      Destroy(go, 0.5f);
      Invoke(nameof(Dec), 0.5f);
    }

    private void Dec() => _active = Mathf.Max(0, _active - 1);
  }
}
