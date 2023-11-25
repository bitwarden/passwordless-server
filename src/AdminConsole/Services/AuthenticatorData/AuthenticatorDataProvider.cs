using System.Collections.Immutable;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public class AuthenticatorDataProvider : IAuthenticatorDataProvider
{
    public AuthenticatorDataProvider()
    {

        var path = Path.Combine(Directory.GetCurrentDirectory(), "Services", "AuthenticatorData", "authenticator-data.json");
        var json = File.ReadAllText(path);
        JsonTypeInfo<AuthenticatorDataDto> context = AuthenticatorDataJsonSerializerContext.Default.AuthenticatorDataDto;
        var authenticatorData = JsonSerializer.Deserialize(json, context);
        if (authenticatorData == null)
        {
            throw new ConfigurationErrorsException("Unable to deserialize authenticator data.");
        }
        Authenticators = authenticatorData
            .Select(x => new Authenticator { AaGuid = x.Key, Name = x.Value.Name })
            .ToImmutableList();
    }

    public IReadOnlyCollection<Authenticator> Authenticators { get; }

    public string? GetName(Guid aaGuid)
    {
        return Authenticators.FirstOrDefault(x => x.AaGuid == aaGuid)?.Name;
    }
}