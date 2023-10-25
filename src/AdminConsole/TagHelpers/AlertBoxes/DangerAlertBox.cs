using Microsoft.AspNetCore.Razor.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Icons;

namespace Passwordless.AdminConsole.TagHelpers.AlertBoxes;

[HtmlTargetElement("danger-alert-box")]
public sealed class DangerAlertBox : BaseAlertBox
{
    public DangerAlertBox()
    {
        Variant = ColorVariant.Danger;
        IconTag = DangerAlertIcon.HtmlTag;
    }
}