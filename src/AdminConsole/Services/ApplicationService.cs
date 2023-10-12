using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Services;

public class ApplicationService<TDbContext> : IApplicationService where TDbContext : ConsoleDbContext
{
    private readonly IPasswordlessManagementClient _client;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    public ApplicationService(IPasswordlessManagementClient client, IDbContextFactory<TDbContext> dbContextFactory)
    {
        _client = client;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<MarkDeleteApplicationResponse> MarkApplicationForDeletionAsync(string applicationId, string userName)
    {
        var response = await _client.MarkDeleteApplication(new MarkDeleteApplicationRequest(applicationId, userName));

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var application = await db.Applications.FirstOrDefaultAsync(x => x.Id == applicationId);

        if (application == null) return response;

        if (response.IsDeleted)
        {
            db.Applications.Entry(application).State = EntityState.Deleted;
        }
        else
        {
            application.DeleteAt = response.DeleteAt;
        }

        await db.SaveChangesAsync();

        return response;
    }

    public async Task CancelDeletionForApplicationAsync(string applicationId)
    {
        _ = await _client.CancelApplicationDeletion(applicationId);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var application = await db.Applications
            .Where(x => x.Id == applicationId)
            .FirstOrDefaultAsync();

        if (application == null) return;

        application.DeleteAt = null;
        await db.SaveChangesAsync();
    }

    public async Task<Onboarding?> GetOnboardingAsync(string applicationId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Onboardings.SingleOrDefaultAsync(x => x.ApplicationId == applicationId);
    }

    public async Task DeleteAsync(string applicationId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var application = new Application { Id = applicationId };
        db.Applications.Remove(application);
        await db.SaveChangesAsync();
    }
}