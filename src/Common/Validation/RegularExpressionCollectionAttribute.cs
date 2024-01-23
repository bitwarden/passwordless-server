using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class RegularExpressionCollectionAttribute(string pattern) : RegularExpressionAttribute(pattern)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not IEnumerable<string> collection)
        {
            throw new InvalidOperationException("This attribute can only be applied to collections");
        }

        foreach (var item in collection)
        {
            var result = base.IsValid(item, validationContext);
            if (result != ValidationResult.Success)
            {
                return result;
            }
        }

        return ValidationResult.Success;
    }
}