using Microsoft.Extensions.Options;
using Passwordless.Common.Configuration;

namespace Passwordless.AdminConsole.Services;

public class SetupService : ISetupService
{
    private readonly PasswordlessOptions _options;
    private readonly bool _isSelfHosted;

    public SetupService(
        IOptions<PasswordlessOptions> options,
        IConfiguration configuration)
    {
        _options = options.Value;
        _isSelfHosted = configuration.IsSelfHosted();
    }

    public Task<bool> HasSetupCompletedAsync()
    {
        // Self-host -> check if API key and secret have been replaced
        if (_isSelfHosted)
        {
            return Task.FromResult(
                !(_options.ApiKey!.Contains("replaceme") || _options.ApiSecret.Contains("replaceme"))
            );
        }
        // Local and cloud -> check if API Secret has been set
        else
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(_options.ApiSecret)
            );
        }
    }
}