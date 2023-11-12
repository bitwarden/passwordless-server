using Microsoft.AspNetCore.Razor.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Extensions;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("link-card")]
public class LinkCard : TagHelper
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string Link { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {

        output.TagName = "a";
        output.Attributes.Add("href", Link);
        output.Attributes.Add("target", "_blank");

        var card = new Card { Title = Title, Description = Description };
        var content = card.RenderHtml(context);

        output.Content.SetHtmlContent(content);
    }
}