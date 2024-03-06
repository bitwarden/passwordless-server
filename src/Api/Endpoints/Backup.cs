using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Common.Backup;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Storage.Ef;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class BackupEndpoints
{
    public static void MapBackupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/backup")
            .RequireCors("default");

        group.MapGet("/export", CreateBackupAsync);
    }

    public static async Task<IResult> CreateBackupAsync(
        IBackupSerializer service,
        DbGlobalContext dbGlobalContext)
    {
        var features = dbGlobalContext.AppFeatures.ToImmutableList();
        var result = service.Serialize(features);
        return Ok(result);
    }
}