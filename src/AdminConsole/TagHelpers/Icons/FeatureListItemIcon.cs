using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers.Icons;

[HtmlTargetElement(HtmlTag)]
public class FeatureListItemIcon : TagHelper
{
    public const string HtmlTag = "feature-list-item-icon";
    
    private readonly string _baseClass = "text-blue-400";
    
    public string? Class { get; set; }
    
    private string GetClass()
    {
        if (string.IsNullOrWhiteSpace(Class))
        {
            return _baseClass;
        }

        return string.Join(' ', _baseClass, Class);
    }


    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg";
        output.Attributes.SetAttribute("class", GetClass());
        output.Attributes.SetAttribute("fill", "none");
        output.Attributes.SetAttribute("stroke", "currentColor");
        output.Attributes.SetAttribute("viewBox", "0 0 24 24");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute("xmlns", "http://www.w3.org/2000/svg"); 
        output.Content.AppendHtml(@"<path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""></path>");
    }
}