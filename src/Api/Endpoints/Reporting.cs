using Passwordless.Api.Authorization;
using Passwordless.Common.Models.Reporting;
using Passwordless.Service;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class ReportingEndpoints
{
    public static void MapReportingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/apps/{appId}/reporting")
            .RequireSecretKey()
            .RequireCors("default");

        group.MapGet("/credentials/periodic", GetPeriodicCredentialReportsAsync);
    }

    public static async Task<IResult> GetPeriodicCredentialReportsAsync(
        [AsParameters] PeriodicCredentialReportRequest request,
        IReportingService reportingService)
    {
        var result = await reportingService.GetPeriodicCredentialReportsAsync(request);
        return Ok(result);
    }
}