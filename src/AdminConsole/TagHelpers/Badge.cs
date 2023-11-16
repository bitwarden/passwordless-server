using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers;

[HtmlTargetElement("badge")]
public class Badge : TagHelper
{
    private readonly string _baseClass = "rounded-md bg-blue-600 px-2 py-1 text-xs font-semibold text-white";

    public ColorVariant? Variant { get; set; }

    public string? Class { get; set; }

    public string Text { get; set; }

    private string GetClass()
    {
        var classBuilder = new StringBuilder("rounded-md px-2 py-1 text-xs font-semibold");

        switch (Variant)
        {
            case ColorVariant.Danger:
                classBuilder.Append(" bg-red-600 text-white");
                break;
            case ColorVariant.Success:
                classBuilder.Append(" bg-green-600 text-white");
                break;
            case ColorVariant.Warning:
                classBuilder.Append(" bg-yellow-600 text-white");
                break;
            default:
                classBuilder.Append(" bg-blue-600 text-white");
                break;
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classBuilder.Append(' ');
            classBuilder.Append(Class);
        }

        return classBuilder.ToString();
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Attributes.Add("class", GetClass());

        if (Text == null)
        {
            throw new ArgumentNullException(nameof(Text));
        }

        output.Content.AppendHtml(Text);
    }


}