using AdminConsole.TagHelpers.Icons;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Extensions;

namespace AdminConsole.TagHelpers.AlertBoxes;

public abstract class BaseAlertBox : TagHelper
{
    protected readonly IHtmlGenerator HtmlGenerator;

    private readonly string _baseClass = "rounded-md p-4 my-3 w-full";

    public ColorVariant? Variant { get; set; }

    public string? IconTag { get; set; }

    public string Message { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", GetBackgroundColorClass());


        var icon = RenderIcon(context);

        var iconContainer = icon == null ? string.Empty : $"<div class=\"flex-shrink-0\">{icon}</div>";

        output.Content.AppendHtml($"""
                                   <div class="flex">
                                       {iconContainer}
                                       <div class="ml-3 {GetTextColorClass()} flex justify-between">
                                           <p class="text-sm font-medium">{Message}</p>
                                       </div>
                                   </div>
                                   """);
    }



    private string? RenderIcon(TagHelperContext context)
    {
        BaseAlertIcon? icon = Variant switch
        {
            ColorVariant.Danger => new DangerAlertIcon(),
            ColorVariant.Info => new InfoAlertIcon(),
            ColorVariant.Success => new SuccessAlertIcon(),
            _ => null
        };

        if (icon == null) return null;

        var iconOutput = new TagHelperOutput(
            string.Empty,
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
                Task.Factory.StartNew<TagHelperContent>(
                    () => new DefaultTagHelperContent()
                ));

        icon.Process(context, iconOutput);

        // {renderHtmlAttributes(iconOutput)}
        return $"<{iconOutput.TagName} {iconOutput.Attributes.ToHtmlOutput()}>{iconOutput.Content.GetContent()}</{iconOutput.TagName}>";
    }

    private string GetBackgroundColorClass()
    {
        if (!Variant.HasValue)
        {
            return _baseClass;
        }

        string colorClass = Variant switch
        {
            ColorVariant.Danger => "bg-red-50",
            ColorVariant.Info => "bg-blue-50",
            ColorVariant.Success => "bg-green-50",
            _ => string.Empty
        };

        return string.Join(' ', _baseClass, colorClass);
    }

    private string GetTextColorClass()
    {
        if (!Variant.HasValue)
        {
            return _baseClass;
        }

        string colorClass = Variant switch
        {
            ColorVariant.Danger => "text-red-800",
            ColorVariant.Info => "text-blue-800",
            ColorVariant.Success => "text-green-800",
            _ => string.Empty
        };

        return colorClass;
    }
}