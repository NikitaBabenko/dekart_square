using DecisionHelper.Core.AI;
using DecisionHelper.Core.Limits;
using DecisionHelper.Core.Localization;
using DecisionHelper.Infrastructure;
using DecisionHelper.Infrastructure.Repositories;
using DecisionHelper.Web.Auth;
using DecisionHelper.Web.Components;
using DecisionHelper.Web.Endpoints;
using DecisionHelper.Web.Services;
using DecisionHelper.Web.Telegram;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();

var appOptions = new AppOptions
{
    TelegramBotToken = builder.Configuration["TELEGRAM_BOT_TOKEN"] ?? string.Empty,
    TelegramBotUsername = (builder.Configuration["TELEGRAM_BOT_USERNAME"] ?? string.Empty).TrimStart('@'),
    TelegramWebhookSecret = builder.Configuration["TELEGRAM_WEBHOOK_SECRET"] ?? string.Empty,
    TelegramUsePolling = bool.TryParse(builder.Configuration["TELEGRAM_USE_POLLING"], out var poll) && poll,
    AppBaseUrl = builder.Configuration["APP_BASE_URL"] ?? string.Empty,
    StarsPrice = int.TryParse(builder.Configuration["STARS_PRICE"], out var sp) ? sp : 150,
    PremiumDays = int.TryParse(builder.Configuration["PREMIUM_DAYS"], out var pd) ? pd : 30,
    PostgresConnectionString = BuildPgConnectionString(builder.Configuration),
};
builder.Services.Configure<AppOptions>(o =>
{
    o.TelegramBotToken = appOptions.TelegramBotToken;
    o.TelegramBotUsername = appOptions.TelegramBotUsername;
    o.TelegramWebhookSecret = appOptions.TelegramWebhookSecret;
    o.TelegramUsePolling = appOptions.TelegramUsePolling;
    o.AppBaseUrl = appOptions.AppBaseUrl;
    o.StarsPrice = appOptions.StarsPrice;
    o.PremiumDays = appOptions.PremiumDays;
    o.PostgresConnectionString = appOptions.PostgresConnectionString;
});

builder.Services.Configure<OpenRouterOptions>(o =>
{
    o.ApiKey = builder.Configuration["OPENROUTER_API_KEY"] ?? string.Empty;
    o.Model = builder.Configuration["OPENROUTER_MODEL"] ?? string.Empty;
    o.BaseUrl = builder.Configuration["OPENROUTER_BASE_URL"] ?? "https://openrouter.ai/api/v1";
    o.AppUrl = appOptions.AppBaseUrl;
});

builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(LimitPolicy.Default));

builder.Services.AddDecisionHelperInfrastructure(appOptions.PostgresConnectionString);

builder.Services.AddHttpClient(OpenRouterClient.HttpClientName);
builder.Services.AddSingleton<IAiClient, OpenRouterClient>();

builder.Services.AddSingleton<IStringResolver>(_ => new InMemoryStringResolver(DefaultStrings.Build()));
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<DecisionService>();

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var token = appOptions.TelegramBotToken;
    return new TelegramBotClient(string.IsNullOrEmpty(token) ? "0:placeholder" : token);
});
builder.Services.AddScoped<BotUpdateHandler>();
builder.Services.AddHostedService<TelegramWebhookSetupService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { await db.Database.MigrateAsync(); }
    catch (InvalidOperationException) { await db.Database.EnsureCreatedAsync(); }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/healthz", () => Results.Ok(new { ok = true }));
app.MapTelegramWebhook();
app.MapAuthEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

static string BuildPgConnectionString(IConfiguration cfg)
{
    var host = cfg["POSTGRES_HOST"] ?? "localhost";
    var port = cfg["POSTGRES_PORT"] ?? "5432";
    var db = cfg["POSTGRES_DB"] ?? "decisionhelper";
    var user = cfg["POSTGRES_USER"] ?? "app";
    var pwd = cfg["POSTGRES_PASSWORD"] ?? string.Empty;
    return $"Host={host};Port={port};Database={db};Username={user};Password={pwd}";
}

public partial class Program { }
