using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
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

    private const string _partialViewScriptItemPrefix = "scripts_";
    public static IHtmlContent PartialSectionScripts(this IHtmlHelper htmlHelper, Func<object, HelperResult> template)
    {
        htmlHelper.ViewContext.HttpContext.Items[_partialViewScriptItemPrefix + Guid.NewGuid()] = template;
        return new HtmlContentBuilder();
    }
    public static IHtmlContent RenderPartialSectionScripts(this IHtmlHelper htmlHelper)
    {
        var partialSectionScripts = htmlHelper.ViewContext.HttpContext.Items.Keys
            .Where(k => Regex.IsMatch(
                k.ToString(),
                "^" + _partialViewScriptItemPrefix + "([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$"));
        var contentBuilder = new HtmlContentBuilder();
        foreach (var key in partialSectionScripts)
        {
            var template = htmlHelper.ViewContext.HttpContext.Items[key] as Func<object, HelperResult>;
            if (template != null)
            {
                var writer = new System.IO.StringWriter();
                template(null).WriteTo(writer, HtmlEncoder.Default);
                contentBuilder.AppendHtml(writer.ToString());
            }
        }
        return contentBuilder;
    }

}