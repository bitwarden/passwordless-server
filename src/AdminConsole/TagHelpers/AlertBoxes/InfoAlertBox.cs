using Microsoft.AspNetCore.Razor.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Icons;

namespace Passwordless.AdminConsole.TagHelpers.AlertBoxes;

[HtmlTargetElement("info-alert-box")]
public sealed class InfoAlertBox : BaseAlertBox
{
    public InfoAlertBox()
    {
        Variant = ColorVariant.Info;
        IconTag = InfoAlertIcon.HtmlTag;
    }
}