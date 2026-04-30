using System.Net;
using System.Text;
using DecisionHelper.Core.Domain;
using DecisionHelper.Core.Localization;

namespace DecisionHelper.Web.Telegram;

internal static class TelegramRenderer
{
    public static string RenderSquare(DecartesSquare square, IStringResolver strings, string locale)
    {
        var sb = new StringBuilder();
        AppendSection(sb, strings[Strings.Q1Title, locale], square.ProsOfDoing);
        AppendSection(sb, strings[Strings.Q2Title, locale], square.ConsOfDoing);
        AppendSection(sb, strings[Strings.Q3Title, locale], square.ProsOfNotDoing);
        AppendSection(sb, strings[Strings.Q4Title, locale], square.ConsOfNotDoing);

        if (!string.IsNullOrWhiteSpace(square.Summary))
        {
            sb.Append("<b>").Append(WebUtility.HtmlEncode(strings[Strings.SummaryHeader, locale])).AppendLine("</b>");
            sb.AppendLine(WebUtility.HtmlEncode(square.Summary));
            sb.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(square.Recommendation))
        {
            sb.Append("<b>").Append(WebUtility.HtmlEncode(strings[Strings.RecommendationHeader, locale])).AppendLine("</b>");
            sb.AppendLine(WebUtility.HtmlEncode(square.Recommendation));
        }
        return sb.ToString();
    }

    private static void AppendSection(StringBuilder sb, string title, IReadOnlyList<string> items)
    {
        sb.Append("<b>").Append(WebUtility.HtmlEncode(title)).AppendLine("</b>");
        if (items.Count == 0)
        {
            sb.AppendLine("—");
        }
        else
        {
            foreach (var item in items) sb.Append("• ").AppendLine(WebUtility.HtmlEncode(item));
        }
        sb.AppendLine();
    }
}
