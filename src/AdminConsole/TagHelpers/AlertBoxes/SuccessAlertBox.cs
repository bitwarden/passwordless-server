using Microsoft.AspNetCore.Razor.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Icons;

namespace Passwordless.AdminConsole.TagHelpers.AlertBoxes;

[HtmlTargetElement("success-alert-box")]
public sealed class SuccessAlertBox : BaseAlertBox
{
    public SuccessAlertBox()
    {
        Variant = ColorVariant.Success;
        IconTag = SuccessAlertIcon.HtmlTag;
    }
}