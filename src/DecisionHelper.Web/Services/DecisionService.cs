using DecisionHelper.Core.AI;
using DecisionHelper.Core.Domain;
using DecisionHelper.Core.Limits;
using DecisionHelper.Core.Localization;
using DecisionHelper.Infrastructure.Repositories;

namespace DecisionHelper.Web.Services;

public sealed record SquareOutcome(bool Allowed, DecartesSquare? Square, LimitCheckResult LimitResult, string? ErrorMessage = null);

public sealed class DecisionService
{
    private readonly IAiClient _ai;
    private readonly IUsageLimiter _limiter;
    private readonly ISessionRepository _sessions;
    private readonly IStringResolver _strings;
    private readonly ILogger<DecisionService> _logger;

    public DecisionService(
        IAiClient ai,
        IUsageLimiter limiter,
        ISessionRepository sessions,
        IStringResolver strings,
        ILogger<DecisionService> logger)
    {
        _ai = ai;
        _limiter = limiter;
        _sessions = sessions;
        _strings = strings;
        _logger = logger;
    }

    public async Task<SquareOutcome> AnalyzeAsync(User user, string dilemma, string locale, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var limit = await _limiter.TryConsumeAsync(user, now, ct);
        if (!limit.Allowed)
        {
            return new SquareOutcome(false, null, limit, BuildLimitMessage(user, limit, locale, now));
        }

        try
        {
            var square = await _ai.GenerateSquareAsync(dilemma, locale, ct);
            await _sessions.SaveAsync(user.Id, dilemma, square, locale, ct);
            return new SquareOutcome(true, square, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI call failed for user {UserId}", user.Id);
            return new SquareOutcome(true, null, limit, _strings[Strings.TgError, locale]);
        }
    }

    public async Task<SquareOutcome> SynthesizeAsync(
        User user,
        string dilemma,
        IReadOnlyList<string> prosOfDoing,
        IReadOnlyList<string> consOfDoing,
        IReadOnlyList<string> prosOfNotDoing,
        IReadOnlyList<string> consOfNotDoing,
        string locale,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var limit = await _limiter.TryConsumeAsync(user, now, ct);
        if (!limit.Allowed)
        {
            return new SquareOutcome(false, null, limit, BuildLimitMessage(user, limit, locale, now));
        }

        try
        {
            var square = await _ai.SynthesizeFromUserInputAsync(
                dilemma, prosOfDoing, consOfDoing, prosOfNotDoing, consOfNotDoing, locale, ct);
            await _sessions.SaveAsync(user.Id, dilemma, square, locale, ct);
            return new SquareOutcome(true, square, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI synthesis failed for user {UserId}", user.Id);
            return new SquareOutcome(true, null, limit, _strings[Strings.TgError, locale]);
        }
    }

    private string BuildLimitMessage(User user, LimitCheckResult limit, string locale, DateTimeOffset now)
    {
        var policy = LimitPolicy.Default;
        if (limit.ReasonCode == "day_limit")
        {
            return _strings.Format(Strings.LimitDay, locale, limit.DayCount, policy.FreeDailyLimit) +
                   " " + _strings[Strings.LimitDayPremiumHint, locale];
        }
        if (limit.ReasonCode == "month_limit")
        {
            var max = user.HasActivePremium(now) ? policy.PremiumMonthlyLimit : policy.FreeMonthlyLimit;
            var hint = user.HasActivePremium(now) ? "" : " " + _strings[Strings.LimitMonthFreeHint, locale];
            return _strings.Format(Strings.LimitMonth, locale, limit.MonthCount, max) + hint;
        }
        return _strings[Strings.TgError, locale];
    }
}
