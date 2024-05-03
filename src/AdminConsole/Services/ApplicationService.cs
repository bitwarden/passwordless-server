using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services;

public class ApplicationService : IApplicationService
{
    private readonly IPasswordlessManagementClient _client;
    private readonly ConsoleDbContext _db;
    private readonly ISharedBillingService _billingService;
    private readonly IMailService _mailService;
    private readonly IMagicLinkBuilder _magicLinkBuilder;

    public ApplicationService(
        IPasswordlessManagementClient client,
        ConsoleDbContext db,
        IMailService mailService,
        IMagicLinkBuilder magicLinkBuilder,
        ISharedBillingService billingService)
    {
        _client = client;
        _db = db;
        _billingService = billingService;
        _mailService = mailService;
        _magicLinkBuilder = magicLinkBuilder;
    }

    public async Task<bool> CanDeleteApplicationImmediatelyAsync(string applicationId) =>
        await _client.CanDeleteApplicationImmediatelyAsync(applicationId);

    public async Task<MarkDeleteApplicationResponse> MarkDeleteApplicationAsync(string applicationId, string userName)
    {
        var response = await _client.MarkDeleteApplicationAsync(new MarkDeleteApplicationRequest(applicationId, userName));

        var application = await _db.Applications.FirstOrDefaultAsync(x => x.Id == applicationId);

        if (application == null) return response;

        if (response.IsDeleted)
        {
            await DeleteAsync(applicationId);
            await _mailService.SendApplicationDeletedAsync(application, response.DeleteAt, userName, response.AdminEmails);
        }
        else
        {
            application.DeleteAt = response.DeleteAt;
            await _db.SaveChangesAsync();
            var magicLink = await _magicLinkBuilder.GetLinkAsync(userName, "/organization/overview");
            await _mailService.SendApplicationToBeDeletedAsync(application, userName, magicLink, response.AdminEmails);
        }

        return response;
    }

    public async Task CancelDeletionForApplicationAsync(string applicationId)
    {
        _ = await _client.CancelApplicationDeletionAsync(applicationId);

        var application = await _db.Applications
            .Where(x => x.Id == applicationId)
            .FirstOrDefaultAsync();

        if (application == null) return;

        application.DeleteAt = null;
        await _db.SaveChangesAsync();
    }

    public async Task<Onboarding?> GetOnboardingAsync(string applicationId)
    {
        return await _db.Onboardings.SingleOrDefaultAsync(x => x.ApplicationId == applicationId);
    }

    public async Task DeleteAsync(string applicationId)
    {
        var application = await _db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId);
        if (application == null) return;
        _db.Applications.Remove(application);
        var rows = await _db.SaveChangesAsync();
        if (rows > 0 && application.BillingSubscriptionItemId != null)
        {
            await _billingService.OnPostApplicationDeletedAsync(application.BillingSubscriptionItemId);
        }
    }

    public async Task CreateApplicationAsync(Application application)
    {
        _db.Applications.Add(application);
        await _db.SaveChangesAsync();
    }
}