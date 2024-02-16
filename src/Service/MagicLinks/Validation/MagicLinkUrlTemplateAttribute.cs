using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.MagicLinks.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MagicLinkTemplateUrlAttribute : ValidationAttribute
{
    private const string TokenTemplate = "__TOKEN__";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not string stringValue || string.IsNullOrWhiteSpace(stringValue))
            return new ValidationResult($"You have provided a null or empty value for {validationContext.MemberName}.");

        if (!stringValue.Contains(TokenTemplate))
            return new ValidationResult($"You have provided a {validationContext.MemberName} without a {TokenTemplate} template. Please include it like so: https://www.example.com?token=__TOKEN__");

        if (!(Uri.TryCreate(stringValue, UriKind.Absolute, out var uriResult)
              && (uriResult.Scheme == Uri.UriSchemeHttp
                  || uriResult.Scheme == Uri.UriSchemeHttps)))
            return new ValidationResult($"You have provided a {validationContext.MemberName} that cannot be converted to a URL.");

        return ValidationResult.Success!;
    }
}