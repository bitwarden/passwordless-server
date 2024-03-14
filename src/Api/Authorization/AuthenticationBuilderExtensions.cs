using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Passwordless.Service;
using Passwordless.Service.Helpers;

namespace Passwordless.Api.Authorization;

public static class AuthenticationBuilderExtensions
{
    public static void AddCustomSchemes(this AuthenticationBuilder builder)
    {
        builder.AddHeaderScheme<ISharedManagementService>(Constants.PublicKeyAuthenticationScheme, c =>
        {
            c.HeaderName = Constants.PublicKeyHeaderName;
            c.ClaimsCreator = async (managementService, apiKey) =>
            {
                var validationResult = await managementService.ValidatePublicKeyAsync(apiKey);

                var claims = new List<Claim>
                {
                    new (CustomClaimTypes.AccountName, validationResult.ApplicationId),
                    new (CustomClaimTypes.KeyType, Constants.PublicKeyType)
                };

                foreach (var scope in validationResult.Scopes)
                {
                    claims.Add(new(CustomClaimTypes.Scopes, scope));
                }

                return claims.ToArray();
            };
            c.ProblemDetailWriter = new DefaultProblemDetailWriter(Constants.SecretKeyHeaderName);
        });

        builder.AddHeaderScheme<ISharedManagementService>(Constants.SecretKeyAuthenticationScheme, c =>
        {
            c.HeaderName = Constants.SecretKeyHeaderName;
            c.ClaimsCreator = async (managementService, apiSecret) =>
            {
                var validationResult = await managementService.ValidateSecretKeyAsync(apiSecret);

                var claims = new List<Claim>
                {
                    new (CustomClaimTypes.AccountName, validationResult.ApplicationId),
                    new (CustomClaimTypes.KeyType, Constants.SecretKeyType)
                };

                foreach (var scope in validationResult.Scopes)
                {
                    claims.Add(new(CustomClaimTypes.Scopes, scope));
                }

                return claims.ToArray();
            };
            c.ProblemDetailWriter = new DefaultProblemDetailWriter(Constants.PublicKeyHeaderName);
        });

        builder.AddHeaderScheme<IOptions<ManagementOptions>>(Constants.ManagementKeyAuthenticationScheme, c =>
        {
            c.HeaderName = Constants.ManagementKeyHeaderName;
            c.ClaimsCreator = (optionsAccessor, key) =>
            {
                var options = optionsAccessor.Value;

                if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(key),
                    Encoding.UTF8.GetBytes(options.ManagementKey)))
                {
                    return Task.FromException<Claim[]>(new ApiException("Bad key", 401));
                }

                return Task.FromResult(new[]
                {
                    new Claim(CustomClaimTypes.KeyType, Constants.ManagementKeyType),
                });
            };
        });
    }

    private static AuthenticationBuilder AddHeaderScheme<TDep>(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<HeaderOptions<TDep>> configureOptions)
    {
        return builder.AddScheme<HeaderOptions<TDep>, HeaderHandler<TDep>>(
            authenticationScheme,
            configureOptions);
    }
}