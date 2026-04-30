using DecisionHelper.Core.Domain;
using DecisionHelper.Core.Localization;
using DecisionHelper.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DecisionHelper.Web.Auth;

public interface ICurrentUserAccessor
{
    Task<User> GetOrCreateAsync(CancellationToken ct);
    string Locale { get; }
}

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    public const string AnonCookieName = "dh_anon";
    public const string TgSessionCookieName = "dh_tg";
    public const string LocaleCookieName = "dh_locale";

    private readonly IHttpContextAccessor _http;
    private readonly IUserRepository _users;
    private readonly AppOptions _options;
    private string? _resolvedLocale;
    private User? _cached;

    public CurrentUserAccessor(IHttpContextAccessor http, IUserRepository users, IOptions<AppOptions> options)
    {
        _http = http;
        _users = users;
        _options = options.Value;
    }

    public string Locale => _resolvedLocale ??= ResolveLocale();

    public async Task<User> GetOrCreateAsync(CancellationToken ct)
    {
        if (_cached is not null) return _cached;
        var ctx = _http.HttpContext ?? throw new InvalidOperationException("No HttpContext.");

        var tgCookie = ctx.Request.Cookies[TgSessionCookieName];
        if (!string.IsNullOrEmpty(tgCookie) && long.TryParse(tgCookie, out var tgId))
        {
            _cached = await _users.GetOrCreateByTelegramAsync(tgId, Locale, ct);
            return _cached;
        }

        var anonCookie = ctx.Request.Cookies[AnonCookieName];
        Guid anonId;
        if (string.IsNullOrEmpty(anonCookie) || !Guid.TryParse(anonCookie, out anonId))
        {
            anonId = Guid.NewGuid();
            ctx.Response.Cookies.Append(AnonCookieName, anonId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = ctx.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
            });
        }
        _cached = await _users.GetOrCreateByAnonAsync(anonId, Locale, ct);
        return _cached;
    }

    private string ResolveLocale()
    {
        var ctx = _http.HttpContext;
        if (ctx is null) return "en";

        var cookie = ctx.Request.Cookies[LocaleCookieName];
        if (!string.IsNullOrWhiteSpace(cookie))
            return SupportedLocales.Normalize(cookie);

        var accept = ctx.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrEmpty(accept)) return "en";
        var first = accept.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (first is null) return "en";
        var primary = first.Split(';')[0].Trim();
        return SupportedLocales.Normalize(primary);
    }
}
