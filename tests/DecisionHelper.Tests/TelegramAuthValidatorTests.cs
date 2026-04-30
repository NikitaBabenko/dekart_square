using System.Security.Cryptography;
using System.Text;
using System.Web;
using DecisionHelper.Web.Auth;
using Xunit;

namespace DecisionHelper.Tests;

public class TelegramAuthValidatorTests
{
    private const string BotToken = "123456:TEST_TOKEN_FOR_UNIT_TEST";

    [Fact]
    public void ValidateInitData_accepts_correctly_signed_payload()
    {
        var now = DateTimeOffset.UtcNow;
        var initData = BuildInitData(BotToken, userId: 42, languageCode: "ru", now: now);

        var result = TelegramAuthValidator.ValidateInitData(initData, BotToken, TimeSpan.FromHours(1), now);

        Assert.True(result.IsValid);
        Assert.NotNull(result.User);
        Assert.Equal(42, result.User!.Id);
        Assert.Equal("ru", result.User.LanguageCode);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void ValidateInitData_rejects_tampered_hash()
    {
        var initData = BuildInitData(BotToken, userId: 42, languageCode: "en", now: DateTimeOffset.UtcNow);
        var tampered = initData.Replace("&hash=", "&hash=00");

        var result = TelegramAuthValidator.ValidateInitData(tampered, BotToken, TimeSpan.FromHours(1), DateTimeOffset.UtcNow);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateInitData_rejects_stale_payload()
    {
        var signedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var initData = BuildInitData(BotToken, userId: 42, languageCode: "en", now: signedAt);

        var result = TelegramAuthValidator.ValidateInitData(initData, BotToken, TimeSpan.FromMinutes(5), DateTimeOffset.UtcNow);

        Assert.False(result.IsValid);
        Assert.Equal("stale", result.Reason);
    }

    [Fact]
    public void ValidateInitData_rejects_empty()
    {
        var result = TelegramAuthValidator.ValidateInitData("", BotToken, TimeSpan.FromHours(1), DateTimeOffset.UtcNow);
        Assert.False(result.IsValid);
    }

    private static string BuildInitData(string botToken, long userId, string languageCode, DateTimeOffset now)
    {
        var authDate = now.ToUnixTimeSeconds().ToString();
        var userJson = $"{{\"id\":{userId},\"first_name\":\"Test\",\"language_code\":\"{languageCode}\"}}";

        var pairs = new List<(string Key, string Value)>
        {
            ("auth_date", authDate),
            ("query_id", "AAAA"),
            ("user", userJson),
        };
        pairs.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        var dataCheckString = string.Join('\n', pairs.Select(p => $"{p.Key}={p.Value}"));

        using var secretHmac = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData"));
        var secretKey = secretHmac.ComputeHash(Encoding.UTF8.GetBytes(botToken));
        using var hmac = new HMACSHA256(secretKey);
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString))).ToLowerInvariant();

        var encoded = string.Join("&", pairs.Select(p => $"{HttpUtility.UrlEncode(p.Key)}={HttpUtility.UrlEncode(p.Value)}"));
        return $"{encoded}&hash={hash}";
    }
}
