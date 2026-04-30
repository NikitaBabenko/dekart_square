using System.Text.Json;
using DecisionHelper.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Infrastructure.Repositories;

public interface ISessionRepository
{
    Task<DecisionSession> SaveAsync(Guid userId, string dilemma, DecartesSquare square, string locale, CancellationToken ct);
    Task<IReadOnlyList<DecisionSession>> ListRecentAsync(Guid userId, int take, CancellationToken ct);
}

public sealed class SessionRepository : ISessionRepository
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    public SessionRepository(AppDbContext db) => _db = db;

    public async Task<DecisionSession> SaveAsync(Guid userId, string dilemma, DecartesSquare square, string locale, CancellationToken ct)
    {
        var session = new DecisionSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Dilemma = dilemma,
            SquareJson = JsonSerializer.Serialize(square, JsonOpts),
            Summary = square.Summary,
            Locale = locale,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.DecisionSessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<IReadOnlyList<DecisionSession>> ListRecentAsync(Guid userId, int take, CancellationToken ct)
    {
        return await _db.DecisionSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}
