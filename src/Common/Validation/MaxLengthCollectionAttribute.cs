using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Validation;

public class MaxLengthCollectionAttribute : MaxLengthAttribute
{
    public MaxLengthCollectionAttribute(int length) : base(length)
    {
        ErrorMessage = "The field {0} must be a collection that contains strings with a maximum length of '{1}'.";
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