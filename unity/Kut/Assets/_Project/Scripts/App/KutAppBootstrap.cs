using System;
using System.Linq;
using System.Text;
using Kut.Core.Animals;
using Kut.Core.App;
using Kut.Core.Commands;
using Kut.Core.Levels;
using Kut.Core.Meta;
using Kut.Core.Objectives;
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
    private GameObject _onboardingRevealPanel = null!;
    private GameObject _homePanel = null!;
    private GameObject _mapPanel = null!;
    private GameObject _levelPanel = null!;
    private GameObject _animalPanel = null!;
    private GameObject _totemPanel = null!;
    private GameObject _collectionPanel = null!;

    private Image? _animalPortrait;
    private Image? _totemPortrait;
    private Image? _revealPortrait;

    private InputField? _monthInput;
    private InputField? _dayInput;
    private Text? _revealText;
    private Text? _homeBody;
    private Transform? _mapListRoot;
    private Text? _levelHud;
    private Text? _objectivesText;
    private BoardGridUi? _boardGrid;
    private ResultOverlayUi? _resultOverlay;
    private KutToastUi? _toast;
    private Image? _homePortrait;
    private Transform? _collectionGridRoot;
    private Button? _totemBtn;
    private Button? _collectionBtn;
    private LevelSession? _levelSession;
    private string? _activeLevelId;

    private void Awake()
    {
      _savePath = UnitySavePaths.SaveFilePath;
      _content = UnityContentLoader.LoadBundle();
      _save = SaveStore.Load(_savePath);

      var canvas = KutUiFactory.CreateRootCanvas("KUT_UI");
      BuildOnboarding(canvas.transform);
      BuildOnboardingReveal(canvas.transform);
      BuildHome(canvas.transform);
      BuildMap(canvas.transform);
      BuildLevel(canvas.transform);
      BuildAnimal(canvas.transform);
      BuildTotem(canvas.transform);
      BuildCollection(canvas.transform);
      _resultOverlay = canvas.gameObject.AddComponent<ResultOverlayUi>();
      _resultOverlay.Build(canvas.transform);
      _toast = KutToastUi.Create(canvas.transform);

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
      _onboardingRevealPanel.SetActive(false);
      _homePanel.SetActive(false);
      _mapPanel.SetActive(false);
      _levelPanel.SetActive(false);
      _animalPanel.SetActive(false);
      _totemPanel.SetActive(false);
      _collectionPanel.SetActive(false);
      _resultOverlay?.Hide();
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

    private void BuildOnboardingReveal(Transform root)
    {
      _onboardingRevealPanel = KutUiFactory.Panel(root, "S01b_Reveal");
      KutUiFactory.Title(_onboardingRevealPanel.transform, "Ruh hayvanın", 48);
      _revealPortrait = KutUiFactory.SpriteSlot(_onboardingRevealPanel.transform, "RevealPortrait",
        new Vector2(0.32f, 0.52f), new Vector2(0.68f, 0.78f));
      _revealText = KutUiFactory.Body(_onboardingRevealPanel.transform, "Reveal",
        new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.48f), 40);
      _revealText.alignment = TextAnchor.MiddleCenter;
      KutUiFactory.PrimaryButton(_onboardingRevealPanel.transform, "Ana ekrana git",
        new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.22f)).onClick.AddListener(() =>
      {
        HideAll();
        RefreshHome();
        _homePanel.SetActive(true);
      });
    }

    private void BuildHome(Transform root)
    {
      _homePanel = KutUiFactory.Panel(root, "S02_Home");
      KutUiFactory.Title(_homePanel.transform, "KUT — Ana Ekran", 48);
      _homePortrait = KutUiFactory.SpriteSlot(_homePanel.transform, "HomeAnimal",
        new Vector2(0.62f, 0.52f), new Vector2(0.92f, 0.82f));
      _homeBody = KutUiFactory.Body(_homePanel.transform, "Body",
        new Vector2(0.08f, 0.45f), new Vector2(0.58f, 0.78f));
      KutUiFactory.PrimaryButton(_homePanel.transform, "Devam et",
        new Vector2(0.12f, 0.28f), new Vector2(0.88f, 0.38f)).onClick.AddListener(() =>
        StartLevel(GameShell.ResolveContinueLevelId(_save, _content)));
      KutUiFactory.PrimaryButton(_homePanel.transform, "Harita",
        new Vector2(0.12f, 0.18f), new Vector2(0.45f, 0.26f)).onClick.AddListener(ShowMap);
      KutUiFactory.PrimaryButton(_homePanel.transform, "Hayvan",
        new Vector2(0.55f, 0.18f), new Vector2(0.88f, 0.26f)).onClick.AddListener(ShowAnimal);
      _totemBtn = KutUiFactory.PrimaryButton(_homePanel.transform, "Totem 🔒",
        new Vector2(0.12f, 0.08f), new Vector2(0.45f, 0.16f));
      _totemBtn.onClick.AddListener(ShowTotem);
      _collectionBtn = KutUiFactory.PrimaryButton(_homePanel.transform, "Koleksiyon 🔒",
        new Vector2(0.55f, 0.08f), new Vector2(0.88f, 0.16f));
      _collectionBtn.onClick.AddListener(ShowCollection);
    }

    private void BuildMap(Transform root)
    {
      _mapPanel = KutUiFactory.Panel(root, "S03_Map");
      KutUiFactory.Title(_mapPanel.transform, "Harita", 48);
      var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
      scrollGo.transform.SetParent(_mapPanel.transform, false);
      var srt = scrollGo.GetComponent<RectTransform>();
      srt.anchorMin = new Vector2(0.06f, 0.18f);
      srt.anchorMax = new Vector2(0.94f, 0.78f);
      srt.offsetMin = Vector2.zero;
      srt.offsetMax = Vector2.zero;
      scrollGo.GetComponent<Image>().color = KutDesignTokens.PanelSurface;

      var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
      viewport.transform.SetParent(scrollGo.transform, false);
      var vrt = viewport.GetComponent<RectTransform>();
      vrt.anchorMin = Vector2.zero;
      vrt.anchorMax = Vector2.one;
      vrt.offsetMin = Vector2.zero;
      vrt.offsetMax = Vector2.zero;
      viewport.GetComponent<Image>().color = KutDesignTokens.PanelSurface;
      viewport.GetComponent<Mask>().showMaskGraphic = false;

      var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
      content.transform.SetParent(viewport.transform, false);
      _mapListRoot = content.transform;
      var crt = content.GetComponent<RectTransform>();
      crt.anchorMin = new Vector2(0, 1);
      crt.anchorMax = new Vector2(1, 1);
      crt.pivot = new Vector2(0.5f, 1f);
      crt.offsetMin = new Vector2(0, crt.offsetMin.y);
      crt.offsetMax = new Vector2(0, crt.offsetMax.y);
      var vlg = content.GetComponent<VerticalLayoutGroup>();
      vlg.spacing = 8;
      vlg.childControlHeight = true;
      vlg.childForceExpandHeight = false;
      vlg.childControlWidth = true;
      vlg.childForceExpandWidth = true;
      vlg.padding = new RectOffset(8, 8, 8, 8);
      var fitter = content.GetComponent<ContentSizeFitter>();
      fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
      fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
      var scroll = scrollGo.GetComponent<ScrollRect>();
      scroll.viewport = vrt;
      scroll.content = crt;
      scroll.horizontal = false;
      scroll.vertical = true;
      scroll.movementType = ScrollRect.MovementType.Clamped;

      KutUiFactory.PrimaryButton(_mapPanel.transform, "Geri",
        new Vector2(0.2f, 0.06f), new Vector2(0.8f, 0.14f)).onClick.AddListener(BackHome);
    }

    private void BuildLevel(Transform root)
    {
      _levelPanel = KutUiFactory.Panel(root, "S04_Level");
      _levelHud = KutUiFactory.Body(_levelPanel.transform, "Hud",
        new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f), 34);
      _objectivesText = KutUiFactory.Body(_levelPanel.transform, "Objectives",
        new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.87f), 28);

      var boardHost = new GameObject("BoardHost", typeof(RectTransform));
      boardHost.transform.SetParent(_levelPanel.transform, false);
      var bhRt = boardHost.GetComponent<RectTransform>();
      bhRt.anchorMin = new Vector2(0, 0.12f);
      bhRt.anchorMax = new Vector2(1, 0.88f);
      bhRt.offsetMin = Vector2.zero;
      bhRt.offsetMax = Vector2.zero;
      _boardGrid = boardHost.AddComponent<BoardGridUi>();

      KutUiFactory.PrimaryButton(_levelPanel.transform, "Geri",
        new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.1f)).onClick.AddListener(ConfirmQuitLevel);
    }

    private void BuildAnimal(Transform root)
    {
      _animalPanel = KutUiFactory.Panel(root, "S05_Animal");
      KutUiFactory.Title(_animalPanel.transform, "Hayvan", 48);
      _animalPortrait = KutUiFactory.SpriteSlot(_animalPanel.transform, "AnimalPortrait",
        new Vector2(0.28f, 0.48f), new Vector2(0.72f, 0.82f));
      var body = KutUiFactory.Body(_animalPanel.transform, "Body",
        new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.46f));
      body.name = "AnimalBody";
      KutUiFactory.PrimaryButton(_animalPanel.transform, "Geri",
        new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.16f)).onClick.AddListener(BackHome);
    }

    private void BuildTotem(Transform root)
    {
      _totemPanel = KutUiFactory.Panel(root, "S06_Totem");
      KutUiFactory.Title(_totemPanel.transform, "Totem", 48);
      _totemPortrait = KutUiFactory.SpriteSlot(_totemPanel.transform, "TotemPortrait",
        new Vector2(0.25f, 0.38f), new Vector2(0.75f, 0.82f));
      KutUiFactory.Body(_totemPanel.transform, "Body",
        new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.36f)).name = "TotemBody";
      KutUiFactory.PrimaryButton(_totemPanel.transform, "Geri",
        new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.16f)).onClick.AddListener(BackHome);
    }

    private void BuildCollection(Transform root)
    {
      _collectionPanel = KutUiFactory.Panel(root, "S07_Collection");
      KutUiFactory.Title(_collectionPanel.transform, "Koleksiyon", 48);
      var scroll = new GameObject("CollectionScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
      scroll.transform.SetParent(_collectionPanel.transform, false);
      var csrt = scroll.GetComponent<RectTransform>();
      csrt.anchorMin = new Vector2(0.06f, 0.18f);
      csrt.anchorMax = new Vector2(0.94f, 0.78f);
      csrt.offsetMin = Vector2.zero;
      csrt.offsetMax = Vector2.zero;
      scroll.GetComponent<Image>().color = KutDesignTokens.PanelSurface;
      var gridGo = new GameObject("RelicGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
      gridGo.transform.SetParent(scroll.transform, false);
      _collectionGridRoot = gridGo.transform;
      var grid = gridGo.GetComponent<GridLayoutGroup>();
      grid.cellSize = new Vector2(140, 180);
      grid.spacing = new Vector2(12, 12);
      grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
      grid.constraintCount = 3;
      gridGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
      var cscroll = scroll.GetComponent<ScrollRect>();
      cscroll.content = gridGo.GetComponent<RectTransform>();
      cscroll.horizontal = false;
      KutUiFactory.Body(_collectionPanel.transform, "CollectionSummary",
        new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.17f), 26).name = "CollectionBody";
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
      _revealText!.text = AnimalAssignment.DisplayNameTr(_save.AnimalId);
      ApplyAnimalPortrait(_revealPortrait, _save.AnimalId);
      HideAll();
      _onboardingRevealPanel.SetActive(true);
    }

    private void RefreshHome()
    {
      var sb = new StringBuilder();
      sb.AppendLine($"Ruh hayvanın: {AnimalAssignment.DisplayNameTr(_save.AnimalId)}");
      sb.AppendLine($"Bonus: {AnimalBonus.DescriptionTr(_save.AnimalId)}");
      sb.AppendLine($"Seviye 1–{_save.HighestUnlockedLevel} / {_content.Chapters.AllLevelIds.Count} açık");
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
      ApplyAnimalPortrait(_homePortrait, _save.AnimalId);

      SetMetaButton(_totemBtn, _save.TotemTabUnlocked, "Totem");
      SetMetaButton(_collectionBtn, _save.CollectionTabUnlocked, "Koleksiyon");
    }

    private static void SetMetaButton(Button? btn, bool unlocked, string label)
    {
      if (btn == null)
      {
        return;
      }

      btn.interactable = unlocked;
      var text = btn.GetComponentInChildren<Text>();
      if (text != null)
      {
        text.text = unlocked ? label : label + " 🔒";
      }

      btn.GetComponent<Image>().color = unlocked ? KutDesignTokens.AccentGold : KutDesignTokens.PanelSurface;
    }

    private void ShowMap()
    {
      HideAll();
      RebuildMapList();
      _mapPanel.SetActive(true);
      Canvas.ForceUpdateCanvases();
      if (_mapListRoot != null)
      {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_mapListRoot.GetComponent<RectTransform>());
      }
    }

    private void RebuildMapList()
    {
      if (_mapListRoot == null)
      {
        return;
      }

      foreach (Transform child in _mapListRoot)
      {
        Destroy(child.gameObject);
      }

      var global = 0;
      foreach (var chapter in _content.Chapters.Chapters)
      {
        var accessible = MetaProgression.CanAccessChapter(chapter, _save, _content.Chapters);
        AddMapHeader(chapter.TitleTr + (accessible ? "" : " 🔒"));
        if (!accessible)
        {
          global += chapter.Levels.Count;
          continue;
        }

        foreach (var levelId in chapter.Levels)
        {
          global++;
          var idx = global;
          var locked = idx > _save.HighestUnlockedLevel;
          var done = _save.Levels.TryGetValue(levelId, out var e) && e.Completed;
          if (locked)
          {
            AddMapHeader($"  🔒 {idx,2}. {levelId}");
            continue;
          }

          var label = $"{(done ? "✓" : "→")} {idx,2}. {levelId}";
          var captured = levelId;
          AddMapButton(label, () => StartLevel(captured));
        }
      }
    }

    private void AddMapHeader(string text)
    {
      var go = new GameObject("Header", typeof(RectTransform), typeof(Text));
      go.transform.SetParent(_mapListRoot, false);
      var t = go.GetComponent<Text>();
      t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      t.fontSize = 28;
      t.color = KutDesignTokens.TextMuted;
      t.text = text;
    }

    private void AddMapButton(string label, Action onClick)
    {
      var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
      go.transform.SetParent(_mapListRoot, false);
      go.GetComponent<Image>().color = KutDesignTokens.AccentGold;
      go.GetComponent<LayoutElement>().minHeight = 72;
      var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
      textGo.transform.SetParent(go.transform, false);
      var txt = textGo.GetComponent<Text>();
      txt.text = label;
      txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      txt.fontSize = 30;
      txt.color = KutDesignTokens.BackgroundDeep;
      txt.alignment = TextAnchor.MiddleCenter;
      var trt = textGo.GetComponent<RectTransform>();
      trt.anchorMin = Vector2.zero;
      trt.anchorMax = Vector2.one;
      trt.offsetMin = Vector2.zero;
      trt.offsetMax = Vector2.zero;
      var btn = go.GetComponent<Button>();
      btn.onClick.AddListener(() => onClick());
    }

    private void StartLevel(string levelId)
    {
      var json = UnityContentLoader.LoadLevelJson(levelId);
      var def = LevelLoader.Parse(json);
      _levelSession = GameShell.CreateLevelSession(_save, def);
      _activeLevelId = levelId;

      _boardGrid!.SwapRequested -= OnSwapRequested;
      _boardGrid.ActivateRequested -= OnActivateRequested;
      _boardGrid.SwapRequested += OnSwapRequested;
      _boardGrid.ActivateRequested += OnActivateRequested;
      _boardGrid.Build(_levelSession.Engine.State);

      RefreshLevelHud();
      HideAll();
      _levelPanel.SetActive(true);
    }

    private void OnSwapRequested(Kut.Core.Board.GridPos a, Kut.Core.Board.GridPos b)
    {
      if (_levelSession == null || _boardGrid == null)
      {
        return;
      }

      var result = _levelSession.Submit(new SwapCommand(a, b));
      if (result.Events.Any(e => e.EventType == "swap_reverted"))
      {
        ShowToast("Geçersiz hamle — eşleşme yok");
      }

      _boardGrid.Refresh(_levelSession.Engine.State);
      RefreshLevelHud();
      CheckLevelEnd();
    }

    private void OnActivateRequested(Kut.Core.Board.GridPos at)
    {
      if (_levelSession == null || _boardGrid == null)
      {
        return;
      }

      _levelSession.Submit(new ActivateSpecialCommand(at));
      _boardGrid.Refresh(_levelSession.Engine.State);
      RefreshLevelHud();
      CheckLevelEnd();
    }

    private void RefreshLevelHud()
    {
      if (_levelSession == null)
      {
        return;
      }

      var moves = _levelSession.Engine.MovesRemaining;
      var chapter = _content.Chapters.Chapters.FirstOrDefault(c => c.Levels.Contains(_activeLevelId!));
      var chapterLabel = chapter?.TitleTr ?? "";
      _levelHud!.text = $"{chapterLabel}\n{_activeLevelId} | Hamle: {moves}";
      _levelHud.color = moves <= 3 ? KutDesignTokens.Danger : KutDesignTokens.TextPrimary;
      var sb = new StringBuilder();
      for (var i = 0; i < _levelSession.Objectives.Definitions.Count; i++)
      {
        var d = _levelSession.Objectives.Definitions[i];
        var mark = _levelSession.Objectives.IsObjectiveComplete(i) ? "✓" : " ";
        sb.AppendLine($"[{mark}] {ObjectiveLabelTr(d)}: {_levelSession.Objectives.GetProgress(i)}/{d.Target}");
      }

      _objectivesText!.text = sb.ToString();
    }

    private void CheckLevelEnd()
    {
      if (_levelSession == null || _activeLevelId == null || _resultOverlay == null)
      {
        return;
      }

      if (_levelSession.Outcome == LevelOutcome.Victory)
      {
        GameShell.ApplyVictory(_save, _content, _activeLevelId);
        SaveStore.Save(_savePath, _save);
        _resultOverlay.ShowVictory(() =>
        {
          ExitLevel();
        });
      }
      else if (_levelSession.Outcome == LevelOutcome.Defeat)
      {
        _resultOverlay.ShowDefeat(
          () => StartLevel(_activeLevelId),
          () =>
          {
            ExitLevel();
            ShowMap();
          });
      }
    }

    private void ConfirmQuitLevel()
    {
      ExitLevel();
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
      ApplyAnimalPortrait(_animalPortrait, _save.AnimalId);
      if (body != null)
      {
        body.text =
          $"Kalıcı ruh eşleşmen: {AnimalAssignment.DisplayNameTr(_save.AnimalId)}\n\n{AnimalBonus.DescriptionTr(_save.AnimalId)}";
        body.color = KutDesignTokens.TextPrimary;
      }

      _animalPanel.SetActive(true);
    }

    private void ShowTotem()
    {
      if (!_save.TotemTabUnlocked)
      {
        ShowToast("Totem — Bölüm 1 seviye 10");
        return;
      }

      HideAll();
      var body = _totemPanel.transform.Find("TotemBody")?.GetComponent<Text>();
      var stage = _save.TotemTier < 1 ? 1 : _save.TotemTier;
      ApplyTotemPortrait(_totemPortrait, stage);
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
        ShowToast("Koleksiyon — Bölüm 1 seviye 10");
        return;
      }

      HideAll();
      var body = _collectionPanel.transform.Find("CollectionBody")?.GetComponent<Text>();
      if (body != null)
      {
        body.text = $"Relik: {_save.UnlockedCollectionIds.Count}/{_content.Collection.Items.Count}";
      }

      RebuildCollectionGrid();
      _collectionPanel.SetActive(true);
    }

    private void BackHome()
    {
      HideAll();
      RefreshHome();
      _homePanel.SetActive(true);
    }

    private void RebuildCollectionGrid()
    {
      if (_collectionGridRoot == null)
      {
        return;
      }

      foreach (Transform child in _collectionGridRoot)
      {
        Destroy(child.gameObject);
      }

      foreach (var item in _content.Collection.Items)
      {
        var owned = _save.UnlockedCollectionIds.Contains(item.Id);
        var card = new GameObject(item.Id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(_collectionGridRoot, false);
        card.GetComponent<Image>().color = owned ? KutDesignTokens.PanelSurface : KutDesignTokens.BackgroundDeep;
        card.GetComponent<LayoutElement>().minHeight = 180;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(card.transform, false);
        var irt = iconGo.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0.1f, 0.28f);
        irt.anchorMax = new Vector2(0.9f, 0.95f);
        irt.offsetMin = Vector2.zero;
        irt.offsetMax = Vector2.zero;
        var icon = iconGo.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.color = owned ? Color.white : new Color(1, 1, 1, 0.25f);
        var sprite = KutArtCatalog.TryRelic(item.Id);
        if (sprite != null)
        {
          icon.sprite = sprite;
        }

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(card.transform, false);
        var trt = titleGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.05f, 0.02f);
        trt.anchorMax = new Vector2(0.95f, 0.26f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var t = titleGo.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 18;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = owned ? KutDesignTokens.TextPrimary : KutDesignTokens.TextMuted;
        t.text = owned ? item.TitleTr : "?";
      }
    }

    private static string ObjectiveLabelTr(ObjectiveDefinition d)
    {
      return d.Type switch
      {
        ObjectiveType.MakeMatches => "Eşleşme",
        ObjectiveType.CollectElement => d.Element == "water" ? "Su topla" : d.Element == "fire" ? "Ateş topla" : "Toprak topla",
        ObjectiveType.CascadeDepthInTurn => "Zincir",
        ObjectiveType.CreateSpecial => "Özel oluştur",
        ObjectiveType.ActivateSpecial => "Özel kullan",
        ObjectiveType.ClearObstacle => d.Obstacle == "mud" ? "Çamur temizle" : "Sarmaşık kır",
        _ => d.Type.ToString()
      };
    }

    private void ShowToast(string msg)
    {
      Debug.Log($"[Toast] {msg}");
      _toast?.Show(msg);
    }

    private static void ApplyAnimalPortrait(Image? target, string animalId)
    {
      if (target == null)
      {
        return;
      }

      var sprite = KutArtCatalog.TryAnimal(animalId);
      target.sprite = sprite;
      target.enabled = sprite != null;
    }

    private static void ApplyTotemPortrait(Image? target, int stage)
    {
      if (target == null)
      {
        return;
      }

      var sprite = KutArtCatalog.TryTotemStage(stage);
      target.sprite = sprite;
      target.enabled = sprite != null;
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
