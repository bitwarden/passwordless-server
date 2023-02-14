using System;
using System.Text;
using Fido2NetLib.Objects;
using Service.Models;

public class EFStoredCredential : PerTenant
{
    public byte[] DescriptorId { get; set; }
    public PublicKeyCredentialType? DescriptorType { get; set; }
    public AuthenticatorTransport[] DescriptorTransports { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; }
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

    internal StoredCredential ToStoredCredential()
    {
        return new StoredCredential()
        {
            Descriptor = new PublicKeyCredentialDescriptor() { Id = DescriptorId, Type = DescriptorType, Transports = DescriptorTransports },
            PublicKey = PublicKey,
            UserHandle = UserHandle,
            SignatureCounter = SignatureCounter,
            CredType = CredType,
            CreatedAt = CreatedAt,
            AaGuid = AaGuid,
            LastUsedAt = LastUsedAt,
            RPID = RPID,
            Origin = Origin,
            Country = Country,
            Device = Device,
            Nickname = Nickname,

        };
    }

    internal static EFStoredCredential FromStoredCredential(StoredCredential s)
    {
        return new EFStoredCredential()
        {
            PublicKey = s.PublicKey,
            UserHandle = s.UserHandle,
            SignatureCounter = s.SignatureCounter,
            CredType = s.CredType,
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
            DescriptorType = s.Descriptor.Type
        };
    }
}

// StoredCredentials
// Aliases
// AdminUsers + roles
// ApiKeys
// EpmeralKeys