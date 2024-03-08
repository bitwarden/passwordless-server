using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public class DataService : IDataService
{
    private readonly IHttpContextAccessor _httpAccessor;
    private readonly ConsoleDbContext _db;
    private readonly int? _orgId;

    public DataService(IHttpContextAccessor httpAccessor, ConsoleDbContext db)
    {
        _httpAccessor = httpAccessor;
        _db = db;
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
        var organization = await _db.Organizations.FirstOrDefaultAsync(x => x.Id == _orgId);
        if (organization == null) return false;
        _db.Organizations.Remove(organization);
        var rowsAffected = await _db.SaveChangesAsync();
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
            .Where(o => !string.IsNullOrEmpty(o.ApiSecret) && o.SensitiveInfoExpireAt < DateTime.UtcNow)
            .ExecuteUpdateAsync(x => x
                .SetProperty(p => p.ApiSecret, string.Empty));
    }

    public async Task CreateOrganizationAsync(Organization organization)
    {
        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync();
    }
}