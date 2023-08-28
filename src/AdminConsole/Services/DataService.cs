using AdminConsole.Db;
using AdminConsole.Helpers;
using AdminConsole.Identity;
using AdminConsole.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminConsole.Services;

public class DataService
{
    private readonly IHttpContextAccessor _httpAccessor;
    private readonly ConsoleDbContext _db;
    private readonly int _orgId;

    public DataService(IHttpContextAccessor httpAccessor, ConsoleDbContext db)
    {
        _httpAccessor = httpAccessor;
        _db = db;
        _orgId = httpAccessor.HttpContext.User.GetOrgId();
    }

    public async Task<List<Application>> GetApplications()
    {
        return await _db.Applications
            .Where(a => a.OrganizationId == _orgId).ToListAsync();
    }

    public async Task<Organization> GetOrganization()
    {
        return await _db.Organizations
            .Where(o => o.Id == _orgId).FirstOrDefaultAsync();
    }

    public async Task<bool> AllowedToCreateApplication()
    {
        var org = await _db.Organizations.Where(o => o.Id == _orgId)
            .Include(o => o.Applications).FirstOrDefaultAsync();

        return org.Applications.Count < org.MaxApplications;
    }

    public async Task<bool> CanInviteAdmin()
    {
        var org = await _db.Organizations.Where(o => o.Id == _orgId)
            .Include(o => o.Admins).FirstOrDefaultAsync();

        return org.Admins.Count() < org.MaxAdmins;
    }

    public async Task<Organization> GetOrganizationWithData()
    {
        var orgData = await _db.Organizations
            .Where(o => o.Id == _orgId)
            .Include(o => o.Applications)
            .Include(o => o.Admins)
            .FirstOrDefaultAsync();

        return orgData;
    }

    public async Task<List<ConsoleAdmin>> GetConsoleAdmins()
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

    public async Task<bool> DeleteOrganizationAsync(int organizationId)
    {
        var organization = await _db.Organizations.FirstOrDefaultAsync(x => x.Id == organizationId);
        if (organization == null) return false;
        _db.Organizations.Remove(organization);
        var rowsAffected = await _db.SaveChangesAsync();
        return rowsAffected > 0;
    }
}