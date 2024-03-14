namespace Passwordless.Api.Authorization;

public static class Constants
{
    public const string PublicKeyAuthenticationScheme = "ApiKey";
    public const string SecretKeyAuthenticationScheme = "ApiSecret";
    public const string ManagementKeyAuthenticationScheme = "ManagementKey";

    public const string PublicKeyType = "public";
    public const string SecretKeyType = "secret";
    public const string ManagementKeyType = "management";

    public const string PublicKeyHeaderName = "ApiKey";
    public const string SecretKeyHeaderName = "ApiSecret";
    public const string ManagementKeyHeaderName = "ManagementKey";
}

public static class CustomClaimTypes
{
    public const string AccountName = "accountName";
    public const string KeyType = "keyType";
    public const string Scopes = "scopes";
}