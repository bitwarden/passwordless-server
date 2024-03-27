using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Models.Reporting;
using Passwordless.Service;
using Passwordless.Service.Models;
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

    /// <summary>
    /// Returns a list of reports, every report mentions how many credentials and users are registered for a given day.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="reportingService"></param>
    /// <returns></returns>
    [ProducesResponseType(typeof(PeriodicCredentialReportResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> GetPeriodicCredentialReportsAsync(
        [AsParameters] PeriodicCredentialReportRequest request,
        IReportingService reportingService)
    {
        var result = await reportingService.GetPeriodicCredentialReportsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Returns a list of active user reports, every report mentions how many users were active in the last day and week
    /// for a given day.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="reportingService"></param>
    /// <returns></returns>
    [ProducesResponseType(typeof(IEnumerable<PeriodicActiveUserReportResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> GetPeriodicActiveUserReportsAsync(
        [AsParameters] PeriodicActiveUserReportRequest request,
        IReportingService reportingService)
    {
        var result = await reportingService.GetPeriodicActiveUserReportsAsync(request);
        return Ok(result);
    }
}