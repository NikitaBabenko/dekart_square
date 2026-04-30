using DecisionHelper.Web.Auth;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DecisionHelper.Web.Telegram;

public sealed class TelegramWebhookSetupService : IHostedService
{
    private readonly ITelegramBotClient _bot;
    private readonly AppOptions _options;
    private readonly ILogger<TelegramWebhookSetupService> _logger;

    public TelegramWebhookSetupService(
        ITelegramBotClient bot,
        IOptions<AppOptions> options,
        ILogger<TelegramWebhookSetupService> logger)
    {
        _bot = bot;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.TelegramBotToken))
        {
            _logger.LogWarning("TELEGRAM_BOT_TOKEN not set; skipping webhook setup.");
            return;
        }

        if (_options.TelegramUsePolling)
        {
            _logger.LogInformation("TELEGRAM_USE_POLLING=true — webhook will be deleted; long polling expected.");
            await _bot.DeleteWebhook(dropPendingUpdates: false, cancellationToken: cancellationToken);
            return;
        }

        if (string.IsNullOrEmpty(_options.AppBaseUrl))
        {
            _logger.LogWarning("APP_BASE_URL not set; webhook not registered.");
            return;
        }

        var url = _options.AppBaseUrl.TrimEnd('/') + "/api/telegram/webhook";
        try
        {
            await _bot.SetWebhook(
                url: url,
                allowedUpdates: [UpdateType.Message, UpdateType.PreCheckoutQuery, UpdateType.CallbackQuery],
                secretToken: string.IsNullOrEmpty(_options.TelegramWebhookSecret) ? null : _options.TelegramWebhookSecret,
                cancellationToken: cancellationToken);
            _logger.LogInformation("Telegram webhook set to {Url}", url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set Telegram webhook.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
