using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Services;

public class ApplicationService<TDbContext> : IApplicationService where TDbContext : ConsoleDbContext
{
    private readonly IPasswordlessManagementClient _client;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly ISharedBillingService _billingService;
    private readonly IMailService _mailService;
    private readonly IMagicLinkBuilder _magicLinkBuilder;

    public ApplicationService(
        IPasswordlessManagementClient client,
        IDbContextFactory<TDbContext> dbContextFactory,
        IMailService mailService,
        IMagicLinkBuilder magicLinkBuilder,
        ISharedBillingService billingService)
    {
        _client = client;
        _dbContextFactory = dbContextFactory;
        _billingService = billingService;
        _mailService = mailService;
        _magicLinkBuilder = magicLinkBuilder;
    }

    public async Task<MarkDeleteApplicationResponse> MarkApplicationForDeletionAsync(string applicationId, string userName)
    {
        var response = await _client.MarkDeleteApplication(new MarkDeleteApplicationRequest(applicationId, userName));

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var application = await db.Applications.FirstOrDefaultAsync(x => x.Id == applicationId);

        if (application == null) return response;

        if (response.IsDeleted)
        {
            await DeleteAsync(applicationId);
            db.Applications.Entry(application).State = EntityState.Deleted;

            await _mailService.SendApplicationDeletedAsync(application, response.DeleteAt, userName, response.AdminEmails);
        }
        else
        {
            application.DeleteAt = response.DeleteAt;
            await db.SaveChangesAsync();
            var magicLink = await _magicLinkBuilder.GetLinkAsync(userName, "/organization/overview");
            await _mailService.SendApplicationToBeDeletedAsync(application, userName, magicLink, response.AdminEmails);
        }
        
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
        var application = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId);
        if (application == null) return;
        db.Applications.Remove(application);
        var rows = await db.SaveChangesAsync();
        if (rows > 0 && application.BillingSubscriptionItemId != null)
        {
            await _billingService.OnPostApplicationDeletedAsync(application.BillingSubscriptionItemId);
        }
    }

    public async Task CreateApplicationAsync(Application application)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Applications.Add(application);
        await db.SaveChangesAsync();
    }
}