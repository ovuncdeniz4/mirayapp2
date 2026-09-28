namespace Kut.Core.Random
{
  /// <summary>
  /// Seeded PRNG for reproducible board generation and refills.
  /// </summary>
  public sealed class DeterministicRandom
  {
    private uint _state;

    public DeterministicRandom(int seed)
    {
      _state = (uint)seed;
      if (_state == 0)
      {
        _state = 1;
      }
    }

    public int NextInt(int maxExclusive)
    {
      if (maxExclusive <= 0)
      {
        return 0;
      }

      return (int)(NextUInt() % (uint)maxExclusive);
    }

    public uint NextUInt()
    {
      _state += 0x6D2B79F5;
      var t = _state;
      t = (uint)(((t ^ (t >> 15)) * (t | 1u)) & 0xFFFFFFFF);
      t ^= t + (uint)(((t ^ (t >> 7)) * (t | 61u)) & 0xFFFFFFFF);
      return t ^ (t >> 14);
    }
  }
}
