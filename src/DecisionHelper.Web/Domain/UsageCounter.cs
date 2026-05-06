namespace DecisionHelper.Core.Domain;

public enum PeriodKind : short
{
    Day = 0,
    Month = 1,
}

public class UsageCounter
{
    public Guid UserId { get; set; }
    public PeriodKind PeriodKind { get; set; }
    public string PeriodKey { get; set; } = string.Empty;
    public int Count { get; set; }

    public static string DayKey(DateTimeOffset utcNow) => utcNow.UtcDateTime.ToString("yyyy-MM-dd");
    public static string MonthKey(DateTimeOffset utcNow) => utcNow.UtcDateTime.ToString("yyyy-MM");
}
