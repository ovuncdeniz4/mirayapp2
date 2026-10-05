using System.Collections.Generic;
using UnityEngine;

namespace Kut.Unity.Design
{
  public static class KutLocalization
  {
    private static readonly Dictionary<string, string> Tr = new Dictionary<string, string>
    {
      ["app.title"] = "Ötüken Kut'u: Erlik'in Gazabı",
      ["onboarding.title"] = "Takımyıldızını Seç",
      ["onboarding.disclaimer"] = "Özgün ve kurgusal bir oyun evrenidir; tarihsel bir geleneği temsil etmez.",
      ["onboarding.choose"] = "Yolculuğunda sana eşlik edecek kalıcı gücü seç.",
      ["home.title"] = "Ötüken Kut'u",
      ["continue"] = "Devam et", ["map"] = "Harita", ["animal"] = "Takımyıldızı",
      ["totem"] = "Totem", ["collection"] = "Koleksiyon", ["settings"] = "Ayarlar",
      ["pause"] = "Duraklat", ["quit"] = "Çık", ["back"] = "Geri", ["retry"] = "Tekrar dene",
      ["yes"] = "Evet", ["cancel"] = "İptal", ["moves"] = "Hamle", ["victory"] = "ZAFER",
      ["defeat"] = "Hamle kalmadı", ["loading"] = "Yükleniyor…",
      ["reshuffle"] = "Yeni bir yol açılıyor…",
      ["content_error"] = "Oyun içeriği yüklenemedi. İçerik dosyalarını kontrol et.",
      ["language"] = "Dil", ["music"] = "Müzik", ["sfx"] = "Ses efektleri",
      ["haptics"] = "Titreşim", ["reduced_motion"] = "Azaltılmış hareket",
      ["reset_save"] = "Kaydı sıfırla", ["stars"] = "Yıldız"
    };

    private static readonly Dictionary<string, string> En = new Dictionary<string, string>
    {
      ["app.title"] = "The Kut of Ötüken: Erlik's Wrath",
      ["onboarding.title"] = "Choose Your Constellation",
      ["onboarding.disclaimer"] = "An original fictional world; it does not represent a historical tradition.",
      ["onboarding.choose"] = "Choose the permanent power that will accompany your journey.",
      ["home.title"] = "The Kut of Ötüken",
      ["continue"] = "Continue", ["map"] = "Map", ["animal"] = "Constellation",
      ["totem"] = "Totem", ["collection"] = "Collection", ["settings"] = "Settings",
      ["pause"] = "Pause", ["quit"] = "Quit", ["back"] = "Back", ["retry"] = "Retry",
      ["yes"] = "Yes", ["cancel"] = "Cancel", ["moves"] = "Moves", ["victory"] = "VICTORY",
      ["defeat"] = "Out of moves", ["loading"] = "Loading…", ["reshuffle"] = "A new path is opening…",
      ["content_error"] = "Game content could not be loaded. Check the content files.",
      ["language"] = "Language", ["music"] = "Music", ["sfx"] = "Sound effects",
      ["haptics"] = "Haptics", ["reduced_motion"] = "Reduced motion",
      ["reset_save"] = "Reset save", ["stars"] = "Stars"
    };

    public static string Language { get; private set; } = "tr";

    public static void SetLanguage(string language) => Language = language == "en" ? "en" : "tr";

    public static string T(string key)
    {
      var table = Language == "en" ? En : Tr;
      return table.TryGetValue(key, out var value) ? value : key;
    }

    public static string SystemDefault() => Application.systemLanguage == SystemLanguage.English ? "en" : "tr";
  }
}
