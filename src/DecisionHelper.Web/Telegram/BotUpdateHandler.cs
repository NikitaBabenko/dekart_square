using DecisionHelper.Core.Domain;
using DecisionHelper.Core.Localization;
using AppUser = DecisionHelper.Core.Domain.User;
using DecisionHelper.Infrastructure.Repositories;
using DecisionHelper.Web.Auth;
using DecisionHelper.Web.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace DecisionHelper.Web.Telegram;

public sealed class BotUpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _users;
    private readonly IPaymentRepository _payments;
    private readonly DecisionService _decisions;
    private readonly IStringResolver _strings;
    private readonly AppOptions _options;
    private readonly ILogger<BotUpdateHandler> _logger;

    public BotUpdateHandler(
        ITelegramBotClient bot,
        IUserRepository users,
        IPaymentRepository payments,
        DecisionService decisions,
        IStringResolver strings,
        IOptions<AppOptions> options,
        ILogger<BotUpdateHandler> logger)
    {
        _bot = bot;
        _users = users;
        _payments = payments;
        _decisions = decisions;
        _strings = strings;
        _options = options.Value;
        _logger = logger;
    }

    public async Task HandleAsync(Update update, CancellationToken ct)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message when update.Message?.SuccessfulPayment is not null:
                    await HandleSuccessfulPaymentAsync(update.Message, ct);
                    break;
                case UpdateType.Message when update.Message is not null:
                    await HandleMessageAsync(update.Message, ct);
                    break;
                case UpdateType.PreCheckoutQuery when update.PreCheckoutQuery is not null:
                    await HandlePreCheckoutAsync(update.PreCheckoutQuery, ct);
                    break;
                default:
                    _logger.LogDebug("Ignoring update type {Type}", update.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle update {UpdateId}", update.Id);
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken ct)
    {
        if (message.From is null) return;
        var locale = SupportedLocales.Normalize(message.From.LanguageCode);
        var user = await _users.GetOrCreateByTelegramAsync(message.From.Id, locale, ct);
        if (user.Locale != locale && user.Locale == "en")
            await _users.UpdateLocaleAsync(user.Id, locale, ct);

        var text = message.Text?.Trim() ?? string.Empty;

        if (text.StartsWith('/'))
        {
            var parts = text.Split(' ', 2);
            var command = parts[0].ToLowerInvariant();
            var argument = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            switch (command)
            {
                case "/start":
                case "/help":
                    await _bot.SendMessage(message.Chat.Id, _strings[Strings.TgStartGreeting, user.Locale], cancellationToken: ct);
                    return;
                case "/premium":
                    await SendPremiumInvoiceAsync(message.Chat.Id, user.Locale, ct);
                    return;
                case "/refund":
                    await HandleRefundAsync(message, user, argument, ct);
                    return;
                default:
                    await _bot.SendMessage(message.Chat.Id, _strings[Strings.TgStartGreeting, user.Locale], cancellationToken: ct);
                    return;
            }
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            await _bot.SendMessage(message.Chat.Id, _strings[Strings.DilemmaEmpty, user.Locale], cancellationToken: ct);
            return;
        }

        await _bot.SendChatAction(message.Chat.Id, ChatAction.Typing, cancellationToken: ct);
        var outcome = await _decisions.AnalyzeAsync(user, text, user.Locale, ct);

        if (!outcome.Allowed)
        {
            await _bot.SendMessage(message.Chat.Id, outcome.ErrorMessage ?? _strings[Strings.TgError, user.Locale], cancellationToken: ct);
            return;
        }

        if (outcome.Square is null)
        {
            await _bot.SendMessage(message.Chat.Id, outcome.ErrorMessage ?? _strings[Strings.TgError, user.Locale], cancellationToken: ct);
            return;
        }

        var rendered = TelegramRenderer.RenderSquare(outcome.Square, _strings, user.Locale);
        await _bot.SendMessage(message.Chat.Id, rendered, parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleRefundAsync(Message message, AppUser user, string chargeId, CancellationToken ct)
    {
        if (message.From is null) return;
        if (!_options.AdminTelegramIds.Contains(message.From.Id))
        {
            // Silently behave like an unknown command for non-admins to avoid leaking the feature.
            await _bot.SendMessage(message.Chat.Id, _strings[Strings.TgStartGreeting, user.Locale], cancellationToken: ct);
            return;
        }

        if (string.IsNullOrEmpty(chargeId))
        {
            await _bot.SendMessage(message.Chat.Id, _strings[Strings.TgRefundUsage, user.Locale], cancellationToken: ct);
            return;
        }

        var payment = await _payments.FindByChargeIdAsync(chargeId, ct);
        if (payment is null)
        {
            await _bot.SendMessage(message.Chat.Id, _strings.Format(Strings.TgRefundNotFound, user.Locale, chargeId), cancellationToken: ct);
            return;
        }

        if (payment.RefundedAt.HasValue)
        {
            await _bot.SendMessage(message.Chat.Id, _strings.Format(Strings.TgRefundAlready, user.Locale, chargeId), cancellationToken: ct);
            return;
        }

        var paidUser = await _users.FindByIdAsync(payment.UserId, ct);
        if (paidUser?.TelegramId is null)
        {
            _logger.LogWarning("Cannot refund {ChargeId}: user has no TelegramId.", chargeId);
            await _bot.SendMessage(message.Chat.Id, _strings.Format(Strings.TgRefundNotFound, user.Locale, chargeId), cancellationToken: ct);
            return;
        }

        try
        {
            await _bot.RefundStarPayment(paidUser.TelegramId.Value, chargeId, cancellationToken: ct);
            var now = DateTimeOffset.UtcNow;
            await _payments.MarkRefundedAsync(payment.Id, now, ct);
            await _users.RevokePremiumDaysAsync(paidUser.Id, payment.PremiumDays, now, ct);
            await _bot.SendMessage(message.Chat.Id, _strings.Format(Strings.TgRefundOk, user.Locale, chargeId), cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefundStarPayment failed for charge {ChargeId}", chargeId);
            await _bot.SendMessage(message.Chat.Id, _strings[Strings.TgError, user.Locale], cancellationToken: ct);
        }
    }

    private async Task SendPremiumInvoiceAsync(long chatId, string locale, CancellationToken ct)
    {
        var line1 = _strings.Format(Strings.TgPremiumLine1, locale, _options.StarsPrice, _options.PremiumDays);
        var line2 = _strings[Strings.TgPremiumLine2, locale];
        var prices = new[] { new LabeledPrice($"Premium {_options.PremiumDays}d", _options.StarsPrice) };

        await _bot.SendInvoice(
            chatId: chatId,
            title: "DecisionHelper Premium",
            description: $"{line1}\n{line2}",
            payload: $"premium:{_options.PremiumDays}",
            currency: "XTR",
            prices: prices,
            providerToken: string.Empty,
            cancellationToken: ct);
    }

    private async Task HandlePreCheckoutAsync(PreCheckoutQuery q, CancellationToken ct)
    {
        await _bot.AnswerPreCheckoutQuery(q.Id, cancellationToken: ct);
    }

    private async Task HandleSuccessfulPaymentAsync(Message message, CancellationToken ct)
    {
        var sp = message.SuccessfulPayment;
        if (sp is null || message.From is null) return;

        if (await _payments.ExistsAsync(sp.TelegramPaymentChargeId, ct)) return;

        var locale = SupportedLocales.Normalize(message.From.LanguageCode);
        var user = await _users.GetOrCreateByTelegramAsync(message.From.Id, locale, ct);

        var days = _options.PremiumDays;
        var payloadParts = sp.InvoicePayload?.Split(':') ?? [];
        if (payloadParts.Length == 2 && payloadParts[0] == "premium" && int.TryParse(payloadParts[1], out var parsedDays))
            days = parsedDays;

        await _payments.RecordAsync(new Payment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TelegramPaymentChargeId = sp.TelegramPaymentChargeId,
            Stars = sp.TotalAmount,
            PremiumDays = days,
            CreatedAt = DateTimeOffset.UtcNow,
        }, ct);

        await _users.GrantPremiumAsync(user.Id, days, DateTimeOffset.UtcNow, ct);

        await _bot.SendMessage(message.Chat.Id, _strings[Strings.TgPremiumPaid, user.Locale], cancellationToken: ct);
    }
}
