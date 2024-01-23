using Passwordless.Common.Validation;

namespace Passwordless.Common.Models.MDS;

public class EntriesRequest
{
    /// <summary>
    /// Filters the list of authenticators by the specified attestation types. When null or empty, all authenticators are returned.
    /// </summary>
    [RegularExpressionCollection("^[a-zA-Z0-9_]*$")]
    public string[]? AttestationTypes { get; set; }

    /// <summary>
    /// Filters the list of authenticators by the specified certification statuses. When null or empty, all authenticators are returned.
    /// </summary>
    [RegularExpressionCollection("^[a-zA-Z0-9_]*$")]
    public string[]? CertificationStatuses { get; set; }
}