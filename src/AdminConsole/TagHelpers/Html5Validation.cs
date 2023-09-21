using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers;

public class Html5Validation
{

}

[HtmlTargetElement("input", Attributes = "asp-for")]
[HtmlTargetElement("textarea", Attributes = "asp-for")]
public class InputRequiredTagHelper : InputTagHelper
{
    public InputRequiredTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator)
    {
    }

    public override int Order => -1001; // beats the default order of -1000

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {

        // if id is not set and if type is radio and if value attribute is set, use both name and value for id
        // if (output.Attributes["id"] == null && context.AllAttributes["type"]?.Value.ToString() == "radio" && context.AllAttributes["value"] != null)
        // {
        //     output.Attributes.SetAttribute("id", $"{context.AllAttributes["asp-for"]?.-{context.AllAttributes["value"]?.Value}");
        // }
        await base.ProcessAsync(context, output);

        var metadata = For.Metadata as DefaultModelMetadata;
        bool hasRequiredAttribute = metadata
            ?.Attributes
            .PropertyAttributes
            .Any(i => i.GetType() == typeof(RequiredAttribute)) ?? false;
        if (hasRequiredAttribute)
        {
            output.Attributes.SetAttribute("required", "");
        }




    }
}