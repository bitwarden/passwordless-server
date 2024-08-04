using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.Services;

public class PostSignInHandlerService : IPostSignInHandlerService
{
    private readonly ConsoleDbContext _db;
    private readonly ILogger<PostSignInHandlerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PostSignInHandlerService(
        ConsoleDbContext db,
        IServiceProvider serviceProvider,
        ILogger<PostSignInHandlerService> logger)
    {
        _db = db;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task HandleAsync(int organizationId)
    {
        var applications = await _db.Applications
            .Where(x => x.OrganizationId == organizationId && !x.DeleteAt.HasValue)
            .ToListAsync();
        foreach (var application in applications)
        {
            var context = _serviceProvider.GetRequiredService<ICurrentContext>();
            context.SetApp(application);
            var passwordlessClient = _serviceProvider.GetRequiredService<IScopedPasswordlessClient>();
            var usersCountResult = await passwordlessClient.GetUsersCountAsync();
            application.CurrentUserCount = usersCountResult.Count;
            _logger.LogInformation("Updating user count for application {ApplicationId} to {UserCount}", application.Id, usersCountResult.Count);
        }

        if (_db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync();
        }
    }
}