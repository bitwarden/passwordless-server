namespace Passwordless.Common.Constants;

public static class ApiKeyScopes
{
    public const string PublicRegister = "register";
    public const string PublicLogin = "login";

    public const string SecretTokenRegister = "token_register";
    public const string SecretTokenVerify = "token_verify";

    public static IReadOnlySet<string> PublicScopes { get; } = new HashSet<string>
    {
        PublicRegister,
        PublicLogin
    };

    public static IReadOnlySet<string> SecretScopes { get; } = new HashSet<string>
    {
        SecretTokenRegister,
        SecretTokenVerify
    };
}