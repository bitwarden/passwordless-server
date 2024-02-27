using System.Text;
using Ganss.Xss;

namespace Passwordless.Common.Serialization;

public static class HtmlSanitizer
{
    private static readonly Ganss.Xss.HtmlSanitizer _htmlSanitizer = new(new HtmlSanitizerOptions
    {
        AllowedTags = new HashSet<string>(0)
    })
    {
        KeepChildNodes = true
    };

    public static string Sanitize(string input)
    {
        var temp = new StringBuilder(_htmlSanitizer.Sanitize(input));
        temp.Replace("https://", string.Empty).Replace("http://", string.Empty);
        return temp.ToString();
    }
}