using System.ComponentModel.DataAnnotations;
using Passwordless.Common.MagicLinks.Models;

namespace Passwordless.Common.MagicLinks.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MagicLinkTemplateUrlAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not string stringValue || string.IsNullOrWhiteSpace(stringValue))
            return new ValidationResult($"You have provided a null or empty value for {validationContext.MemberName}.");

        if (!stringValue.Contains(SendMagicLinkRequest.TokenTemplate, StringComparison.CurrentCultureIgnoreCase))
            return new ValidationResult($"You have provided a {validationContext.MemberName} without a {SendMagicLinkRequest.TokenTemplate} template. Please include it like so: https://www.example.com?token={SendMagicLinkRequest.TokenTemplate}");

        if (!(Uri.TryCreate(stringValue, UriKind.Absolute, out var uriResult)
              && (uriResult.Scheme == Uri.UriSchemeHttp
                  || uriResult.Scheme == Uri.UriSchemeHttps)))
            return new ValidationResult($"You have provided a {validationContext.MemberName} that cannot be converted to a URL.");

        return ValidationResult.Success!;
    }
}