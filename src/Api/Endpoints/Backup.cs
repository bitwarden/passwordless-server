using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Common.Models.Backup;
using Passwordless.Service.Backup;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class BackupEndpoints
{
    public static void MapBackupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/backup")
            .RequireSecretKey()
            .RequireCors("default");

        group.MapPost("/schedule", ScheduleAsync);

        group.MapGet("/jobs", GetJobsAsync);
    }

    /// <summary>
    /// Schedules a new backup to be created.
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static async Task<IResult> ScheduleAsync([FromServices] IBackupService service)
    {
        var id = await service.ScheduleAsync();
        return Ok(new ScheduleBackupResponse(id));
    }

    /// <summary>
    /// Retrieves all the background jobs.
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static async Task<IResult> GetJobsAsync([FromServices] IBackupService service)
    {
        var result = await service.GetJobsAsync();
        return Ok(result);
    }
}