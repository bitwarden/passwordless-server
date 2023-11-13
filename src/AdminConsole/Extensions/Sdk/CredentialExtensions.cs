namespace Passwordless.AdminConsole.Extensions.Sdk;

public static class CredentialExtensions
{
    public static bool IsNew(this Credential credential)
    {
        return credential.CreatedAt > DateTime.UtcNow.AddMinutes(-1);
    }
}