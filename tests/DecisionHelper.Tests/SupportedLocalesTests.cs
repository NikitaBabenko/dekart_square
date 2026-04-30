using DecisionHelper.Core.Localization;
using Xunit;

namespace DecisionHelper.Tests;

public class SupportedLocalesTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("EN", "en")]
    [InlineData("en-US", "en")]
    [InlineData("ru-RU", "ru")]
    [InlineData("zh_CN", "zh")]
    [InlineData("ar", "ar")]
    [InlineData("xx", "en")]
    [InlineData("", "en")]
    [InlineData(null, "en")]
    public void Normalize_returns_supported_primary(string? input, string expected)
    {
        Assert.Equal(expected, SupportedLocales.Normalize(input));
    }

    [Fact]
    public void Rtl_includes_ar_and_fa()
    {
        Assert.Contains("ar", SupportedLocales.Rtl);
        Assert.Contains("fa", SupportedLocales.Rtl);
        Assert.DoesNotContain("en", SupportedLocales.Rtl);
    }
}
