using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Db;
using Passwordless.Common.Configuration;
using Passwordless.Common.Extensions;

namespace Passwordless.AdminConsole.Services;

public class SetupService : ISetupService
{
    private readonly PasswordlessOptions _options;
    private readonly bool _isSelfHosted;
    private readonly ConsoleDbContext _dbContext;

    public SetupService(
        IOptions<PasswordlessOptions> options,
        IConfiguration configuration,
        ConsoleDbContext dbContext)
    {
        _options = options.Value;
        _isSelfHosted = configuration.IsSelfHosted();
        _dbContext = dbContext;
    }

    public async Task<bool> HasSetupCompletedAsync()
    {
        if (_isSelfHosted)
        {
            return !(_options.ApiKey!.Contains("replaceme") || _options.ApiSecret.Contains("replaceme"));
        }
        else
        {
            return await _dbContext.HasAppliedMigrationsAsync();
        }
    }
}