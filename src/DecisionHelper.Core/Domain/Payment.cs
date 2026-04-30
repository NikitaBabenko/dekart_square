namespace DecisionHelper.Core.Domain;

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TelegramPaymentChargeId { get; set; } = string.Empty;
    public int Stars { get; set; }
    public int PremiumDays { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
