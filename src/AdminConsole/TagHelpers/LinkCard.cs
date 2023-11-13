using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("link-card")]
public class LinkCard : TagHelper
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string? Link { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {

        output.TagName = "a";
        output.Attributes.Add("href", Link);
        output.Attributes.Add("target", "_blank");

        var contentBuilder = new StringBuilder();
        contentBuilder.Append("<div class=\"relative flex rounded-lg border bg-white p-4 shadow-sm focus:outline-none\">");
        contentBuilder.Append($"""
                        <div class="flex-1 flex flex-col space-y-1">
                            <span class="block text-sm font-medium text-gray-900">{Title}</span>
                            <span class="flex items-center text-sm text-gray-500">{Description}</span>
                        </div>
                        """);
        if (!string.IsNullOrEmpty(Link))
        {
            contentBuilder.Append("""
                                  <div class="flex-none flex items-end">
                                      <p class="link-blue"><span>Learn more</span><span aria-hidden="true">→</span></p>
                                  </div>
                                  """);
        }
        contentBuilder.Append("</div>");


        output.Content.SetHtmlContent(contentBuilder.ToString());
    }
}