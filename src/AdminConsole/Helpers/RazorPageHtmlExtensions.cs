using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using NuGet.Protocol;

namespace Passwordless.AdminConsole.Helpers;

public static class RazorPageHtmlExtensions
{
    public static IHtmlContent ImportMap(this IHtmlHelper html, Dictionary<string, (string Dev, string Prod)> importMaps, IFileVersionProvider fileVersionProvider, string? nonce = null)
    {
        var map = new Dictionary<string, object>();
        var imports = new Dictionary<string, object> { ["imports"] = map };

        // check if we are in development environment
        var isDev = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

        foreach (var importMap in importMaps)
        {
            map[importMap.Key] = isDev ? importMap.Value.Dev : importMap.Value.Prod;
            map[importMap.Key] = fileVersionProvider.AddFileVersionToPath(html.ViewContext.HttpContext.Request.PathBase, map[importMap.Key].ToString());
        }

        nonce ??= (html.ViewContext.HttpContext?.Items["csp-nonce"])?.ToString();
        var script = $"<script type=\"importmap\" nonce={nonce}>\n{imports.ToJson(Formatting.Indented)}\n</script>";
        return html.Raw(script);
    }

    private const string PartialViewScriptItemPrefix = "scripts_";

    /// <summary>
    /// Called in the partial view in place of where you would otherwise be using @section Scripts.
    /// </summary>
    /// <param name="htmlHelper"></param>
    /// <param name="template"></param>
    /// <returns></returns>
    public static IHtmlContent PartialSectionScripts(this IHtmlHelper htmlHelper, Func<object, HelperResult> template)
    {
        htmlHelper.ViewContext.HttpContext.Items[PartialViewScriptItemPrefix + Guid.NewGuid()] = template;
        return new HtmlContentBuilder();
    }

    /// <summary>
    /// Allows rendering of scripts from partial views in the layout. RenderPartialSectionScripts would typically be called in your shared layout, e.g. _Layout.cshtml in the standard scaffolded projects, and will render any scripts added in partials via the PartialSectionScripts method call.
    /// </summary>
    /// <param name="htmlHelper"></param>
    /// <returns></returns>
    public static IHtmlContent RenderPartialSectionScripts(this IHtmlHelper htmlHelper)
    {
        var partialSectionScripts = htmlHelper.ViewContext.HttpContext.Items.Keys
            .Where(k => Regex.IsMatch(
                k.ToString(),
                "^" + PartialViewScriptItemPrefix + "([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$"));
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