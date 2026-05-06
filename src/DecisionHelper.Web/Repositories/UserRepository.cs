using DecisionHelper.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User> GetOrCreateByTelegramAsync(long telegramId, string locale, CancellationToken ct);
    Task<User> GetOrCreateByAnonAsync(Guid anonId, string locale, CancellationToken ct);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct);
    Task UpdateLocaleAsync(Guid id, string locale, CancellationToken ct);
    Task GrantPremiumAsync(Guid id, int days, DateTimeOffset now, CancellationToken ct);
    Task RevokePremiumDaysAsync(Guid id, int days, DateTimeOffset now, CancellationToken ct);
}

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User> GetOrCreateByTelegramAsync(long telegramId, string locale, CancellationToken ct)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
        if (existing is not null) return existing;

        var user = new User
        {
            Id = Guid.NewGuid(),
            TelegramId = telegramId,
            Locale = locale,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<User> GetOrCreateByAnonAsync(Guid anonId, string locale, CancellationToken ct)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.AnonId == anonId, ct);
        if (existing is not null) return existing;

        var user = new User
        {
            Id = Guid.NewGuid(),
            AnonId = anonId,
            Locale = locale,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task UpdateLocaleAsync(Guid id, string locale, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return;
        user.Locale = locale;
        await _db.SaveChangesAsync(ct);
    }

    public async Task GrantPremiumAsync(Guid id, int days, DateTimeOffset now, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new InvalidOperationException($"User {id} not found.");
        var baseTime = user.PremiumUntil.HasValue && user.PremiumUntil.Value > now ? user.PremiumUntil.Value : now;
        user.PremiumUntil = baseTime.AddDays(days);
        user.IsPremium = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokePremiumDaysAsync(Guid id, int days, DateTimeOffset now, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new InvalidOperationException($"User {id} not found.");
        if (!user.PremiumUntil.HasValue) return;
        var newUntil = user.PremiumUntil.Value.AddDays(-days);
        if (newUntil <= now)
        {
            user.PremiumUntil = null;
            user.IsPremium = false;
        }
        else
        {
            user.PremiumUntil = newUntil;
        }
        await _db.SaveChangesAsync(ct);
    }
}
