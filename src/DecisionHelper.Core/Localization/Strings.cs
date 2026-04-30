namespace DecisionHelper.Core.Localization;

public static class Strings
{
    public const string AppTitle = "app.title";
    public const string AppTagline = "app.tagline";
    public const string CtaTry = "cta.try";
    public const string CtaPremium = "cta.premium";
    public const string DilemmaPlaceholder = "dilemma.placeholder";
    public const string DilemmaSubmit = "dilemma.submit";
    public const string DilemmaEmpty = "dilemma.empty";
    public const string ModeAi = "mode.ai";
    public const string ModeManual = "mode.manual";
    public const string Q1Title = "q1.title";
    public const string Q2Title = "q2.title";
    public const string Q3Title = "q3.title";
    public const string Q4Title = "q4.title";
    public const string SummaryHeader = "summary.header";
    public const string RecommendationHeader = "recommendation.header";
    public const string LimitDay = "limit.day";
    public const string LimitMonth = "limit.month";
    public const string LimitDayPremiumHint = "limit.day.premium_hint";
    public const string LimitMonthFreeHint = "limit.month.free_hint";
    public const string PremiumDescription = "premium.description";
    public const string PremiumActiveUntil = "premium.active_until";
    public const string TgStartGreeting = "tg.start.greeting";
    public const string TgPremiumLine1 = "tg.premium.line1";
    public const string TgPremiumLine2 = "tg.premium.line2";
    public const string TgPremiumPaid = "tg.premium.paid";
    public const string TgError = "tg.error";
    public const string Working = "working";
}

public static class DefaultStrings
{
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Build() =>
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                [Strings.AppTitle] = "Decision Helper — Cartesian Square with AI",
                [Strings.AppTagline] = "Get unstuck. Four questions, structured by AI.",
                [Strings.CtaTry] = "Try it now",
                [Strings.CtaPremium] = "Upgrade in Telegram",
                [Strings.DilemmaPlaceholder] = "Describe the decision you're weighing…",
                [Strings.DilemmaSubmit] = "Analyze",
                [Strings.DilemmaEmpty] = "Please describe your decision first.",
                [Strings.ModeAi] = "Let AI fill the square",
                [Strings.ModeManual] = "I'll write the answers myself",
                [Strings.Q1Title] = "What WILL happen if I do this?",
                [Strings.Q2Title] = "What will NOT happen if I do this?",
                [Strings.Q3Title] = "What WILL happen if I don't do this?",
                [Strings.Q4Title] = "What will NOT happen if I don't do this?",
                [Strings.SummaryHeader] = "Reflection",
                [Strings.RecommendationHeader] = "Direction",
                [Strings.LimitDay] = "Daily limit reached ({0}/{1}). Comes back in 24h (UTC).",
                [Strings.LimitMonth] = "Monthly limit reached ({0}/{1}).",
                [Strings.LimitDayPremiumHint] = "Premium has no daily limit — upgrade in the Telegram bot.",
                [Strings.LimitMonthFreeHint] = "Premium gives 500 monthly requests — upgrade in Telegram.",
                [Strings.PremiumDescription] = "30 days, 500 AI requests / month, no daily cap.",
                [Strings.PremiumActiveUntil] = "Premium active until {0}.",
                [Strings.TgStartGreeting] = "Hi! Send me a decision you're weighing and I'll run a Cartesian Square analysis. Use /premium for unlimited usage, /help for commands.",
                [Strings.TgPremiumLine1] = "Premium: {0} ⭐ for {1} days.",
                [Strings.TgPremiumLine2] = "500 AI requests / month, no daily limit.",
                [Strings.TgPremiumPaid] = "Premium activated. Thank you!",
                [Strings.TgError] = "Something went wrong. Please try again later.",
                [Strings.Working] = "Thinking…",
            },
            ["ru"] = new Dictionary<string, string>
            {
                [Strings.AppTitle] = "Decision Helper — Квадрат Декарта с ИИ",
                [Strings.AppTagline] = "Структурированный взгляд на решение за 4 вопроса.",
                [Strings.CtaTry] = "Попробовать",
                [Strings.CtaPremium] = "Премиум в Telegram",
                [Strings.DilemmaPlaceholder] = "Опишите решение, которое взвешиваете…",
                [Strings.DilemmaSubmit] = "Разобрать",
                [Strings.DilemmaEmpty] = "Сначала опишите своё решение.",
                [Strings.ModeAi] = "Пусть ИИ заполнит квадрат",
                [Strings.ModeManual] = "Заполню ответы сам",
                [Strings.Q1Title] = "Что БУДЕТ, если я это сделаю?",
                [Strings.Q2Title] = "Чего НЕ БУДЕТ, если я это сделаю?",
                [Strings.Q3Title] = "Что БУДЕТ, если я этого НЕ сделаю?",
                [Strings.Q4Title] = "Чего НЕ БУДЕТ, если я этого НЕ сделаю?",
                [Strings.SummaryHeader] = "Рефлексия",
                [Strings.RecommendationHeader] = "Направление",
                [Strings.LimitDay] = "Дневной лимит ({0}/{1}). Сбросится через 24ч (UTC).",
                [Strings.LimitMonth] = "Месячный лимит ({0}/{1}).",
                [Strings.LimitDayPremiumHint] = "В Premium нет дневного лимита — оформите в боте Telegram.",
                [Strings.LimitMonthFreeHint] = "Premium даёт 500 запросов в месяц — оформите в Telegram.",
                [Strings.PremiumDescription] = "30 дней, 500 AI-запросов в месяц, без дневного лимита.",
                [Strings.PremiumActiveUntil] = "Premium активен до {0}.",
                [Strings.TgStartGreeting] = "Привет! Пришлите решение, которое взвешиваете — разберу через Квадрат Декарта. /premium — безлимит, /help — команды.",
                [Strings.TgPremiumLine1] = "Premium: {0} ⭐ на {1} дней.",
                [Strings.TgPremiumLine2] = "500 AI-запросов в месяц, без дневного лимита.",
                [Strings.TgPremiumPaid] = "Premium активирован. Спасибо!",
                [Strings.TgError] = "Что-то пошло не так. Попробуйте позже.",
                [Strings.Working] = "Думаю…",
            },
        };
}
