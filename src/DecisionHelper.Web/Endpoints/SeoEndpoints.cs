using System.Text;
using DecisionHelper.Web.Auth;
using Microsoft.Extensions.Options;

namespace DecisionHelper.Web.Endpoints;

public static class SeoEndpoints
{
    private static readonly DateTimeOffset BuildTime =
        File.GetLastWriteTimeUtc(typeof(SeoEndpoints).Assembly.Location);

    private static readonly string[] Routes = ["/", "/app", "/pricing"];

    public static void MapSeoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sitemap.xml", (HttpContext ctx, IOptions<AppOptions> opts) =>
        {
            var baseUrl = ResolveBaseUrl(ctx, opts.Value);
            var lastmod = BuildTime.ToString("yyyy-MM-dd");

            var sb = new StringBuilder();
            sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
            sb.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");
            foreach (var route in Routes)
            {
                sb.AppendLine($"""  <url><loc>{baseUrl}{route}</loc><lastmod>{lastmod}</lastmod></url>""");
            }
            sb.AppendLine("</urlset>");

            return Results.Content(sb.ToString(), "application/xml; charset=utf-8");
        });
    }

    public static string ResolveBaseUrl(HttpContext ctx, AppOptions opts)
    {
        if (!string.IsNullOrEmpty(opts.AppBaseUrl))
            return opts.AppBaseUrl.TrimEnd('/');
        return $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    }
}
