using Passwordless.Api.Authorization;
using Passwordless.Common.Backup;
using Passwordless.Service.Storage.Ef;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class BackupEndpoints
{
    public static void MapBackupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/backup")
            .RequireSecretKey()
            .RequireCors("default");

        group.MapGet("/export", CreateBackupAsync);
    }

    public static async Task<IResult> CreateBackupAsync(
        IBackupUtility service,
        DbGlobalContext dbGlobalContext)
    {
        await service.BackupAsync();
        return NoContent();
    }
}