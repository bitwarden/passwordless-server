namespace Passwordless.Api.Authorization;

public static class Constants
{
    public const string Scheme = "ApiKey";
    public const string PublicKeyPolicy = "PublicKey";
    public const string SecretKeyPolicy = "SecretKey";
    public const string ManagementKeyPolicy = "ManagementKey";
}

public static class CustomClaimTypes
{
    public const string AccountName = "accountName";
    public const string KeyType = "keyType";
    public const string Scopes = "scopes";
}