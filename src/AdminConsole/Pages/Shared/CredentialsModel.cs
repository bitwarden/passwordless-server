using Passwordless.AdminConsole.Services.AuthenticatorData;
using Passwordless.Common.Extensions;

namespace Passwordless.AdminConsole.Pages.Shared;

public sealed class CredentialsModel
{
    private readonly IAuthenticatorDataService _authenticatorDataService;

    public CredentialsModel(IAuthenticatorDataService authenticatorDataService)
    {
        _authenticatorDataService = authenticatorDataService;
    }

    /// <summary>
    /// The list of credentials.
    /// </summary>
    public IReadOnlyCollection<CredentialModel> Items { get; private set; } = new List<CredentialModel>();

    public async Task SetItemsAsync(IReadOnlyCollection<Credential> items)
    {
        var aaGuids = items.Select(x => x.AaGuid).ToList();
        var authenticators = await _authenticatorDataService.GetAsync(aaGuids);
        Items = items
            .Select(x =>
            {
                var viewModel = new CredentialModel(x.Descriptor.Id, x.PublicKey, x.SignatureCounter,
                    x.AttestationFmt, x.CreatedAt, x.AaGuid, x.LastUsedAt, x.RPID, x.Origin, x.Device, x.Nickname);
                var authenticator = authenticators.SingleOrDefault(a => a.AaGuid == x.AaGuid);

                if (authenticator != null)
                {
                    viewModel.AuthenticatorName = authenticator.Name;
                }

                return viewModel;
            }).ToList();
    }

    /// <summary>
    /// Determines whether the details of the credentials should be hidden.
    /// </summary>
    public bool HideDetails { get; set; }

    /// <summary>
    /// Credential view model
    /// </summary>
    public record CredentialModel
    {
        public string DescriptorId { get; }

        public byte[] PublicKey { get; }

        public uint SignatureCounter { get; }

        public string AttestationFmt { get; }

        public DateTime CreatedAt { get; }

        public Guid AaGuid { get; }

        public DateTime LastUsedAt { get; }

        public string RPID { get; }

        public string Origin { get; }

        public string Device { get; }

        public string Nickname { get; }

        public bool? BackupState { get; set; }

        public bool? IsBackupEligible { get; set; }

        public bool? IsDiscoverable { get; set; }

        public string AuthenticatorName { get; set; }

        public bool IsNew()
        {
            return CreatedAt > DateTime.UtcNow.AddMinutes(-1);
        }

        public bool IsAuthenticatorKnown => AaGuid != Guid.Empty;

        public CredentialModel(byte[] descriptorId, byte[] publicKey, uint signatureCounter, string attestationFmt, DateTime createdAt, Guid aaGuid, DateTime lastUsedAt, string rpid, string origin, string device, string nickname)
        {
            DescriptorId = descriptorId.ToBase64Url();
            PublicKey = publicKey;
            SignatureCounter = signatureCounter;
            AttestationFmt = attestationFmt;
            CreatedAt = createdAt;
            AaGuid = aaGuid;
            LastUsedAt = lastUsedAt;
            RPID = rpid;
            Origin = origin;
            Device = device;
            Nickname = nickname;
        }
    }
}