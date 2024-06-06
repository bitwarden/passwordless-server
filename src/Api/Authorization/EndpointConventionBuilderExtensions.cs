using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Passwordless.Common.Constants;
using Passwordless.Common.Extensions;

namespace Passwordless.Api.Authorization;

public static class EndpointConventionBuilderExtensions
{
    /// <summary>
    /// Requires a `ManagementKey` header to access the endpoint.
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static TBuilder RequireManagementKey<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        var requirements = new List<IAuthorizationRequirement>
        {
            new ClaimsAuthorizationRequirement(CustomClaimTypes.KeyType, new []{ Constants.ManagementKeyType }),
        };

        var schemes = new List<string> { Constants.ManagementKeyAuthenticationScheme };
        var pol = new AuthorizationPolicy(requirements, schemes);
        return builder.RequireAuthorization(pol);
    }

    /// <summary>
    /// Requires a `ApiKey` header to access the endpoint.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="scope">When provided, requires the `ApiKey` to have the specified scope.</param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static TBuilder RequirePublicKey<TBuilder>(this TBuilder builder, PublicKeyScopes? scope = null) where TBuilder : IEndpointConventionBuilder
    {
        var requirements = new List<IAuthorizationRequirement>
        {
            new ClaimsAuthorizationRequirement(CustomClaimTypes.AccountName, null),
            new ClaimsAuthorizationRequirement(CustomClaimTypes.KeyType, new []{ Constants.PublicKeyType }),
        };

        if (scope.HasValue)
        {
            requirements.Add(new ClaimsAuthorizationRequirement(CustomClaimTypes.Scopes, new[] { scope.Value.GetDescription() }));
        }

        var schemes = new List<string> { Constants.PublicKeyAuthenticationScheme };
        var pol = new AuthorizationPolicy(requirements, schemes);
        return builder.RequireAuthorization(pol);
    }

    /// <summary>
    /// Requires a `ApiSecret` header to access the endpoint.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="scope">When provided, requires the `ApiSecret` to have the specified scope.</param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static TBuilder RequireSecretKey<TBuilder>(this TBuilder builder, SecretKeyScopes? scope = null) where TBuilder : IEndpointConventionBuilder
    {
        var requirements = new List<IAuthorizationRequirement>
        {
            new ClaimsAuthorizationRequirement(CustomClaimTypes.AccountName, null),
            new ClaimsAuthorizationRequirement(CustomClaimTypes.KeyType, new []{ Constants.SecretKeyType }),
        };

        if (scope.HasValue)
        {
            requirements.Add(new ClaimsAuthorizationRequirement(CustomClaimTypes.Scopes, new[] { scope.Value.GetDescription() }));
        }

        var schemes = new List<string> { Constants.SecretKeyAuthenticationScheme };
        var pol = new AuthorizationPolicy(requirements, schemes);
        return builder.RequireAuthorization(pol);
    }
}