namespace DecisionHelper.Core.Domain;

public class User
{
    public Guid Id { get; set; }
    public long? TelegramId { get; set; }
    public Guid? AnonId { get; set; }
    public string Locale { get; set; } = "en";
    public bool IsPremium { get; set; }
    public DateTimeOffset? PremiumUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? MergedIntoUserId { get; set; }

    public bool HasActivePremium(DateTimeOffset now)
        => IsPremium && PremiumUntil.HasValue && PremiumUntil.Value > now;
}
