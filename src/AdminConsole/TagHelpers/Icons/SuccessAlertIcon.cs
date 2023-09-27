using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers.Icons;

[HtmlTargetElement("success-alert-icon")]
public sealed class SuccessAlertIcon : BaseAlertIcon
{
    public SuccessAlertIcon()
    {
        Variant = IconVariant.Success;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.Content.AppendHtml(@"<path fill-rule=""evenodd"" d=""M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"" clip-rule=""evenodd""/>");
    }
}