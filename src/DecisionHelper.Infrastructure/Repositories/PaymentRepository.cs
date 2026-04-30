using DecisionHelper.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Infrastructure.Repositories;

public interface IPaymentRepository
{
    Task<bool> ExistsAsync(string telegramPaymentChargeId, CancellationToken ct);
    Task RecordAsync(Payment payment, CancellationToken ct);
    Task<Payment?> FindByChargeIdAsync(string telegramPaymentChargeId, CancellationToken ct);
    Task MarkRefundedAsync(Guid paymentId, DateTimeOffset refundedAt, CancellationToken ct);
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

    public Task<Payment?> FindByChargeIdAsync(string telegramPaymentChargeId, CancellationToken ct)
        => _db.Payments.FirstOrDefaultAsync(p => p.TelegramPaymentChargeId == telegramPaymentChargeId, ct);

    public async Task MarkRefundedAsync(Guid paymentId, DateTimeOffset refundedAt, CancellationToken ct)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found.");
        payment.RefundedAt = refundedAt;
        await _db.SaveChangesAsync(ct);
    }
}
