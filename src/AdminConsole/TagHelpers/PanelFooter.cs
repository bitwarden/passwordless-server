using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("panel-footer")]
public class PanelFooter : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "mt-6 flex");

        var content = await output.GetChildContentAsync();
        output.Content.AppendHtml(content.GetContent());
    }
}