namespace Passwordless.Common.Constants;

public static class ApiKeyScopes
{
    public const string Register = "register";
    public const string Login = "login";
    public const string TokenRegister = "token_register";
    public const string TokenVerify = "token_verify";



    public static IReadOnlySet<string> PublicScopes { get; } = new HashSet<string>
    {
        Register,
        Login
    };

    public static IReadOnlySet<string> SecretScopes { get; } = new HashSet<string>
    {
        TokenRegister,
        TokenVerify
    };
}