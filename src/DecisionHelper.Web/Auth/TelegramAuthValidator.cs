using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace DecisionHelper.Web.Auth;

public sealed record TmaUser(long Id, string? Username, string? FirstName, string? LastName, string? LanguageCode);

public sealed record TmaAuthResult(bool IsValid, TmaUser? User, DateTimeOffset? AuthDate, string? Reason);

public sealed record LoginWidgetData(
    long Id,
    string? Username,
    string? FirstName,
    string? LastName,
    string? PhotoUrl,
    long AuthDate,
    string Hash);

public static class TelegramAuthValidator
{
    public static TmaAuthResult ValidateInitData(string initData, string botToken, TimeSpan maxAge, DateTimeOffset now)
    {
        if (string.IsNullOrEmpty(initData)) return new(false, null, null, "empty");
        if (string.IsNullOrEmpty(botToken)) return new(false, null, null, "no_bot_token");

        var parsed = HttpUtility.ParseQueryString(initData);
        var providedHash = parsed["hash"];
        if (string.IsNullOrEmpty(providedHash)) return new(false, null, null, "no_hash");

        var pairs = new List<(string Key, string Value)>();
        foreach (string? key in parsed.Keys)
        {
            if (key is null || key == "hash") continue;
            pairs.Add((key, parsed[key] ?? string.Empty));
        }
        pairs.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        var dataCheckString = string.Join('\n', pairs.Select(p => $"{p.Key}={p.Value}"));

        var secretKey = HmacSha256(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(botToken));
        var expectedHash = HmacSha256(secretKey, Encoding.UTF8.GetBytes(dataCheckString));
        var expectedHex = Convert.ToHexString(expectedHash).ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expectedHex),
                Encoding.ASCII.GetBytes(providedHash.ToLowerInvariant())))
        {
            return new(false, null, null, "bad_hash");
        }

        DateTimeOffset? authDate = null;
        if (long.TryParse(parsed["auth_date"], out var authUnix))
        {
            authDate = DateTimeOffset.FromUnixTimeSeconds(authUnix);
            if (now - authDate.Value > maxAge)
                return new(false, null, authDate, "stale");
        }

        TmaUser? user = null;
        var userJson = parsed["user"];
        if (!string.IsNullOrEmpty(userJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(userJson);
                var root = doc.RootElement;
                user = new TmaUser(
                    Id: root.GetProperty("id").GetInt64(),
                    Username: root.TryGetProperty("username", out var u) ? u.GetString() : null,
                    FirstName: root.TryGetProperty("first_name", out var fn) ? fn.GetString() : null,
                    LastName: root.TryGetProperty("last_name", out var ln) ? ln.GetString() : null,
                    LanguageCode: root.TryGetProperty("language_code", out var lc) ? lc.GetString() : null);
            }
            catch (JsonException)
            {
                return new(false, null, authDate, "bad_user_json");
            }
        }

        return new(true, user, authDate, null);
    }

    public static bool ValidateLoginWidget(LoginWidgetData data, string botToken, TimeSpan maxAge, DateTimeOffset now)
    {
        if (string.IsNullOrEmpty(botToken)) return false;
        if (now - DateTimeOffset.FromUnixTimeSeconds(data.AuthDate) > maxAge) return false;

        var pairs = new List<(string Key, string Value)>();
        void Add(string key, string? value) { if (value is not null) pairs.Add((key, value)); }
        Add("auth_date", data.AuthDate.ToString(CultureInfo.InvariantCulture));
        Add("first_name", data.FirstName);
        Add("id", data.Id.ToString(CultureInfo.InvariantCulture));
        Add("last_name", data.LastName);
        Add("photo_url", data.PhotoUrl);
        Add("username", data.Username);
        pairs.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        var dataCheckString = string.Join('\n', pairs.Select(p => $"{p.Key}={p.Value}"));

        var secretKey = SHA256.HashData(Encoding.UTF8.GetBytes(botToken));
        var expectedHash = HmacSha256(secretKey, Encoding.UTF8.GetBytes(dataCheckString));
        var expectedHex = Convert.ToHexString(expectedHash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expectedHex),
            Encoding.ASCII.GetBytes(data.Hash.ToLowerInvariant()));
    }

    private static byte[] HmacSha256(byte[] key, byte[] data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }
}
