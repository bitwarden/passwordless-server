using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NuGet.Protocol;

namespace AdminConsole.Helpers;

public static class RazorPageHtmlExtensions
{
    public static IHtmlContent ImportMap(this IHtmlHelper html, Dictionary<string, (string Dev, string Prod)> importMaps, string? nonce = null)
    {
        var map = new Dictionary<string, object>();
        var imports = new Dictionary<string, object> { ["imports"] = map };

        // check if we are in development environment
        var isDev = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

        foreach (var importMap in importMaps)
        {
            map[importMap.Key] = isDev ? importMap.Value.Dev : importMap.Value.Prod;
        }

        nonce ??= (html.ViewContext.HttpContext?.Items["csp-nonce"])?.ToString();
        var script = $"<script type=\"importmap\" nonce={nonce}>\n{imports.ToJson(Formatting.Indented)}\n</script>";
        return html.Raw(script);
    }
}