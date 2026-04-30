using DecisionHelper.Web.Auth;
using DecisionHelper.Web.Telegram;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace DecisionHelper.Web.Endpoints;

public static class TelegramWebhookEndpoint
{
    public const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";

    public static void MapTelegramWebhook(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/telegram/webhook", async (
            HttpContext ctx,
            [FromBody] Update update,
            [FromServices] BotUpdateHandler handler,
            [FromServices] IOptions<AppOptions> options,
            CancellationToken ct) =>
        {
            var expectedSecret = options.Value.TelegramWebhookSecret;
            if (!string.IsNullOrEmpty(expectedSecret))
            {
                var provided = ctx.Request.Headers[SecretHeader].ToString();
                if (!string.Equals(provided, expectedSecret, StringComparison.Ordinal))
                    return Results.Unauthorized();
            }

            await handler.HandleAsync(update, ct);
            return Results.Ok();
        });
    }
}
