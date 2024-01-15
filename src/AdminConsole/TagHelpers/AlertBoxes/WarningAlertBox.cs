using Microsoft.AspNetCore.Razor.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Icons;

namespace Passwordless.AdminConsole.TagHelpers.AlertBoxes;

[HtmlTargetElement("warning-alert-box")]
public sealed class WarningAlertBox : BaseAlertBox
{
    public WarningAlertBox()
    {
        Variant = ColorVariant.Warning;
        IconTag = WarningAlertIcon.HtmlTag;
    }
}