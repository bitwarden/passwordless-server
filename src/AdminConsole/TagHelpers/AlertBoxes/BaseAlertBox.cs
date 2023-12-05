using Microsoft.AspNetCore.Razor.TagHelpers;
using Passwordless.AdminConsole.TagHelpers.Extensions;
using Passwordless.AdminConsole.TagHelpers.Icons;

namespace Passwordless.AdminConsole.TagHelpers.AlertBoxes;

public abstract class BaseAlertBox : TagHelper
{
    private readonly string _baseClass = "rounded-md p-4 my-3";

    public ColorVariant? Variant { get; set; }

    public string? IconTag { get; set; }

    public string? Class { get; set; }

    public string Message { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", GetClass());


        var icon = RenderIcon(context);

        var iconContainer = icon == null ? string.Empty : $"<div class=\"flex-shrink-0\">{icon}</div>";

        output.Content.AppendHtml($"""
                                   <div class="flex">
                                       {iconContainer}
                                       <div class="ml-3 {GetTextColorClass()} flex justify-between">
                                           <p class="alert-box-message">{Message}</p>
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

        return icon == null ? null : icon.RenderHtml(context);
    }

    private string GetClass()
    {
        var classes = new List<string>();
        classes.Add(_baseClass);

        if (Variant.HasValue)
        {
            string colorClass = Variant switch
            {
                ColorVariant.Danger => "bg-red-50",
                ColorVariant.Info => "bg-blue-50",
                ColorVariant.Success => "bg-green-50",
                _ => string.Empty
            };
            classes.Add(colorClass);
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(' ', classes);
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