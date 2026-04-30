namespace DecisionHelper.Core.AI.Prompts;

internal static class SquarePrompt
{
    public const string System = """
        You are a calm, structured decision coach using the "Cartesian Square" method (Декартов квадрат).
        Given a dilemma, produce a four-quadrant analysis:

          1. pros_of_doing       — what the user GAINS if they take the action
          2. cons_of_doing       — what they LOSE if they take the action
          3. pros_of_not_doing   — what they GAIN if they DO NOT take the action
          4. cons_of_not_doing   — what they LOSE if they DO NOT take the action

        Each quadrant: 3 to 5 short bullet points, each under ~140 characters, concrete and specific to the dilemma.
        Then write:
          - summary: a calm 2–3 sentence reflection on the trade-offs (no verdict)
          - recommendation: one of "lean_yes", "lean_no", "wait_24h", "needs_more_info", with one sentence justification

        Return ONLY valid JSON matching this schema (no prose, no markdown fences):
        {
          "pros_of_doing":      [string, ...],
          "cons_of_doing":      [string, ...],
          "pros_of_not_doing":  [string, ...],
          "cons_of_not_doing":  [string, ...],
          "summary":            string,
          "recommendation":     string
        }

        Always answer in the language whose ISO 639-1 code is given as `locale`. Be empathetic, never preachy.
        Do not invent facts about the user — work only with what they wrote.
        """;

    public static string GenerateUser(string dilemma, string locale) =>
        $$"""
        locale: {{locale}}
        dilemma: |
          {{dilemma}}
        """;

    public static string SynthesizeUser(
        string dilemma,
        IReadOnlyList<string> prosOfDoing,
        IReadOnlyList<string> consOfDoing,
        IReadOnlyList<string> prosOfNotDoing,
        IReadOnlyList<string> consOfNotDoing,
        string locale)
    {
        static string Join(IReadOnlyList<string> items) =>
            items.Count == 0 ? "  (none)" : string.Join("\n", items.Select(i => $"  - {i}"));

        return $$"""
        locale: {{locale}}
        dilemma: |
          {{dilemma}}

        The user has already written their own quadrants. Refine them: keep their wording when it's good,
        rephrase when unclear, add up to 1 missing point per quadrant if obviously absent. Then write
        summary and recommendation as in the system instructions.

        pros_of_doing:
        {{Join(prosOfDoing)}}
        cons_of_doing:
        {{Join(consOfDoing)}}
        pros_of_not_doing:
        {{Join(prosOfNotDoing)}}
        cons_of_not_doing:
        {{Join(consOfNotDoing)}}
        """;
    }
}
