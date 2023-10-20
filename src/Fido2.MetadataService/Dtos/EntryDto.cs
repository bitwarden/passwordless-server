using System.Text.Json.Serialization;

namespace Passwordless.Fido2.MetadataService.Dtos;

public class EntryDto
{
    [JsonPropertyName("aaguid")]
    public Guid AaGuid { get; set; }
    
    [JsonPropertyName("metadataStatement")]
    public StatementDto Statement { get; set; }

    [JsonPropertyName("statusReports")]
    public StatusReportsDto StatusReports { get; set; }
}