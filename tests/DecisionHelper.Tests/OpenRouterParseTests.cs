using DecisionHelper.Core.AI;
using Xunit;

namespace DecisionHelper.Tests;

public class OpenRouterParseTests
{
    [Fact]
    public void ParseSquare_handles_clean_json()
    {
        var json = """
            {
              "pros_of_doing": ["a", "b"],
              "cons_of_doing": ["c"],
              "pros_of_not_doing": ["d"],
              "cons_of_not_doing": ["e", "f"],
              "summary": "ok",
              "recommendation": "lean_yes — try it"
            }
            """;

        var square = OpenRouterClient.ParseSquare(json);

        Assert.Equal(2, square.ProsOfDoing.Count);
        Assert.Equal("a", square.ProsOfDoing[0]);
        Assert.Single(square.ConsOfDoing);
        Assert.Equal("ok", square.Summary);
        Assert.StartsWith("lean_yes", square.Recommendation);
    }

    [Fact]
    public void ParseSquare_handles_markdown_fenced_json()
    {
        var json = "```json\n{\"pros_of_doing\":[\"x\"],\"cons_of_doing\":[],\"pros_of_not_doing\":[],\"cons_of_not_doing\":[],\"summary\":\"s\",\"recommendation\":\"r\"}\n```";
        var square = OpenRouterClient.ParseSquare(json);
        Assert.Single(square.ProsOfDoing);
        Assert.Equal("s", square.Summary);
    }

    [Fact]
    public void ParseSquare_tolerates_missing_fields()
    {
        var json = "{\"pros_of_doing\":[\"only\"]}";
        var square = OpenRouterClient.ParseSquare(json);
        Assert.Single(square.ProsOfDoing);
        Assert.Empty(square.ConsOfDoing);
        Assert.Equal(string.Empty, square.Summary);
    }
}
