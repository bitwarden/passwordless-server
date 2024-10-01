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

    public string UserId
    {
        get => Encoding.UTF8.GetString(UserHandle);
        // This setter is required by EF, but the value should already be
        // set in UserHandle. Ideally, we'd remove this column and use a
        // computed column or query filter instead, but it is what it is.
        [Obsolete("This should only be used internally by EF.", true)]
        private set { }
    }

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
    public string? AuthenticatorDisplayName { get; set; }

    public bool? BackupState { get; set; }
    public bool? IsBackupEligible { get; set; }
    public bool? IsDiscoverable { get; set; }

    internal StoredCredential ToStoredCredential() => new()
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
        AuthenticatorDisplayName = AuthenticatorDisplayName,
        BackupState = BackupState,
        IsBackupEligible = IsBackupEligible,
        IsDiscoverable = IsDiscoverable
    };

    internal static EFStoredCredential FromStoredCredential(StoredCredential credential, string tenant) => new()
    {
        Tenant = tenant,
        PublicKey = credential.PublicKey,
        UserHandle = credential.UserHandle,
        SignatureCounter = credential.SignatureCounter,
        AttestationFmt = credential.AttestationFmt,
        CreatedAt = credential.CreatedAt,
        AaGuid = credential.AaGuid,
        LastUsedAt = credential.LastUsedAt,
        RPID = credential.RPID,
        Origin = credential.Origin,
        Country = credential.Country,
        Device = credential.Device,
        Nickname = credential.Nickname,
        AuthenticatorDisplayName = credential.AuthenticatorDisplayName,
        DescriptorId = credential.Descriptor.Id,
        DescriptorTransports = credential.Descriptor.Transports,
        DescriptorType = credential.Descriptor.Type,
        BackupState = credential.BackupState,
        IsBackupEligible = credential.IsBackupEligible,
        IsDiscoverable = credential.IsDiscoverable
    };
}