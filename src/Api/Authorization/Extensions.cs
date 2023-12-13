using Microsoft.AspNetCore.Authorization;
using Passwordless.Common.Constants;

namespace Passwordless.Api.Authorization;

public static class GeneralExtensions
{
    public static RouteHandlerBuilder RequireSecretKey(this RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization(Constants.SecretKeyPolicy);
    }

    public static RouteHandlerBuilder RequireManagementKey(this RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization(Constants.ManagementKeyPolicy);
    }

    public static RouteHandlerBuilder RequireAuthorization(this RouteHandlerBuilder builder, string scope)
    {
        return builder.RequireAuthorization(scope);
    }

    public static void AddPasswordlessPolicies(this AuthorizationOptions options)
    {
        // For any endpoint that takes the public key, the private key is also valid
        options.AddPolicy(Constants.PublicKeyPolicy, policy => policy
            .AddAuthenticationSchemes("ApiKey")
            .RequireAuthenticatedUser()
            .RequireClaim(CustomClaimTypes.AccountName)
            .RequireClaim(CustomClaimTypes.KeyType, "public"));

        // A secret key, requires only that secret key
        options.AddPolicy(Constants.SecretKeyPolicy, policy => policy
            .AddAuthenticationSchemes("ApiSecret")
            .RequireAuthenticatedUser()
            .RequireClaim(CustomClaimTypes.AccountName)
            .RequireClaim(CustomClaimTypes.KeyType, "secret"));

        options.AddPolicy(Constants.ManagementKeyPolicy, policy => policy
            .AddAuthenticationSchemes("ManagementKey")
            .RequireAuthenticatedUser()
            .RequireClaim(CustomClaimTypes.KeyType, "management"));

        foreach (var scope in ApiKeyScopes.PublicScopes)
        {
            options.AddPolicy(scope, policy => policy
                .AddAuthenticationSchemes("ApiKey")
                .RequireAuthenticatedUser()
                .RequireClaim(CustomClaimTypes.Scopes, scope));
        }

        foreach (var scope in ApiKeyScopes.SecretScopes)
        {
            options.AddPolicy(scope, policy => policy
                .AddAuthenticationSchemes("ApiSecret")
                .RequireAuthenticatedUser()
                .RequireClaim(CustomClaimTypes.Scopes, scope));
        }
    }
}