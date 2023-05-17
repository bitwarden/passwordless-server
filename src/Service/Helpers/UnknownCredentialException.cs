namespace Passwordless.Service.Helpers;

public class UnknownCredentialException : ApiException
{
    public UnknownCredentialException(string credentialId)
        : base("unknown_credential", "We don't recognize the passkey you sent us.", 400, new () { { "credentialId", credentialId }})
    {
    }
}