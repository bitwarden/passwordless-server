using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers.Icons;

[HtmlTargetElement(HtmlTag)]
public sealed class DangerAlertIcon : BaseAlertIcon
{
    public const string HtmlTag = "danger-alert-icon";

    public DangerAlertIcon()
    {
        Variant = ColorVariant.Danger;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.Content.AppendHtml(@"<path fill-rule=""evenodd"" d=""M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z"" clip-rule=""evenodd""></path>");
    }
}