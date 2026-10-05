using System;
using Kut.Core.Economy;
using Kut.Core.Save;
using Kut.Core.Zodiac;

namespace Kut.Core.App
{
  public interface IPlayerProfileRepository
  {
    SaveData Load();
    void Save(SaveData profile);
  }

  public sealed class FilePlayerProfileRepository : IPlayerProfileRepository
  {
    private readonly string _path;
    public FilePlayerProfileRepository(string path) { _path = path; }
    public SaveData Load() => SaveStore.Load(_path);
    public void Save(SaveData profile) => SaveStore.Save(_path, profile);
  }

  public sealed class PlayerDataManager
  {
    private readonly IPlayerProfileRepository _repository;
    public SaveData Profile { get; private set; }

    public PlayerDataManager(IPlayerProfileRepository repository)
    {
      _repository = repository;
      Profile = repository.Load();
      if (string.IsNullOrWhiteSpace(Profile.PlayerId))
      {
        Profile.PlayerId = Guid.NewGuid().ToString("N");
        _repository.Save(Profile);
      }
    }

    public bool SelectZodiac(ZodiacType zodiac)
    {
      if (Profile.ZodiacSelectionComplete || zodiac == ZodiacType.None || !Enum.IsDefined(typeof(ZodiacType), zodiac)) return false;
      Profile.SelectedZodiac = zodiac;
      Profile.ZodiacSelectionComplete = true;
      Profile.OnboardingComplete = true;
      _repository.Save(Profile);
      return true;
    }

    public bool TryBuyBooster(BoosterType type, EconomyConfig config)
    {
      var gold = Profile.Gold;
      if (!EconomyService.TryPurchase(ref gold, Profile.Boosters, type, config)) return false;
      Profile.Gold = gold;
      _repository.Save(Profile);
      return true;
    }

    public void RecordVictory(int levelNumber, int stars, int score, int movesRemaining, int boostersUsed, int goldReward)
    {
      if (levelNumber < 1 || levelNumber > 1000) throw new ArgumentOutOfRangeException(nameof(levelNumber));
      if (!Profile.GeneratedLevels.TryGetValue(levelNumber, out var result))
      {
        result = new GeneratedLevelResult { FewestBoostersUsed = Math.Max(0, boostersUsed) };
        Profile.GeneratedLevels[levelNumber] = result;
      }
      var firstCompletion = !result.Completed;
      result.Completed = true;
      result.BestStars = Math.Max(result.BestStars, Math.Clamp(stars, 1, 3));
      result.BestScore = Math.Max(result.BestScore, Math.Max(0, score));
      result.BestMovesRemaining = Math.Max(result.BestMovesRemaining, Math.Max(0, movesRemaining));
      result.FewestBoostersUsed = Math.Min(result.FewestBoostersUsed, Math.Max(0, boostersUsed));
      Profile.HighestUnlockedLevel = Math.Max(Profile.HighestUnlockedLevel, Math.Min(1000, levelNumber + 1));
      if (firstCompletion) Profile.Gold = checked(Profile.Gold + Math.Max(0, goldReward));
      _repository.Save(Profile);
    }
  }
}
