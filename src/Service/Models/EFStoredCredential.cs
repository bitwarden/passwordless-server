using System.Text;
using Fido2NetLib.Objects;

namespace Passwordless.Service.Models;

public class EFStoredCredential : PerTenant
{
    public required byte[] DescriptorId { get; set; }
    public PublicKeyCredentialType? DescriptorType { get; set; }
    public AuthenticatorTransport[]? DescriptorTransports { get; set; }
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
    public string UserId
    {
        get => Encoding.UTF8.GetString(UserHandle);
        // This setter is required by EF, but the value should already be
        // set in UserHandle. Ideally, we'd remove this column and use a
        // computed column or query filter instead, but it is what it is.
        [Obsolete]
        private set { }
    }

    public bool? BackupState { get; set; }

    public bool? IsBackupEligible { get; set; }

    public bool? IsDiscoverable { get; set; }

    internal StoredCredential ToStoredCredential()
    {
        return new StoredCredential
        {
            Descriptor = new PublicKeyCredentialDescriptor(DescriptorType.Value, DescriptorId, DescriptorTransports),
            PublicKey = PublicKey,
            UserHandle = UserHandle,
            SignatureCounter = SignatureCounter,
            AttestationFmt = AttestationFmt,
            CreatedAt = CreatedAt,
            AaGuid = AaGuid,
            LastUsedAt = LastUsedAt,
            RPID = RPID,
            Origin = Origin,
            Country = Country,
            Device = Device,
            Nickname = Nickname,
            BackupState = BackupState,
            IsBackupEligible = IsBackupEligible,
            IsDiscoverable = IsDiscoverable
        };
    }

    internal static EFStoredCredential FromStoredCredential(StoredCredential s, string tenant)
    {
        return new EFStoredCredential
        {
            Tenant = tenant,
            PublicKey = s.PublicKey,
            UserHandle = s.UserHandle,
            SignatureCounter = s.SignatureCounter,
            AttestationFmt = s.AttestationFmt,
            CreatedAt = s.CreatedAt,
            AaGuid = s.AaGuid,
            LastUsedAt = s.LastUsedAt,
            RPID = s.RPID,
            Origin = s.Origin,
            Country = s.Country,
            Device = s.Device,
            Nickname = s.Nickname,
            DescriptorId = s.Descriptor.Id,
            DescriptorTransports = s.Descriptor.Transports,
            DescriptorType = s.Descriptor.Type,
            BackupState = s.BackupState,
            IsBackupEligible = s.IsBackupEligible,
            IsDiscoverable = s.IsDiscoverable
        };
    }
}