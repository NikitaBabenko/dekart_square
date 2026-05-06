using DecisionHelper.Core.Domain;

namespace DecisionHelper.Core.AI;

public interface IAiClient
{
    Task<DecartesSquare> GenerateSquareAsync(string dilemma, string locale, CancellationToken ct);

    Task<DecartesSquare> SynthesizeFromUserInputAsync(
        string dilemma,
        IReadOnlyList<string> prosOfDoing,
        IReadOnlyList<string> consOfDoing,
        IReadOnlyList<string> prosOfNotDoing,
        IReadOnlyList<string> consOfNotDoing,
        string locale,
        CancellationToken ct);
}
