using DecisionHelper.Core.Localization;
using DecisionHelper.Infrastructure.Repositories;
using DecisionHelper.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DecisionHelper.Web.Endpoints;

public static class AuthEndpoints
{
    public sealed record TmaLoginRequest(string InitData);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/tma", async (
            HttpContext ctx,
            [FromBody] TmaLoginRequest req,
            [FromServices] IOptions<AppOptions> options,
            [FromServices] IUserRepository users,
            CancellationToken ct) =>
        {
            var token = options.Value.TelegramBotToken;
            if (string.IsNullOrEmpty(token)) return Results.StatusCode(503);

            var result = TelegramAuthValidator.ValidateInitData(
                req.InitData, token, TimeSpan.FromHours(24), DateTimeOffset.UtcNow);

            if (!result.IsValid || result.User is null)
                return Results.Unauthorized();

            var locale = SupportedLocales.Normalize(result.User.LanguageCode);
            var user = await users.GetOrCreateByTelegramAsync(result.User.Id, locale, ct);

            ctx.Response.Cookies.Append(CurrentUserAccessor.TgSessionCookieName, user.TelegramId!.Value.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = ctx.Request.IsHttps,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                IsEssential = true,
            });

            return Results.Ok(new { ok = true, locale = user.Locale });
        });

        app.MapGet("/api/locale/{locale}", async (
            string locale,
            HttpContext ctx,
            [FromServices] ICurrentUserAccessor current,
            [FromServices] IUserRepository users,
            CancellationToken ct) =>
        {
            var normalized = SupportedLocales.Normalize(locale);
            ctx.Response.Cookies.Append(CurrentUserAccessor.LocaleCookieName, normalized, new CookieOptions
            {
                Secure = ctx.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
            });
            var user = await current.GetOrCreateAsync(ct);
            await users.UpdateLocaleAsync(user.Id, normalized, ct);
            var redirect = ctx.Request.Headers.Referer.ToString();
            return Results.Redirect(string.IsNullOrEmpty(redirect) ? "/" : redirect);
        });
    }
}
