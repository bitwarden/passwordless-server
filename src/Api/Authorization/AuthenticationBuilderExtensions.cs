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
    public static AuthenticationBuilder AddCustomSchemes(this AuthenticationBuilder builder)
    {
        builder.AddHeaderScheme<ISharedManagementService>("ApiKey", options =>
        {
            options.HeaderName = "ApiKey";
            options.ClaimsCreator = async (managementService, apiKey) =>
            {
                var validationResult = await managementService.ValidatePublicKeyAsync(apiKey);

                var claims = new List<Claim>
                {
                    new (CustomClaimTypes.AccountName, validationResult.ApplicationId),
                    new (CustomClaimTypes.KeyType, "public")
                };

                foreach (var scope in validationResult.Scopes)
                {
                    claims.Add(new(CustomClaimTypes.Scopes, scope));
                }

                return claims.ToArray();
            };
            options.ProblemDetailWriter = new DefaultProblemDetailWriter("ApiSecret");
        });

        builder.AddHeaderScheme<ISharedManagementService>("ApiSecret", options =>
        {
            options.HeaderName = "ApiSecret";
            options.ClaimsCreator = async (managementService, apiSecret) =>
            {
                var validationResult = await managementService.ValidateSecretKeyAsync(apiSecret);

                var claims = new List<Claim>
                {
                    new (CustomClaimTypes.AccountName, validationResult.ApplicationId),
                    new (CustomClaimTypes.KeyType, "secret")
                };

                foreach (var scope in validationResult.Scopes)
                {
                    claims.Add(new(CustomClaimTypes.Scopes, scope));
                }

                return claims.ToArray();
            };
            options.ProblemDetailWriter = new DefaultProblemDetailWriter("ApiKey");
        });

        builder.AddHeaderScheme<IOptions<ManagementOptions>>("ManagementKey", options =>
        {
            options.HeaderName = "ManagementKey";
            options.ClaimsCreator = (optionsAccessor, key) =>
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
                    new Claim(CustomClaimTypes.KeyType, "management"),
                });
            };
        });

        return builder;
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