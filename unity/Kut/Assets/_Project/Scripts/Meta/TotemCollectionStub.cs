using UnityEngine;

namespace Kut.Unity.Meta
{
  /// <summary>
  /// Placeholder for post-Ch1 totem and collection tabs (wire to save + UI canvas later).
  /// </summary>
  public sealed class TotemCollectionStub : MonoBehaviour
  {
    public void ShowTotem(string animalId)
    {
      Debug.Log($"[Totem] Spirit animal totem awake: {animalId}");
    }

    public void ShowCollection(int completedLevels)
    {
      Debug.Log($"[Collection] Completed levels: {completedLevels}");
    }
  }
}
