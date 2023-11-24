using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public class AuthenticatorDataUpdaterBackgroundService : BackgroundService
{
    private readonly IAuthenticatorDataService _service;
    private readonly ILogger<AuthenticatorDataUpdaterBackgroundService> _logger;

    public AuthenticatorDataUpdaterBackgroundService(
        IAuthenticatorDataService service,
        ILogger<AuthenticatorDataUpdaterBackgroundService> logger)
    {
        _service = service;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Services", "AuthenticatorData", "authenticator-data.json");
        var json = await File.ReadAllTextAsync(path, stoppingToken);
        JsonTypeInfo<AuthenticatorDataDto> context = AuthenticatorDataJsonSerializerContext.Default.AuthenticatorDataDto;
        var authenticatorData = JsonSerializer.Deserialize(json, context);

        foreach (var authenticator in authenticatorData)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await _service.AddOrUpdateAuthenticatorDataAsync(authenticator.Key, authenticator.Value.Name,
                    authenticator.Value.IconLight);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating authenticator data");
                return;
            }
        }
    }
}