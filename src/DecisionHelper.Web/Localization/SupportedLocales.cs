using System.Globalization;

namespace DecisionHelper.Core.Localization;

public static class SupportedLocales
{
    public static readonly IReadOnlyList<string> All =
    [
        "en", "ru", "es", "pt", "de", "fr", "it", "tr", "uk", "pl",
        "ar", "hi", "zh", "ja", "ko", "vi", "id", "fa", "nl", "sv",
    ];

    public static readonly IReadOnlySet<string> Rtl = new HashSet<string> { "ar", "fa" };

    public static readonly IReadOnlyDictionary<string, string> DisplayNames = new Dictionary<string, string>
    {
        ["en"] = "English",
        ["ru"] = "Русский",
        ["es"] = "Español",
        ["pt"] = "Português",
        ["de"] = "Deutsch",
        ["fr"] = "Français",
        ["it"] = "Italiano",
        ["tr"] = "Türkçe",
        ["uk"] = "Українська",
        ["pl"] = "Polski",
        ["ar"] = "العربية",
        ["hi"] = "हिन्दी",
        ["zh"] = "中文",
        ["ja"] = "日本語",
        ["ko"] = "한국어",
        ["vi"] = "Tiếng Việt",
        ["id"] = "Bahasa Indonesia",
        ["fa"] = "فارسی",
        ["nl"] = "Nederlands",
        ["sv"] = "Svenska",
    };

    public static string Normalize(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)) return "en";
        var lower = candidate.Trim().ToLowerInvariant();
        var primary = lower.Split('-', '_')[0];
        return All.Contains(primary) ? primary : "en";
    }

    public static CultureInfo ToCulture(string locale)
    {
        try { return CultureInfo.GetCultureInfo(locale); }
        catch (CultureNotFoundException) { return CultureInfo.GetCultureInfo("en"); }
    }
}
