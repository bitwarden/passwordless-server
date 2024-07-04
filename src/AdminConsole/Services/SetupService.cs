using Passwordless.Common.Configuration;

namespace Passwordless.AdminConsole.Services;

public class SetupService : ISetupService
{
    private readonly PasswordlessOptions? _options;
    private readonly bool _isSelfHosted;

    public SetupService(
        IConfiguration configuration)
    {
        // Get the Passwordless options manually to skip the built-in validations in the Passwordless SDK
        _options = configuration.GetSection("Passwordless").Get<PasswordlessOptions>();
        _isSelfHosted = configuration.IsSelfHosted();
    }

    public Task<bool> HasSetupCompletedAsync()
    {
        // Self-host -> check if API key and secret have been replaced
        if (_isSelfHosted)
        {
            return Task.FromResult(
                !(_options?.ApiKey?.Contains("replaceme") == true ||
                  _options?.ApiSecret.Contains("replaceme") == true)
            );
        }
        // Local and cloud -> check if API Secret has been set
        else
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(_options?.ApiSecret)
            );
        }
    }
}