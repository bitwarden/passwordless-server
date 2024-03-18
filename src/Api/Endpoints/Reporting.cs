using Microsoft.OpenApi.Models;
using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Models.Reporting;
using Passwordless.Service;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class ReportingEndpoints
{
    public static void MapReportingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/reporting")
            .RequireSecretKey()
            .RequireCors("default")
            .WithTags(OpenApiTags.Reporting);

        group.MapGet("/credentials/periodic", GetPeriodicCredentialReportsAsync);

        group.MapGet("/active-users/periodic", GetPeriodicActiveUserReportsAsync);
    }

    public static async Task<IResult> GetPeriodicCredentialReportsAsync(
        [AsParameters] PeriodicCredentialReportRequest request,
        IReportingService reportingService)
    {
        var result = await reportingService.GetPeriodicCredentialReportsAsync(request);
        return Ok(result);
    }

    public static async Task<IResult> GetPeriodicActiveUserReportsAsync(
        [AsParameters] PeriodicActiveUserReportRequest request,
        IReportingService reportingService)
    {
        var result = await reportingService.GetPeriodicActiveUserReportsAsync(request);
        return Ok(result);
    }
}