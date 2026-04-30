namespace DecisionHelper.Core.Limits;

public sealed record LimitPolicy(int FreeDailyLimit, int FreeMonthlyLimit, int PremiumMonthlyLimit)
{
    public static LimitPolicy Default { get; } = new(FreeDailyLimit: 3, FreeMonthlyLimit: 12, PremiumMonthlyLimit: 500);
}

public sealed record LimitCheckResult(bool Allowed, int DayCount, int MonthCount, string? ReasonCode)
{
    public static LimitCheckResult Ok(int day, int month) => new(true, day, month, null);
    public static LimitCheckResult DayExceeded(int day, int month) => new(false, day, month, "day_limit");
    public static LimitCheckResult MonthExceeded(int day, int month) => new(false, day, month, "month_limit");
}
