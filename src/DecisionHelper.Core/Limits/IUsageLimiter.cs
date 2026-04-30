using DecisionHelper.Core.Domain;

namespace DecisionHelper.Core.Limits;

public interface IUsageLimiter
{
    Task<LimitCheckResult> TryConsumeAsync(User user, DateTimeOffset now, CancellationToken ct);
    Task<(int Day, int Month)> GetCurrentAsync(User user, DateTimeOffset now, CancellationToken ct);
}
