using AdminConsole.TagHelpers.Icons;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers.AlertBoxes;

[HtmlTargetElement("danger-alert-box")]
public sealed class DangerAlertBox : BaseAlertBox
{
    public DangerAlertBox()
    {
        Variant = ColorVariant.Danger;
        IconTag = DangerAlertIcon.HtmlTag;
    }
}