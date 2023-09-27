using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Passwordless.AdminConsole.TagHelpers.Extensions;

public static class TagHelperAttributesListExtensions
{
    public static string ToHtmlOutput(this TagHelperAttributeList attributes)
    {
        // Create a string builder to build the attribute string
        var attributeString = new StringBuilder();

        foreach (var attribute in attributes)
        {
            // Append the attribute name
            attributeString.Append($" {attribute.Name}");

            // Append the attribute value if it exists
            if (!string.IsNullOrEmpty(attribute.Value?.ToString()))
            {
                attributeString.Append($"=\"{attribute.Value}\"");
            }
        }

        // Convert the attribute string to a single string
        string attributesHtml = attributeString.ToString();

        return attributesHtml;
    }
}