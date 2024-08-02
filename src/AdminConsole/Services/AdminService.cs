using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;

namespace Passwordless.AdminConsole.Services;

public class AdminService : IAdminService
{
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly ConsoleDbContext _db;
    private readonly ILogger<AdminService> _logger;
    private readonly int _organizationId;

    public AdminService(
        IPasswordlessClient passwordlessClient,
        ConsoleDbContext db,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AdminService> logger)
    {
        _passwordlessClient = passwordlessClient;
        _db = db;
        _logger = logger;
        _organizationId = httpContextAccessor.HttpContext!.User.GetOrgId()!.Value;
    }

    public async Task<bool> CanDisableMagicLinksAsync()
    {
        IReadOnlyCollection<string> adminIds = await _db.Users
            .Where(x => x.OrganizationId == _organizationId)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var adminId in adminIds)
        {
            try
            {
                var credentials = await _passwordlessClient.ListCredentialsAsync(adminId);
                if (credentials.Count == 0)
                {
                    return false;
                }
            }
            catch (PasswordlessApiException e)
            {
                _logger.LogError(e, "Failed to list credentials for user {UserId}", adminId);
                throw new InvalidOperationException("Failed to list credentials for user.");
            }
        }

        return true;
    }
}