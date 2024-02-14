using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

internal class UsageService : IUsageService
{
    private readonly ConsoleDbContext _db;
    private readonly ILogger<UsageService> _logger;

    public UsageService(
        ConsoleDbContext db,
        ILogger<UsageService> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task UpdateUsersCountAsync()
    {
        var apps = await _db.Applications.ToListAsync();
        foreach (var app in apps)
        {
            if (app.DeleteAt.HasValue)
            {
                _logger.LogWarning("Skipped updating usage for app pending deletion {appId}.", app.Id);
                continue;
            }
            try
            {
                var users = await GetUsersCounts(app);
                app.CurrentUserCount = users;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to update usage for app {appId}: {error}", app.Id, e.Message);
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task<int> GetUsersCounts(Application app)
    {
        var api = new PasswordlessClient(new PasswordlessOptions
        {
            ApiSecret = app.ApiSecret,
            ApiUrl = app.ApiUrl
        });

        var countResponse = await api.GetUsersCountAsync();

        return countResponse.Count;
    }
}