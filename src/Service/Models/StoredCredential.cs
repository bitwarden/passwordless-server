using System.Text;
using Fido2NetLib.Objects;

namespace Passwordless.Service.Models;

public class StoredCredential
{
    public required PublicKeyCredentialDescriptor Descriptor { get; set; }
    public required byte[] PublicKey { get; set; }
    public required byte[] UserHandle { get; set; }
    public required uint SignatureCounter { get; set; }
    public required string AttestationFmt { get; set; }
    public required DateTime CreatedAt { get; set; }
    public Guid? AaGuid { get; set; }
    public DateTime LastUsedAt { get; set; }
    public required string RPID { get; set; }
    public required string Origin { get; set; }
    public string? Country { get; set; }
    public string? Device { get; set; }
    public string? Nickname { get; set; }

    public string UserId => Encoding.UTF8.GetString(UserHandle);

    public bool? BackupState { get; set; }

    public bool? IsBackupEligible { get; set; }

    public bool? IsDiscoverable { get; set; }
}