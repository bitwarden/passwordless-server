using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("asp-local-time")]
public class LocalTime : TagHelper
{
    [HtmlAttributeName("datetime")]
    public DateTime DateTime { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "local-time";
        output.Attributes.Add("datetime", DateTime.SpecifyKind(DateTime, DateTimeKind.Utc).ToString("O"));
    }
}