namespace DecisionHelper.Core.Domain;

public sealed record DecartesSquare(
    IReadOnlyList<string> ProsOfDoing,
    IReadOnlyList<string> ConsOfDoing,
    IReadOnlyList<string> ProsOfNotDoing,
    IReadOnlyList<string> ConsOfNotDoing,
    string Summary,
    string Recommendation);
