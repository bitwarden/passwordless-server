using Passwordless.Common.Validation;

namespace Passwordless.Common.Models.MDS;

public class EntriesRequest
{
    [RegularExpressionCollection("^[a-zA-Z0-9_]*$")]
    public string[]? AttestationTypes { get; set; }

    [RegularExpressionCollection("^[a-zA-Z0-9_]*$")]
    public string[]? CertificationStatuses { get; set; }
}