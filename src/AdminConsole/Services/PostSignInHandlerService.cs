using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.Services;

public class PostSignInHandlerService : IPostSignInHandlerService
{
    private readonly ConsoleDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PostSignInHandlerService> _logger;

    public PostSignInHandlerService(
        ConsoleDbContext db,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        ILogger<PostSignInHandlerService> logger)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleAsync()
    {
        var user = _httpContextAccessor.HttpContext.User;
        var organizationIdValue = user.FindFirstValue(CustomClaimTypes.OrgId);
        if (organizationIdValue == null) return;
        var organizationId = int.Parse(organizationIdValue);
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