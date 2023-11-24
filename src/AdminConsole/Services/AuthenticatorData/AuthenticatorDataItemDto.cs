using System.Text.Json.Serialization;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public class AuthenticatorDataItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("icon_light")]
    public string IconLight { get; set; }
}