using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers.Extensions;

public static class TagHelperExtensions
{
    /**
     * Renders the specified tag helper as HTML.
     */
    public static string RenderHtml(this TagHelper tagHelper, TagHelperContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var output = new TagHelperOutput(
            string.Empty,
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
                Task.Factory.StartNew<TagHelperContent>(
                    () => new DefaultTagHelperContent()
                ));

        tagHelper.Process(context, output);
        return output.RenderHtml();
    }
}