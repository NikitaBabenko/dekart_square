using DecisionHelper.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Infrastructure.Repositories;

public interface IPaymentRepository
{
    Task<bool> ExistsAsync(string telegramPaymentChargeId, CancellationToken ct);
    Task RecordAsync(Payment payment, CancellationToken ct);
}

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    public PaymentRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(string telegramPaymentChargeId, CancellationToken ct)
        => _db.Payments.AnyAsync(p => p.TelegramPaymentChargeId == telegramPaymentChargeId, ct);

    public async Task RecordAsync(Payment payment, CancellationToken ct)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);
    }
}
