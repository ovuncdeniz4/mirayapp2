using System.Collections.Generic;
using UnityEngine;

namespace Kut.Unity.Design
{
  public static class KutLocalization
  {
    private static readonly Dictionary<string, string> Tr = new Dictionary<string, string>
    {
      ["app.title"] = "KUT — Beş Element", ["onboarding.title"] = "KUT — Ruh Töreni",
      ["onboarding.disclaimer"] = "Kurgusal oyun sistemi — tarihsel şaman geleneği değildir.",
      ["onboarding.month"] = "Doğum ayın (1–12)", ["onboarding.day"] = "Doğum günün (1–31)",
      ["onboarding.find"] = "Ruhumu bul", ["onboarding.invalid_date"] = "Lütfen geçerli bir tarih gir.",
      ["home.title"] = "KUT — Ana Ekran", ["continue"] = "Devam et", ["map"] = "Harita",
      ["animal"] = "Hayvan", ["totem"] = "Totem", ["collection"] = "Koleksiyon",
      ["settings"] = "Ayarlar", ["pause"] = "Duraklat", ["quit"] = "Çık",
      ["back"] = "Geri", ["retry"] = "Tekrar dene", ["yes"] = "Evet", ["cancel"] = "İptal",
      ["moves"] = "Hamle", ["victory"] = "ZAFER", ["defeat"] = "Hamle kalmadı",
      ["loading"] = "Yükleniyor…", ["reshuffle"] = "Yeni bir yol açılıyor…",
      ["content_error"] = "Oyun içeriği yüklenemedi. Resources/Content dosyalarını kontrol et.",
      ["language"] = "Dil", ["music"] = "Müzik", ["sfx"] = "Ses efektleri",
      ["haptics"] = "Titreşim", ["reduced_motion"] = "Azaltılmış hareket",
      ["reset_save"] = "Kaydı sıfırla", ["stars"] = "Yıldız"
    };

    private static readonly Dictionary<string, string> En = new Dictionary<string, string>
    {
      ["app.title"] = "KUT — Five Elements", ["onboarding.title"] = "KUT — Spirit Ceremony",
      ["onboarding.disclaimer"] = "A fictional game world — not a representation of historical shamanic tradition.",
      ["onboarding.month"] = "Birth month (1–12)", ["onboarding.day"] = "Birth day (1–31)",
      ["onboarding.find"] = "Find my spirit", ["onboarding.invalid_date"] = "Please enter a valid date.",
      ["home.title"] = "KUT — Home", ["continue"] = "Continue", ["map"] = "Map",
      ["animal"] = "Animal", ["totem"] = "Totem", ["collection"] = "Collection",
      ["settings"] = "Settings", ["pause"] = "Pause", ["quit"] = "Quit",
      ["back"] = "Back", ["retry"] = "Retry", ["yes"] = "Yes", ["cancel"] = "Cancel",
      ["moves"] = "Moves", ["victory"] = "VICTORY", ["defeat"] = "Out of moves",
      ["loading"] = "Loading…", ["reshuffle"] = "A new path is opening…",
      ["content_error"] = "Game content could not be loaded. Check the Resources/Content files.",
      ["language"] = "Language", ["music"] = "Music", ["sfx"] = "Sound effects",
      ["haptics"] = "Haptics", ["reduced_motion"] = "Reduced motion",
      ["reset_save"] = "Reset save", ["stars"] = "Stars"
    };

    public static string Language { get; private set; } = "tr";

    public static void SetLanguage(string language)
    {
      Language = language == "en" ? "en" : "tr";
    }

    public static string T(string key)
    {
      var table = Language == "en" ? En : Tr;
      return table.TryGetValue(key, out var value) ? value : key;
    }

    public static string SystemDefault()
    {
      return Application.systemLanguage == SystemLanguage.English ? "en" : "tr";
    }
  }
}
