using System.Text;
using Fido2NetLib.Objects;

namespace Passwordless.Service.Models;

public class StoredCredential
{
    public PublicKeyCredentialDescriptor Descriptor { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
    public uint SignatureCounter { get; set; }
    public string AttestationFmt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid AaGuid { get; set; }
    public DateTime LastUsedAt { get; set; }
    public string RPID { get; set; }
    public string Origin { get; set; }
    public string Country { get; set; }
    public string Device { get; set; }
    public string Nickname { get; set; }
    public string UserId
    {
        get
        {
            return Encoding.UTF8.GetString(UserHandle);
        }
        private set { }
    }

    public bool? BackupState { get; set; }

    public bool? IsBackupEligible { get; set; }

    public bool? IsDiscoverable { get; set; }
}