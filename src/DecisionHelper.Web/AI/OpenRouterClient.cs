using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DecisionHelper.Core.AI.Prompts;
using DecisionHelper.Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DecisionHelper.Core.AI;

public sealed class OpenRouterClient : IAiClient
{
    public const string HttpClientName = "openrouter";

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterClient> _logger;

    public OpenRouterClient(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenRouterOptions> options,
        ILogger<OpenRouterClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public Task<DecartesSquare> GenerateSquareAsync(string dilemma, string locale, CancellationToken ct)
        => CallAsync(SquarePrompt.GenerateUser(dilemma, locale), ct);

    public Task<DecartesSquare> SynthesizeFromUserInputAsync(
        string dilemma,
        IReadOnlyList<string> prosOfDoing,
        IReadOnlyList<string> consOfDoing,
        IReadOnlyList<string> prosOfNotDoing,
        IReadOnlyList<string> consOfNotDoing,
        string locale,
        CancellationToken ct)
        => CallAsync(
            SquarePrompt.SynthesizeUser(dilemma, prosOfDoing, consOfDoing, prosOfNotDoing, consOfNotDoing, locale),
            ct);

    private async Task<DecartesSquare> CallAsync(string userMessage, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("OPENROUTER_API_KEY is not configured.");
        if (string.IsNullOrWhiteSpace(_options.Model))
            throw new InvalidOperationException("OPENROUTER_MODEL is not configured.");

        var client = _httpClientFactory.CreateClient(HttpClientName);
        if (client.BaseAddress is null)
            client.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", _options.AppUrl);
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", _options.AppTitle);

        var request = new ChatRequest
        {
            Model = _options.Model,
            Temperature = 0.4,
            ResponseFormat = new ResponseFormat { Type = "json_object" },
            Messages =
            [
                new ChatMessage { Role = "system", Content = SquarePrompt.System },
                new ChatMessage { Role = "user", Content = userMessage },
            ],
        };

        using var response = await client.PostAsJsonAsync("chat/completions", request, JsonOpts, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("OpenRouter call failed: {Status} {Body}", response.StatusCode, body);
            throw new HttpRequestException($"OpenRouter returned {(int)response.StatusCode}");
        }

        var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOpts, ct)
            ?? throw new InvalidOperationException("Empty response from OpenRouter.");

        var content = payload.Choices.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("No choices in OpenRouter response.");

        return ParseSquare(content);
    }

    public static DecartesSquare ParseSquare(string content)
    {
        var json = ExtractJson(content);
        var dto = JsonSerializer.Deserialize<SquareDto>(json, JsonOpts)
            ?? throw new InvalidOperationException("Failed to parse square JSON.");

        return new DecartesSquare(
            dto.ProsOfDoing ?? new List<string>(),
            dto.ConsOfDoing ?? new List<string>(),
            dto.ProsOfNotDoing ?? new List<string>(),
            dto.ConsOfNotDoing ?? new List<string>(),
            dto.Summary ?? string.Empty,
            dto.Recommendation ?? string.Empty);
    }

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline > 0) trimmed = trimmed[(firstNewline + 1)..];
            var fenceClose = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fenceClose > 0) trimmed = trimmed[..fenceClose];
        }
        return trimmed.Trim();
    }

    private sealed class ChatRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
        [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = [];
        [JsonPropertyName("temperature")] public double? Temperature { get; set; }
        [JsonPropertyName("response_format")] public ResponseFormat? ResponseFormat { get; set; }
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
        [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
    }

    private sealed class ResponseFormat
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "json_object";
    }

    private sealed class ChatResponse
    {
        [JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = [];
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")] public ChatMessage? Message { get; set; }
    }

    private sealed class SquareDto
    {
        [JsonPropertyName("pros_of_doing")] public List<string>? ProsOfDoing { get; set; }
        [JsonPropertyName("cons_of_doing")] public List<string>? ConsOfDoing { get; set; }
        [JsonPropertyName("pros_of_not_doing")] public List<string>? ProsOfNotDoing { get; set; }
        [JsonPropertyName("cons_of_not_doing")] public List<string>? ConsOfNotDoing { get; set; }
        [JsonPropertyName("summary")] public string? Summary { get; set; }
        [JsonPropertyName("recommendation")] public string? Recommendation { get; set; }
    }
}
