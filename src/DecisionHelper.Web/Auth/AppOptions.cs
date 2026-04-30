namespace DecisionHelper.Web.Auth;

public sealed class AppOptions
{
    public string TelegramBotToken { get; set; } = string.Empty;
    public string TelegramBotUsername { get; set; } = string.Empty;
    public string TelegramWebhookSecret { get; set; } = string.Empty;
    public bool TelegramUsePolling { get; set; }
    public string AppBaseUrl { get; set; } = string.Empty;
    public int StarsPrice { get; set; } = 150;
    public int PremiumDays { get; set; } = 30;
    public string PostgresConnectionString { get; set; } = string.Empty;
}
