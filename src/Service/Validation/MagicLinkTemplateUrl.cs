using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MagicLinkTemplateUrl() : ValidationAttribute("Magic Link Url must be a valid uri and contain the token template (e.g. `<token>`).")
{
    private const string TokenTemplate = "<token>";

    public override bool IsValid(object value)
    {
        if (value is not string stringValue) return false;

        return stringValue.Contains(TokenTemplate)
               && Uri.TryCreate(stringValue, UriKind.Absolute, out Uri uriResult)
               && (uriResult!.Scheme == Uri.UriSchemeHttp
                   || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}