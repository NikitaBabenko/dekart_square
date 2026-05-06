namespace DecisionHelper.Core.Domain;

public class DecisionSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Dilemma { get; set; } = string.Empty;
    public string SquareJson { get; set; } = "{}";
    public string? Summary { get; set; }
    public string Locale { get; set; } = "en";
    public DateTimeOffset CreatedAt { get; set; }
}
