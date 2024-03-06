namespace Passwordless.Common.Models.MDS;

/// <param name="AaGuid">Authenticator Attestation GUID</param>
/// <param name="Name">Authenticator name</param>
/// <param name="CertificationStatuses">Certification statuses</param>
/// <param name="AttestationTypes">Supported attestation types</param>
/// <param name="Icon">Icon</param>
public record EntryResponse(
    Guid AaGuid,
    string Name,
    IEnumerable<string> CertificationStatuses,
    IEnumerable<string> AttestationTypes,
    string Icon);