using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("card")]
public class Card : TagHelper
{
    public string Title { get; set; }

    public string Description { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.Add("class",
            "relative flex rounded-lg border bg-white p-4 shadow-sm focus:outline-none flex flex-col space-y-1");

        var content =
            $"""
             <span class="block text-sm font-medium text-gray-900">{Title}</span>
             <span class="flex items-center text-sm text-gray-500">{Description}</span>
             """;

        output.Content.SetHtmlContent(content);
    }
}