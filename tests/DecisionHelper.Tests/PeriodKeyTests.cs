using DecisionHelper.Core.Domain;
using Xunit;

namespace DecisionHelper.Tests;

public class PeriodKeyTests
{
    [Fact]
    public void DayKey_uses_utc_yyyymmdd()
    {
        var d = new DateTimeOffset(2026, 4, 30, 23, 59, 0, TimeSpan.Zero);
        Assert.Equal("2026-04-30", UsageCounter.DayKey(d));
    }

    [Fact]
    public void DayKey_in_local_offset_normalizes_to_utc()
    {
        // 30 April 2026, 02:00 in +05:00 = 29 April 2026, 21:00 UTC
        var d = new DateTimeOffset(2026, 4, 30, 2, 0, 0, TimeSpan.FromHours(5));
        Assert.Equal("2026-04-29", UsageCounter.DayKey(d));
    }

    [Fact]
    public void MonthKey_uses_utc_yyyymm()
    {
        var d = new DateTimeOffset(2026, 4, 30, 12, 0, 0, TimeSpan.Zero);
        Assert.Equal("2026-04", UsageCounter.MonthKey(d));
    }
}
