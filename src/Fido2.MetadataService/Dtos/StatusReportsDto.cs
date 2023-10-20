using System.Text.Json.Serialization;

namespace Passwordless.Fido2.MetadataService.Dtos;

public class StatusReportsDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("effectiveDate")]
    public DateTime EffectiveDate { get; set; }
}