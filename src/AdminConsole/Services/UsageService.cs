using AdminConsole.Db;
using AdminConsole.Models;
using Microsoft.EntityFrameworkCore;
using Passwordless.Net;

namespace AdminConsole.Services;

internal class UsageService
{
    private readonly ConsoleDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UsageService> _logger;

    public UsageService(ConsoleDbContext db, IHttpClientFactory httpClientFactory, ILogger<UsageService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task UpdateUsersCount()
    {

        // TODO: Improve by using a batch call or parallelism
        var apps = await _db.Applications.ToListAsync();
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

        await _db.SaveChangesAsync();

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