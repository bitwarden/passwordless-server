using Fido2NetLib;

namespace Passwordless.Service.Models;

public class RegisterSession
{
    public CredentialCreateOptions Options { get; set; }
}