using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers.Extensions;

public static class TagHelperOutputExtensions
{
    /**
     * Converts the processed TagHelperOutput to a valid HTML tag.
     */
    public static string RenderHtml(this TagHelperOutput output)
    {
        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        var content = output.Content.GetContent();
        return $"<{output.TagName} {output.Attributes.ToHtmlOutput()}>{content}</{output.TagName}>";
    }
}