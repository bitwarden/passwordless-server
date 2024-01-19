namespace Passwordless.Common.Models.MDS;

public record EntryResponse(
    Guid AaGuid,
    string Name,
    IEnumerable<string> CertificationStatuses,
    IEnumerable<string> AttestationTypes);