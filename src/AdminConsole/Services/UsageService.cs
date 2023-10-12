using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

internal class UsageService<TDbContext> : IUsageService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UsageService<TDbContext>> _logger;

    public UsageService(IDbContextFactory<TDbContext> dbContextFactory, IHttpClientFactory httpClientFactory, ILogger<UsageService<TDbContext>> logger)
    {
        _dbContextFactory = dbContextFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task UpdateUsersCount()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var apps = await db.Applications.ToListAsync();
        foreach (var app in apps)
        {
            try
            {
                var users = await GetUsersCounts(app);
                app.CurrentUserCount = users;
                // todo: Add a LastUpdated field
                _logger.LogInformation("Updated usage for app {appId} to {count}", app.Id, users);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to update usage for app {appId}: {error}", app.Id, e.Message);
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task<int> GetUsersCounts(Application app)
    {
        var api = PasswordlessClient.Create(new PasswordlessOptions
        {
            ApiSecret = app.ApiSecret,
            ApiUrl = app.ApiUrl
        }, _httpClientFactory);

        var countResponse = await api.GetUsersCountAsync();

        return countResponse.Count;
    }
}