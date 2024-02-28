using System.ComponentModel.DataAnnotations;
using Passwordless.Common.Serialization;

namespace Passwordless.Common.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class NoForbiddenContentAttribute()
    : ValidationAttribute("The value contains forbidden content such as HTML tags.")
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        if (!(value is string))
        {
            throw new ArgumentException("This attribute can only be applied to strings.");
        }

        var valueString = (string)value;
        var sanitizedValue = HtmlSanitizer.Sanitize(valueString);
        return sanitizedValue == valueString;
    }
}