using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("local-time")]
public class LocalTime : TagHelper
{
    [HtmlAttributeName("datetime")]
    public DateTime DateTime { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.Add("datetime", DateTime.ToString("O"));
    }
}