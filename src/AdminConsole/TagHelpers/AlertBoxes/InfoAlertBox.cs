using AdminConsole.TagHelpers.Icons;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers.AlertBoxes;

[HtmlTargetElement("info-alert-box")]
public sealed class InfoAlertBox : BaseAlertBox
{
    public InfoAlertBox()
    {
        Variant = ColorVariant.Info;
        IconTag = InfoAlertIcon.HtmlTag;
    }
}