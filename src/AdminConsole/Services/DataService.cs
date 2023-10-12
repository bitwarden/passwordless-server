using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public class DataService<TDbContext> : IDataService where TDbContext : ConsoleDbContext
{
    private readonly IHttpContextAccessor _httpAccessor;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly int? _orgId;

    public DataService(IHttpContextAccessor httpAccessor, IDbContextFactory<TDbContext> dbContextFactory)
    {
        _httpAccessor = httpAccessor;
        _dbContextFactory = dbContextFactory;
        _orgId = httpAccessor.HttpContext.User?.GetOrgId();
    }

    public async Task<List<Application>> GetApplicationsAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Applications
            .Where(a => a.OrganizationId == _orgId).ToListAsync();
    }

    public async Task<Organization> GetOrganizationAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Organizations
            .Where(o => o.Id == _orgId).FirstOrDefaultAsync();
    }

    public async Task<bool> AllowedToCreateApplicationAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var org = await db.Organizations.Where(o => o.Id == _orgId)
            .Include(o => o.Applications).FirstOrDefaultAsync();

        return org.Applications.Count < org.MaxApplications;
    }

    public async Task<bool> CanInviteAdminAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var org = await db.Organizations.Where(o => o.Id == _orgId)
            .Include(o => o.Admins).FirstOrDefaultAsync();

        return org.Admins.Count() < org.MaxAdmins;
    }

    public async Task<Organization> GetOrganizationWithDataAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var orgData = await db.Organizations
            .Where(o => o.Id == _orgId)
            .Include(o => o.Applications)
            .Include(o => o.Admins)
            .FirstOrDefaultAsync();

        return orgData;
    }

    public async Task<List<ConsoleAdmin>> GetConsoleAdminsAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var admins = await db.Users
            .Where(a => a.OrganizationId == _orgId)
            .ToListAsync();

        return admins;
    }

    public async Task<ConsoleAdmin> GetUserAsync()
    {
        var currentUserEmail = _httpAccessor.HttpContext.User.GetEmail().ToUpper();
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var user = await db.Users
            .Where(u => u.NormalizedEmail == currentUserEmail).FirstOrDefaultAsync();

        return user;
    }

    public async Task<bool> DeleteOrganizationAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organization = await db.Organizations.FirstOrDefaultAsync(x => x.Id == _orgId);
        if (organization == null) return false;
        db.Organizations.Remove(organization);
        var rowsAffected = await db.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task<Application?> GetApplicationAsync(string applicationId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var application = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId);
        return application;
    }

    public async Task<bool> CanConnectAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Database.CanConnectAsync();
    }

    public async Task CleanUpOnboardingAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Onboardings
            .Where(o => !string.IsNullOrEmpty(o.ApiSecret) && o.SensitiveInfoExpireAt < DateTime.UtcNow)
            .ToList().ForEach(o =>
            {
                o.ApiSecret = string.Empty;
            });
        await db.SaveChangesAsync();
    }
}