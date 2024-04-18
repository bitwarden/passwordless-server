using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.BackgroundServices;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public class DataService : IDataService
{
    private readonly IHttpContextAccessor _httpAccessor;
    private readonly ConsoleDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly int? _orgId;

    public DataService(IHttpContextAccessor httpAccessor, ConsoleDbContext db, TimeProvider timeProvider)
    {
        _httpAccessor = httpAccessor;
        _db = db;
        _timeProvider = timeProvider;
        _orgId = httpAccessor.HttpContext?.User?.GetOrgId();
    }

    public async Task<List<Application>> GetApplicationsAsync()
    {
        return await _db.Applications
            .Where(a => a.OrganizationId == _orgId).ToListAsync();
    }

    public async Task<Organization> GetOrganizationAsync()
    {
        return await _db.Organizations
            .Where(o => o.Id == _orgId).FirstOrDefaultAsync();
    }

    public async Task<bool> AllowedToCreateApplicationAsync()
    {
        var org = await _db.Organizations.Where(o => o.Id == _orgId)
            .Include(o => o.Applications).FirstOrDefaultAsync();

        return org.Applications.Count < org.MaxApplications;
    }

    public async Task<bool> CanInviteAdminAsync()
    {
        var org = await _db.Organizations.Where(o => o.Id == _orgId)
            .Include(o => o.Admins).FirstOrDefaultAsync();

        return org.Admins.Count() < org.MaxAdmins;
    }

    public async Task<Organization> GetOrganizationWithDataAsync()
    {
        var orgData = await _db.Organizations
            .Where(o => o.Id == _orgId)
            .Include(o => o.Applications)
            .Include(o => o.Admins)
            .FirstOrDefaultAsync();

        return orgData;
    }

    public async Task<List<ConsoleAdmin>> GetConsoleAdminsAsync()
    {
        var admins = await _db.Users
            .Where(a => a.OrganizationId == _orgId)
            .ToListAsync();

        return admins;
    }

    public async Task<ConsoleAdmin> GetUserAsync()
    {
        var currentUserEmail = _httpAccessor.HttpContext.User.GetEmail().ToUpper();
        var user = await _db.Users
            .Where(u => u.NormalizedEmail == currentUserEmail).FirstOrDefaultAsync();

        return user;
    }

    public async Task<bool> DeleteOrganizationAsync()
    {
        var rowsAffected = await _db.Organizations.Where(x => x.Id == _orgId).ExecuteDeleteAsync();
        return rowsAffected > 0;
    }

    public async Task<Application?> GetApplicationAsync(string applicationId)
    {
        var application = await _db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId);
        return application;
    }

    public async Task<bool> CanConnectAsync()
    {
        return await _db.Database.CanConnectAsync();
    }

    public async Task CleanUpOnboardingAsync()
    {
        await _db.Onboardings
            .Where(o => !string.IsNullOrEmpty(o.ApiSecret) && o.SensitiveInfoExpireAt < _timeProvider.GetUtcNow().UtcDateTime)
            .ExecuteUpdateAsync(x => x
                .SetProperty(p => p.ApiSecret, string.Empty));
    }

    public async Task CreateOrganizationAsync(Organization organization)
    {
        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync();
    }

    public async Task<UnconfirmedAccountCleanUpQueryResult> CleanUpUnconfirmedAccounts(CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = _timeProvider.GetUtcNow().Subtract(TimeSpan.FromDays(30));

        var organizationsToDelete = await _db.Organizations
            .Where(o => o.CreatedAt <= thirtyDaysAgo.UtcDateTime // orgs created 30 days ago
                        && !_db.Applications.Any(a => a.OrganizationId == o.Id) // no applications
                        && !_db.Users.Any(u => u.OrganizationId == o.Id && u.EmailConfirmed)) // only has unconfirmed users
            .ToListAsync(cancellationToken);

        var usersToDelete = await _db.Users
            .Where(u => organizationsToDelete.Any(o => o.Id == u.OrganizationId))
            .ToListAsync(cancellationToken);

        var result = new UnconfirmedAccountCleanUpQueryResult(organizationsToDelete.Count, usersToDelete.Count);

        _db.Organizations.RemoveRange(organizationsToDelete);
        _db.Users.RemoveRange(usersToDelete);

        await _db.SaveChangesAsync(cancellationToken);

        return result;
    }
}