using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers.Icons;

public abstract class BaseAlertIcon : TagHelper
{
    private readonly string _baseClass = "h-5 w-5";

    public IconVariant? Variant { get; set; }

    private string GetClass()
    {
        if (!Variant.HasValue)
        {
            return _baseClass;
        }

        string colorClass = Variant switch
        {
            IconVariant.Danger => "text-red-400",
            IconVariant.Warning => "text-yellow-400",
            IconVariant.Info => "text-blue-400",
            IconVariant.Success => "text-green-400",
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