using Microsoft.Extensions.Options;
using Passwordless.AspNetCore;
using Passwordless.Common.Configuration;

namespace Passwordless.AdminConsole.Services;

public class SetupService(
    IOptionsSnapshot<PasswordlessAspNetCoreOptions> options,
    IConfiguration configuration)
    : ISetupService
{
    private readonly bool _isSelfHosted = configuration.IsSelfHosted();

    public Task<bool> HasSetupCompletedAsync()
    {
        // Self-host -> check if API key and secret have been replaced
        if (_isSelfHosted)
        {
            return Task.FromResult(
                !(options.Value.ApiKey?.Contains("replaceme") == true ||
                  options.Value.ApiSecret.Contains("replaceme"))
            );
        }
        // Local and cloud -> check if API Secret has been set
        else
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(options.Value.ApiSecret)
            );
        }
    }
}