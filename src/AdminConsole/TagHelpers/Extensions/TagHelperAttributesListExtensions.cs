using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers.Extensions;

public static class TagHelperAttributesListExtensions
{
    public static string ToHtmlOutput(this TagHelperAttributeList attributes)
    {
        // Create a string builder to build the attribute string

        var list = new List<string>(attributes.Count);

        foreach (var attribute in attributes)
        {
            var item = new StringBuilder();
            item.Append($"{attribute.Name}");
            if (!string.IsNullOrEmpty(attribute.Value?.ToString()))
            {
                item.Append($"=\"{attribute.Value}\"");
            }
            list.Add(item.ToString());
        }

        // Convert the attribute string to a single string
        string attributesHtml = string.Join(' ', list);

        return attributesHtml;
    }
}