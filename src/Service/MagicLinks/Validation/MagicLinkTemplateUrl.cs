using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.MagicLinks.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MagicLinkTemplateUrl : ValidationAttribute
{
    private const string TokenTemplate = "<token>";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not string stringValue || string.IsNullOrWhiteSpace(stringValue))
            return new ValidationResult("Value must be a non-empty string.");

        if (!stringValue.Contains(TokenTemplate))
            return new ValidationResult($"Value must contain the `{TokenTemplate}` template.");

        if (!(Uri.TryCreate(stringValue, UriKind.Absolute, out Uri uriResult)
              && (uriResult!.Scheme == Uri.UriSchemeHttp
                  || uriResult.Scheme == Uri.UriSchemeHttps)))
            return new ValidationResult("Value must be a valid url.");

        return ValidationResult.Success;
    }
}