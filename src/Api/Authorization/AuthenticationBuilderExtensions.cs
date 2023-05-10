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
                var accountName = await managementService.ValidatePublicKey(apiKey);

                return new[]
                {
                    new Claim(CustomClaimTypes.AccountName, accountName),
                    new Claim(CustomClaimTypes.KeyType, "public"),
                };
            };
            options.ProblemDetailWriter = new DefaultProblemDetailWriter("ApiSecret");
        });

        builder.AddHeaderScheme<ISharedManagementService>("ApiSecret", options =>
        {
            options.HeaderName = "ApiSecret";
            options.ClaimsCreator = async (managementService, apiSecret) =>
            {
                var accountName = await managementService.ValidateSecretKey(apiSecret);

                return new[]
                {
                    new Claim(CustomClaimTypes.AccountName, accountName),
                    new Claim(CustomClaimTypes.KeyType,  "secret"),
                };
            };
            options.ProblemDetailWriter = new DefaultProblemDetailWriter("ApiKey");
        });

        builder.AddHeaderScheme<IOptions<MangementOptions>>("ManagementKey", options =>
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