using Fido2NetLib;

namespace Passwordless.Service.Models;

public class RegisterSession
{
    public CredentialCreateOptions Options { get; set; }
    public HashSet<string>? Aliases { get; set; }
    public bool AliasHashing { get; set; } = true;
}