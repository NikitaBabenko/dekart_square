using System.Reflection;
using DecisionHelper.Core.Localization;
using Xunit;

namespace DecisionHelper.Tests;

public class LocalizationCoverageTests
{
    [Fact]
    public void All_supported_locales_have_translations()
    {
        var dict = DefaultStrings.Build();
        foreach (var locale in SupportedLocales.All)
        {
            Assert.True(dict.ContainsKey(locale), $"Locale '{locale}' is missing from DefaultStrings.Build().");
        }
    }

    [Fact]
    public void All_locales_cover_every_string_key()
    {
        var keys = typeof(Strings)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToList();

        var dict = DefaultStrings.Build();

        foreach (var locale in SupportedLocales.All)
        {
            var table = dict[locale];
            foreach (var key in keys)
            {
                Assert.True(table.ContainsKey(key), $"Locale '{locale}' is missing key '{key}'.");
                Assert.False(string.IsNullOrWhiteSpace(table[key]), $"Locale '{locale}' has empty value for key '{key}'.");
            }
        }
    }

    [Fact]
    public void Format_string_placeholders_are_consistent_across_locales()
    {
        var dict = DefaultStrings.Build();
        var en = dict["en"];

        foreach (var locale in SupportedLocales.All)
        {
            if (locale == "en") continue;
            var table = dict[locale];
            foreach (var (key, enValue) in en)
            {
                var enPlaceholders = CountPlaceholders(enValue);
                if (enPlaceholders == 0) continue;
                var localePlaceholders = CountPlaceholders(table[key]);
                Assert.Equal(enPlaceholders, localePlaceholders);
            }
        }
    }

    private static int CountPlaceholders(string value)
    {
        var count = 0;
        for (var i = 0; i < value.Length - 1; i++)
        {
            if (value[i] == '{' && char.IsDigit(value[i + 1])) count++;
        }
        return count;
    }
}
