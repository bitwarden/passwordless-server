using Humanizer;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;

namespace Passwordless.Service.Validation;

public static class TokenValidator
{
    /// <summary>
    /// Validates the register token request.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="features"></param>
    /// <exception cref="ApiException">Thrown when the attestation type is not supported.</exception>
    public static void ValidateAttestation(RegisterToken token, IFeaturesContext features)
    {
        if (token.Attestation.Equals("none", StringComparison.CurrentCultureIgnoreCase))
        {
            return;
        }

        // We won't support enterprise for now or other new attestation types known at this time.
        if (token.Attestation != "direct" && token.Attestation != "indirect")
        {
            throw new ApiException("invalid_attestation", "Attestation type not supported", 400);
        }

        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation type not supported on your plan", 400);
        }
    }

    public static void Validate(this Token token, DateTimeOffset now)
    {
        if (token.ExpiresAt >= now) return;

        var drift = now - token.ExpiresAt;

        throw new ApiException("expired_token", $"The token expired {drift.Humanize()} ago.", 403);
    }
}