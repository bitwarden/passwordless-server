using System.ComponentModel.DataAnnotations;
using Passwordless.Common.Constants;

namespace Passwordless.Common.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class Base64UrlAttribute : RegularExpressionAttribute
{
    public Base64UrlAttribute() : base(RegularExpressions.Base64Url)
    {
    }
}