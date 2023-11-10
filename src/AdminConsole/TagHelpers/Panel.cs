using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("panel")]
public class Panel : TagHelper
{
    [HtmlAttributeName("header")]
    public string Header { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "panel");
        output.Content.AppendHtml($"<h2>{Header}</h2>");

        var content = await output.GetChildContentAsync();
        output.Content.AppendHtml($"<div class='panel-content'>{content.GetContent()}</div>");
    }
}