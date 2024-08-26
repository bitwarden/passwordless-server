using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Helpers;
using Passwordless.Common.Extensions;
using Passwordless.Common.Validation;

namespace Passwordless.AdminConsole.Components.Shared;

public partial class Credentials : ComponentBase
{
    public const string RemoveCredentialFormName = "remove-credential-form";

    public required IReadOnlyCollection<Credential> Items { get; set; }

    public IReadOnlyCollection<CredentialModel> GetItems()
    {
        return Items.Select(x =>
        {
            var viewModel = new CredentialModel(
                x.Descriptor.Id,
                x.PublicKey,
                x.SignatureCounter,
                x.AttestationFmt,
                x.CreatedAt,
                x.AaGuid,
                x.LastUsedAt,
                x.RpId,
                x.Origin,
                x.Device,
                x.Nickname,
                x.BackupState,
                x.IsBackupEligible,
                x.IsDiscoverable,
                AuthenticatorDataProvider.GetName(x.AaGuid));
            return viewModel;
        }).ToList();
    }

    /// <summary>
    /// Determines whether the details of the credentials should be hidden.
    /// </summary>
    [Parameter]
    public bool HideDetails { get; set; }

    [Parameter]
    public required IPasswordlessClient PasswordlessClient { get; set; }

    [Parameter]
    public required string UserId { get; set; }

    [SupplyParameterFromForm(FormName = RemoveCredentialFormName)]
    public DeleteCredentialFormModel DeleteCredentialForm { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Items = await PasswordlessClient.ListCredentialsAsync(UserId);
    }

    public async Task DeleteCredentialAsync()
    {
        var validationContext = new ValidationContext(DeleteCredentialForm);
        var validationResult = Validator.TryValidateObject(DeleteCredentialForm, validationContext, null, true);
        if (!validationResult)
        {
            throw new ArgumentException("The request is not valid.");
        }
        await PasswordlessClient.DeleteCredentialAsync(DeleteCredentialForm.CredentialId);
        NavigationManager.NavigateTo(NavigationManager.Uri);

    }

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

        public bool? BackupState { get; }

        public bool? IsBackupEligible { get; }

        public bool? IsDiscoverable { get; }

        public string AuthenticatorName { get; set; }

        public bool IsNew()
        {
            return CreatedAt > DateTime.UtcNow.AddMinutes(-1);
        }

        /// <summary>
        /// The title of the credential card.
        /// </summary>
        public string Title
        {
            get
            {
                if (IsAuthenticatorKnown)
                {
                    return AuthenticatorName;
                }
                return string.IsNullOrEmpty(Device) ? "Passkey" : Device;
            }
        }

        private string? _subtitle;

        /// <summary>
        /// The sub title of the credential card.
        /// </summary>
        public string? SubTitle
        {
            get
            {
                if (_subtitle != null)
                {
                    return _subtitle;
                }

                if (IsAuthenticatorKnown)
                {
                    if (string.IsNullOrEmpty(Nickname))
                    {
                        _subtitle = Device;
                    }
                    else
                    {
                        var nickname = string.IsNullOrEmpty(Nickname) ? "No nickname" : Nickname;
                        _subtitle = $"{nickname} on {Device}";
                    }
                }
                else
                {
                    _subtitle = Nickname;
                }

                return _subtitle;
            }
        }

        public bool IsAuthenticatorKnown => AaGuid != Guid.Empty;

        public CredentialModel(
            byte[] descriptorId,
            byte[] publicKey,
            uint signatureCounter,
            string attestationFmt,
            DateTime createdAt,
            Guid aaGuid,
            DateTime lastUsedAt,
            string rpid,
            string origin,
            string device,
            string nickname,
            bool? backupState,
            bool? isBackupEligible,
            bool? isDiscoverable,
            string authenticatorName)
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
            BackupState = backupState;
            IsBackupEligible = isBackupEligible;
            IsDiscoverable = isDiscoverable;
            AuthenticatorName = authenticatorName;
        }
    }

    public sealed class DeleteCredentialFormModel
    {
        [Base64Url]
        public string CredentialId { get; set; }
    }
}