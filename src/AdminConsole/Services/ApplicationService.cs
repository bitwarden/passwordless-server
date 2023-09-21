using AdminConsole.Db;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Models.DTOs;

namespace Passwordless.AdminConsole.Services;

public class ApplicationService
{
    private readonly IPasswordlessManagementClient _client;
    private readonly ConsoleDbContext _db;

    public ApplicationService(IPasswordlessManagementClient client, ConsoleDbContext db)
    {
        _client = client;
        _db = db;
    }

    public async Task<MarkDeleteApplicationResponse> MarkApplicationForDeletion(string applicationId, string userName)
    {
        var response = await _client.MarkDeleteApplication(new MarkDeleteApplicationRequest(applicationId, userName));

        var application = await _db.Applications.FirstOrDefaultAsync(x => x.Id == applicationId);

        if (application == null) return response;

        if (response.IsDeleted)
        {
            _db.Applications.Entry(application).State = EntityState.Deleted;
        }
        else
        {
            application.DeleteAt = response.DeleteAt;
        }

        await _db.SaveChangesAsync();

        return response;
    }

    public async Task CancelDeletionForApplication(string applicationId)
    {
        _ = await _client.CancelApplicationDeletion(applicationId);

        var application = await _db.Applications
            .Where(x => x.Id == applicationId)
            .FirstOrDefaultAsync();

        if (application == null) return;

        application.DeleteAt = null;
        await _db.SaveChangesAsync();
    }
}