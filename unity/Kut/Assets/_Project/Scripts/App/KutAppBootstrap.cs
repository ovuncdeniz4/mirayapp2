using System;
using System.Linq;
using System.Text;
using Kut.Core.Animals;
using Kut.Core.App;
using Kut.Core.Commands;
using Kut.Core.Levels;
using Kut.Core.Meta;
using Kut.Core.Save;
using Kut.Unity.Design;
using Kut.Unity.Presentation;
using Kut.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kut.Unity.App
{
  /// <summary>
  /// Runtime-built UX per docs/ux-ui-spec.md — add to empty scene and press Play.
  /// </summary>
  public sealed class KutAppBootstrap : MonoBehaviour
  {
    private GameContentBundle _content = null!;
    private SaveData _save = null!;
    private string _savePath = "";

    private GameObject _onboardingPanel = null!;
    private GameObject _homePanel = null!;
    private GameObject _mapPanel = null!;
    private GameObject _levelPanel = null!;
    private GameObject _animalPanel = null!;
    private GameObject _totemPanel = null!;
    private GameObject _collectionPanel = null!;

    private InputField? _monthInput;
    private InputField? _dayInput;
    private Text? _homeBody;
    private Text? _mapBody;
    private Text? _levelHud;
    private Text? _objectivesText;
    private BoardView? _boardView;
    private LevelSession? _levelSession;
    private string? _activeLevelId;

    private void Awake()
    {
      _savePath = UnitySavePaths.SaveFilePath;
      _content = UnityContentLoader.LoadBundle();
      _save = SaveStore.Load(_savePath);

      var canvas = KutUiFactory.CreateRootCanvas("KUT_UI");
      BuildOnboarding(canvas.transform);
      BuildHome(canvas.transform);
      BuildMap(canvas.transform);
      BuildLevel(canvas.transform);
      BuildAnimal(canvas.transform);
      BuildTotem(canvas.transform);
      BuildCollection(canvas.transform);

      ShowInitial();
    }

    private void ShowInitial()
    {
      HideAll();
      if (!_save.OnboardingComplete)
      {
        _onboardingPanel.SetActive(true);
        return;
      }

      RefreshHome();
      _homePanel.SetActive(true);
    }

    private void HideAll()
    {
      _onboardingPanel.SetActive(false);
      _homePanel.SetActive(false);
      _mapPanel.SetActive(false);
      _levelPanel.SetActive(false);
      _animalPanel.SetActive(false);
      _totemPanel.SetActive(false);
      _collectionPanel.SetActive(false);
    }

    private void BuildOnboarding(Transform root)
    {
      _onboardingPanel = KutUiFactory.Panel(root, "S01_Onboarding");
      KutUiFactory.Title(_onboardingPanel.transform, "KUT — Ruh Töreni");
      KutUiFactory.Body(_onboardingPanel.transform, "Disclaimer",
        new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.8f), 24).text =
        "Kurgusal oyun sistemi — tarihsel şaman geleneği değildir.";
      KutUiFactory.Body(_onboardingPanel.transform, "MonthLabel",
        new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.64f)).text = "Doğum ayın (1-12)";
      _monthInput = CreateInput(_onboardingPanel.transform, new Vector2(0.08f, 0.5f), new Vector2(0.92f, 0.56f));
      KutUiFactory.Body(_onboardingPanel.transform, "DayLabel",
        new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.48f)).text = "Doğum günün (1-31)";
      _dayInput = CreateInput(_onboardingPanel.transform, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.4f));
      KutUiFactory.PrimaryButton(_onboardingPanel.transform, "Ruhumu bul",
        new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.22f)).onClick.AddListener(OnOnboardingSubmit);
    }

    private void BuildHome(Transform root)
    {
      _homePanel = KutUiFactory.Panel(root, "S02_Home");
      KutUiFactory.Title(_homePanel.transform, "KUT — Ana Ekran", 48);
      _homeBody = KutUiFactory.Body(_homePanel.transform, "Body",
        new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.78f));
      KutUiFactory.PrimaryButton(_homePanel.transform, "Devam et",
        new Vector2(0.12f, 0.28f), new Vector2(0.88f, 0.38f)).onClick.AddListener(() => StartLevel(GameShell.ResolveContinueLevelId(_save, _content)));
      KutUiFactory.PrimaryButton(_homePanel.transform, "Harita",
        new Vector2(0.12f, 0.18f), new Vector2(0.45f, 0.26f)).onClick.AddListener(ShowMap);
      KutUiFactory.PrimaryButton(_homePanel.transform, "Hayvan",
        new Vector2(0.55f, 0.18f), new Vector2(0.88f, 0.26f)).onClick.AddListener(ShowAnimal);
      KutUiFactory.PrimaryButton(_homePanel.transform, "Totem",
        new Vector2(0.12f, 0.08f), new Vector2(0.45f, 0.16f)).onClick.AddListener(ShowTotem);
      KutUiFactory.PrimaryButton(_homePanel.transform, "Koleksiyon",
        new Vector2(0.55f, 0.08f), new Vector2(0.88f, 0.16f)).onClick.AddListener(ShowCollection);
    }

    private void BuildMap(Transform root)
    {
      _mapPanel = KutUiFactory.Panel(root, "S03_Map");
      KutUiFactory.Title(_mapPanel.transform, "Harita", 48);
      _mapBody = KutUiFactory.Body(_mapPanel.transform, "List",
        new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.78f));
      KutUiFactory.PrimaryButton(_mapPanel.transform, "Geri",
        new Vector2(0.2f, 0.06f), new Vector2(0.8f, 0.14f)).onClick.AddListener(() =>
      {
        HideAll();
        RefreshHome();
        _homePanel.SetActive(true);
      });
    }

    private void BuildLevel(Transform root)
    {
      _levelPanel = KutUiFactory.Panel(root, "S04_Level");
      _levelHud = KutUiFactory.Body(_levelPanel.transform, "Hud",
        new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f), 34);
      _objectivesText = KutUiFactory.Body(_levelPanel.transform, "Objectives",
        new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.87f), 28);

      var boardGo = new GameObject("BoardView", typeof(RectTransform));
      boardGo.transform.SetParent(_levelPanel.transform, false);
      _boardView = boardGo.AddComponent<BoardView>();

      KutUiFactory.PrimaryButton(_levelPanel.transform, "Geri",
        new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.1f)).onClick.AddListener(ExitLevel);
    }

    private void BuildAnimal(Transform root)
    {
      _animalPanel = KutUiFactory.Panel(root, "S05_Animal");
      KutUiFactory.Title(_animalPanel.transform, "Hayvan", 48);
      var body = KutUiFactory.Body(_animalPanel.transform, "Body",
        new Vector2(0.08f, 0.3f), new Vector2(0.92f, 0.7f));
      body.name = "AnimalBody";
      KutUiFactory.PrimaryButton(_animalPanel.transform, "Geri",
        new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.16f)).onClick.AddListener(BackHome);
    }

    private void BuildTotem(Transform root)
    {
      _totemPanel = KutUiFactory.Panel(root, "S06_Totem");
      KutUiFactory.Title(_totemPanel.transform, "Totem", 48);
      KutUiFactory.Body(_totemPanel.transform, "Body",
        new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.75f)).name = "TotemBody";
      KutUiFactory.PrimaryButton(_totemPanel.transform, "Geri",
        new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.16f)).onClick.AddListener(BackHome);
    }

    private void BuildCollection(Transform root)
    {
      _collectionPanel = KutUiFactory.Panel(root, "S07_Collection");
      KutUiFactory.Title(_collectionPanel.transform, "Koleksiyon", 48);
      KutUiFactory.Body(_collectionPanel.transform, "Body",
        new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.78f)).name = "CollectionBody";
      KutUiFactory.PrimaryButton(_collectionPanel.transform, "Geri",
        new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.16f)).onClick.AddListener(BackHome);
    }

    private void OnOnboardingSubmit()
    {
      _ = int.TryParse(_monthInput?.text, out var month);
      _ = int.TryParse(_dayInput?.text, out var day);
      if (month < 1)
      {
        month = 1;
      }

      if (day < 1)
      {
        day = 1;
      }

      GameShell.CompleteOnboarding(_save, month, day);
      SaveStore.Save(_savePath, _save);
      HideAll();
      RefreshHome();
      _homePanel.SetActive(true);
    }

    private void RefreshHome()
    {
      var sb = new StringBuilder();
      sb.AppendLine($"Ruh hayvanın: {AnimalAssignment.DisplayNameTr(_save.AnimalId)}");
      sb.AppendLine($"Bonus: {AnimalBonus.DescriptionTr(_save.AnimalId)}");
      sb.AppendLine($"İlerleme: seviye 1–{_save.HighestUnlockedLevel} / {_content.Chapters.AllLevelIds.Count}");
      if (_save.TotemTier > 0)
      {
        sb.AppendLine($"Totem katmanı: {_save.TotemTier}");
      }

      if (_save.Chapter2Complete)
      {
        sb.AppendLine("Bölüm 2 tamamlandı.");
      }

      sb.AppendLine();
      sb.AppendLine("Kurgusal oyun — tarihsel şaman geleneği değildir.");
      _homeBody!.text = sb.ToString();
    }

    private void ShowMap()
    {
      HideAll();
      var sb = new StringBuilder();
      var global = 0;
      foreach (var chapter in _content.Chapters.Chapters)
      {
        var accessible = MetaProgression.CanAccessChapter(chapter, _save, _content.Chapters);
        sb.AppendLine(accessible ? chapter.TitleTr : chapter.TitleTr + " 🔒");
        if (!accessible)
        {
          global += chapter.Levels.Count;
          continue;
        }

        foreach (var levelId in chapter.Levels)
        {
          global++;
          var locked = global > _save.HighestUnlockedLevel;
          var done = _save.Levels.TryGetValue(levelId, out var e) && e.Completed;
          sb.AppendLine($"  {(done ? "✓" : locked ? "🔒" : "→")} {global,2}. {levelId}");
        }
      }

      sb.AppendLine("\n(Demo: Devam et uses current level; full map pick in next iteration.)");
      _mapBody!.text = sb.ToString();
      _mapPanel.SetActive(true);
    }

    private void StartLevel(string levelId)
    {
      var json = UnityContentLoader.LoadLevelJson(levelId);
      var def = LevelLoader.Parse(json);
      _levelSession = GameShell.CreateLevelSession(_save, def);
      _activeLevelId = levelId;
      _boardView!.BindState(_levelSession.Engine.State);
      RefreshLevelHud();
      HideAll();
      _levelPanel.SetActive(true);
    }

    private void RefreshLevelHud()
    {
      if (_levelSession == null)
      {
        return;
      }

      _levelHud!.text = $"{_activeLevelId} | Hamle: {_levelSession.Engine.MovesRemaining}";
      var sb = new StringBuilder();
      for (var i = 0; i < _levelSession.Objectives.Definitions.Count; i++)
      {
        var d = _levelSession.Objectives.Definitions[i];
        var mark = _levelSession.Objectives.IsObjectiveComplete(i) ? "✓" : " ";
        sb.AppendLine($"[{mark}] {d.Type}: {_levelSession.Objectives.GetProgress(i)}/{d.Target}");
      }

      _objectivesText!.text = sb.ToString();
    }

    private void Update()
    {
      if (_levelSession == null || _levelSession.Outcome != LevelOutcome.InProgress)
      {
        return;
      }

      // Demo input: tap two cells via number keys — production uses swipe on BoardView colliders.
      if (Input.GetKeyDown(KeyCode.Alpha1))
      {
        TryDemoSwap(new Kut.Core.Board.GridPos(0, 0), new Kut.Core.Board.GridPos(1, 0));
      }
    }

    private void TryDemoSwap(Kut.Core.Board.GridPos a, Kut.Core.Board.GridPos b)
    {
      if (_levelSession == null || _boardView == null)
      {
        return;
      }

      var result = _levelSession.Submit(new SwapCommand(a, b));
      _boardView.ReplayEvents(result.Events, _levelSession.Engine.State);
      RefreshLevelHud();
      CheckLevelEnd();
    }

    private void CheckLevelEnd()
    {
      if (_levelSession == null || _activeLevelId == null)
      {
        return;
      }

      if (_levelSession.Outcome == LevelOutcome.Victory)
      {
        GameShell.ApplyVictory(_save, _content, _activeLevelId);
        SaveStore.Save(_savePath, _save);
        Debug.Log("*** ZAFER ***");
        ExitLevel();
      }
      else if (_levelSession.Outcome == LevelOutcome.Defeat)
      {
        Debug.Log("--- Yenilgi ---");
        ExitLevel();
      }
    }

    private void ExitLevel()
    {
      _levelSession = null;
      _activeLevelId = null;
      HideAll();
      RefreshHome();
      _homePanel.SetActive(true);
    }

    private void ShowAnimal()
    {
      HideAll();
      var body = _animalPanel.transform.Find("AnimalBody")?.GetComponent<Text>();
      if (body != null)
      {
        body.text =
          $"Kalıcı ruh eşleşmen: {AnimalAssignment.DisplayNameTr(_save.AnimalId)}\n\n{AnimalBonus.DescriptionTr(_save.AnimalId)}";
      }

      _animalPanel.SetActive(true);
    }

    private void ShowTotem()
    {
      if (!_save.TotemTabUnlocked)
      {
        Debug.Log("Totem kilitli (level 10).");
        return;
      }

      HideAll();
      var body = _totemPanel.transform.Find("TotemBody")?.GetComponent<Text>();
      if (body != null)
      {
        body.text =
          $"Totem katmanı: {_save.TotemTier}\n\nRuh yolu: Toprak/Su → Ateş/Rüzgar → Ruh (meta).";
      }

      _totemPanel.SetActive(true);
    }

    private void ShowCollection()
    {
      if (!_save.CollectionTabUnlocked)
      {
        Debug.Log("Koleksiyon kilitli (level 10).");
        return;
      }

      HideAll();
      var body = _collectionPanel.transform.Find("CollectionBody")?.GetComponent<Text>();
      if (body != null)
      {
        var sb = new StringBuilder();
        sb.AppendLine($"Relik: {_save.UnlockedCollectionIds.Count}/{_content.Collection.Items.Count}");
        foreach (var item in _content.Collection.Items)
        {
          var owned = _save.UnlockedCollectionIds.Contains(item.Id);
          sb.AppendLine($"{(owned ? "★" : "·")} {item.TitleTr}");
        }

        body.text = sb.ToString();
      }

      _collectionPanel.SetActive(true);
    }

    private void BackHome()
    {
      HideAll();
      RefreshHome();
      _homePanel.SetActive(true);
    }

    private static InputField CreateInput(Transform parent, Vector2 min, Vector2 max)
    {
      var go = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(InputField));
      go.transform.SetParent(parent, false);
      var rt = go.GetComponent<RectTransform>();
      rt.anchorMin = min;
      rt.anchorMax = max;
      rt.offsetMin = Vector2.zero;
      rt.offsetMax = Vector2.zero;
      go.GetComponent<Image>().color = KutDesignTokens.PanelSurface;

      var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
      textGo.transform.SetParent(go.transform, false);
      var text = textGo.GetComponent<Text>();
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 32;
      text.color = KutDesignTokens.TextPrimary;
      var trt = textGo.GetComponent<RectTransform>();
      trt.anchorMin = Vector2.zero;
      trt.anchorMax = Vector2.one;
      trt.offsetMin = new Vector2(10, 0);
      trt.offsetMax = new Vector2(-10, 0);

      var input = go.GetComponent<InputField>();
      input.textComponent = text;
      return input;
    }
  }
}
