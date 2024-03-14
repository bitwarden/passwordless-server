using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Validation;

public class RequiredCollectionAttribute : RequiredAttribute
{
    public RequiredCollectionAttribute()
    {
        ErrorMessage = "The field {0} must be a collection where every item is required.";
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is not IEnumerable<object?> collection)
        {
            return false;
        }

        return collection.All(item => base.IsValid(item));
    }
}