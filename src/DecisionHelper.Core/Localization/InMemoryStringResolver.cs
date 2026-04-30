namespace DecisionHelper.Core.Localization;

public sealed class InMemoryStringResolver : IStringResolver
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _byLocale;

    public InMemoryStringResolver(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> byLocale)
    {
        _byLocale = byLocale;
    }

    public string this[string key, string locale]
    {
        get
        {
            var normalized = SupportedLocales.Normalize(locale);
            if (_byLocale.TryGetValue(normalized, out var table) && table.TryGetValue(key, out var value))
                return value;
            if (_byLocale.TryGetValue("en", out var en) && en.TryGetValue(key, out var fallback))
                return fallback;
            return key;
        }
    }

    public string Format(string key, string locale, params object[] args)
        => string.Format(this[key, locale], args);
}
