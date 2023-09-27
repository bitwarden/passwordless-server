using AdminConsole.TagHelpers.Icons;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers.AlertBoxes;

[HtmlTargetElement("success-alert-box")]
public sealed class SuccessAlertBox : BaseAlertBox
{
    public SuccessAlertBox()
    {
        Variant = ColorVariant.Success;
        IconTag = SuccessAlertIcon.HtmlTag;
    }
}