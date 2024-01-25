using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Validation;

/// <summary>
/// Validate collections of strings against a regular expression.
/// </summary>
/// <param name="pattern"></param>
[AttributeUsage(AttributeTargets.Property)]
public class RegularExpressionCollectionAttribute(string pattern) : RegularExpressionAttribute(pattern)
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is not IEnumerable<string> collection)
        {
            throw new InvalidOperationException("This attribute can only be applied to collections.");
        }

        foreach (var item in collection)
        {
            var result = base.IsValid(item);
            if (!result)
            {
                return result;
            }
        }

        return true;
    }
}