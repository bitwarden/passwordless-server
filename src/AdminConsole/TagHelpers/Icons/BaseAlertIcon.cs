using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers.Icons;

public abstract class BaseAlertIcon : TagHelper
{
    private readonly string _baseClass = "h-5 w-5";

    public ColorVariant? Variant { get; set; }

    private string GetClass()
    {
        if (!Variant.HasValue)
        {
            return _baseClass;
        }

        string colorClass = Variant switch
        {
            ColorVariant.Danger => "fill-red-400",
            ColorVariant.Info => "fill-blue-400",
            ColorVariant.Success => "fill-green-400",
            _ => null
        } ?? string.Empty;

        return string.Join(' ', _baseClass, colorClass);
    }


    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg";
        output.Attributes.SetAttribute("class", GetClass());
        output.Attributes.SetAttribute("fill", "currentColor");
        output.Attributes.SetAttribute("viewBox", "0 0 20 20");
        output.Attributes.SetAttribute("aria-hidden", "true");
    }
}