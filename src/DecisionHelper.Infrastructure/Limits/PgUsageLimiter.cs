using DecisionHelper.Core.Domain;
using DecisionHelper.Core.Limits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DecisionHelper.Infrastructure.Limits;

public sealed class PgUsageLimiter : IUsageLimiter
{
    private readonly AppDbContext _db;
    private readonly LimitPolicy _policy;

    public PgUsageLimiter(AppDbContext db, IOptions<LimitPolicy> policy)
    {
        _db = db;
        _policy = policy.Value;
    }

    public async Task<LimitCheckResult> TryConsumeAsync(User user, DateTimeOffset now, CancellationToken ct)
    {
        var dayKey = UsageCounter.DayKey(now);
        var monthKey = UsageCounter.MonthKey(now);
        var isPremium = user.HasActivePremium(now);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var day = isPremium
            ? 0
            : await IncrementAsync(user.Id, PeriodKind.Day, dayKey, ct);
        var month = await IncrementAsync(user.Id, PeriodKind.Month, monthKey, ct);

        if (isPremium)
        {
            if (month > _policy.PremiumMonthlyLimit)
            {
                await tx.RollbackAsync(ct);
                return LimitCheckResult.MonthExceeded(0, month - 1);
            }
        }
        else
        {
            if (day > _policy.FreeDailyLimit)
            {
                await tx.RollbackAsync(ct);
                return LimitCheckResult.DayExceeded(day - 1, month - 1);
            }
            if (month > _policy.FreeMonthlyLimit)
            {
                await tx.RollbackAsync(ct);
                return LimitCheckResult.MonthExceeded(day - 1, month - 1);
            }
        }

        await tx.CommitAsync(ct);
        return LimitCheckResult.Ok(day, month);
    }

    public async Task<(int Day, int Month)> GetCurrentAsync(User user, DateTimeOffset now, CancellationToken ct)
    {
        var dayKey = UsageCounter.DayKey(now);
        var monthKey = UsageCounter.MonthKey(now);

        var day = await _db.UsageCounters
            .Where(c => c.UserId == user.Id && c.PeriodKind == PeriodKind.Day && c.PeriodKey == dayKey)
            .Select(c => (int?)c.Count).FirstOrDefaultAsync(ct) ?? 0;

        var month = await _db.UsageCounters
            .Where(c => c.UserId == user.Id && c.PeriodKind == PeriodKind.Month && c.PeriodKey == monthKey)
            .Select(c => (int?)c.Count).FirstOrDefaultAsync(ct) ?? 0;

        return (day, month);
    }

    private async Task<int> IncrementAsync(Guid userId, PeriodKind kind, string key, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO usage_counters (user_id, period_kind, period_key, count)
            VALUES (@u, @k, @p, 1)
            ON CONFLICT (user_id, period_kind, period_key)
            DO UPDATE SET count = usage_counters.count + 1
            RETURNING count;
            """;

        var conn = (NpgsqlConnection)_db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var tx = _db.Database.CurrentTransaction?.GetDbTransaction() as NpgsqlTransaction;
        if (tx is not null) cmd.Transaction = tx;
        cmd.Parameters.AddWithValue("u", userId);
        cmd.Parameters.AddWithValue("k", (short)kind);
        cmd.Parameters.AddWithValue("p", key);

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }
}
