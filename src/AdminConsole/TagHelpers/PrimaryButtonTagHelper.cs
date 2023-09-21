using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers;

[HtmlTargetElement("*", Attributes = "btn-primary")]
public class PrimaryButtonTagHelper : TagHelper
{
    /*[HtmlAttributeName("asp-primary")]
    public bool Primary { get; set; }*/
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.AddClass("btn-primary", HtmlEncoder.Default);
    }
}

[HtmlTargetElement("*", Attributes = "btn-secondary")]
public class SecondaryButtonTagHelper : TagHelper
{
    /*[HtmlAttributeName("asp-primary")]
    public bool Primary { get; set; }*/
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var classes = "relative -ml-px inline-flex items-center gap-x-1.5 rounded-md px-3 py-2 text-sm font-semibold text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50";
        // for each class in classes, add it to the output
        foreach (var c in classes.Split(" "))
        {
            output.AddClass(c, HtmlEncoder.Default);
        }
    }
}