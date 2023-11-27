using System.Text.Json.Serialization;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

[JsonSerializable(typeof(AuthenticatorDataDto))]
[JsonSerializable(typeof(AuthenticatorDataItemDto))]
internal partial class AuthenticatorDataJsonSerializerContext : JsonSerializerContext
{
}