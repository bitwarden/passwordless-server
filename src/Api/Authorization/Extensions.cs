using Microsoft.AspNetCore.Authorization;

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

    public static RouteHandlerBuilder RequirePublicKey(this RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization(Constants.PublicKeyPolicy);
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
    }
}