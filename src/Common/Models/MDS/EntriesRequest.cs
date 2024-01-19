using Passwordless.Common.Validation;

namespace Passwordless.Common.Models.MDS;

public class EntriesRequest
{
    [RegularExpressionCollection("^[a-zA-Z0-9_]$")]
    public IEnumerable<string>? AttestationTypes { get; set; }

    [RegularExpressionCollection("^[a-zA-Z0-9_]$")]
    public IEnumerable<string>? CertificationStatuses { get; set; }
}