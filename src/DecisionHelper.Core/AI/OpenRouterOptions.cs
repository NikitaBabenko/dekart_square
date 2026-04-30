namespace DecisionHelper.Core.AI;

public sealed class OpenRouterOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string AppTitle { get; set; } = "DecisionHelper";
    public string AppUrl { get; set; } = "https://example.com";
}
