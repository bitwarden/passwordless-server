using Passwordless.Api.Authorization;
using Passwordless.Common.Models.MDS;
using Passwordless.Service.MDS;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class MetaDataServiceEndpoints
{
    public static void MapMetaDataServiceEndpoints(this WebApplication app)
    {
        var parent = app
            .MapGroup("/mds")
            .RequireManagementKey()
            .RequireCors("default");

        // internal endpoints do not document, pending scopes to be added to be used by admin console only.
        parent.MapGet(
            "/attestation-types",
            async (IMetaDataService mds) =>
            {
                var result = await mds.GetAttestationTypesAsync();
                return Ok(result);
            });

        // internal endpoints do not document, pending scopes to be added to be used by admin console only.
        parent.MapGet(
            "/certification-statuses",
            async (IMetaDataService mds) =>
            {
                var result = await mds.GetCertificationStatusesAsync();
                return Ok(result);
            });

        // internal endpoints do not document, pending scopes to be added to be used by admin console only.
        parent.MapGet(
            "/entries",
            async (
                [AsParameters] EntriesRequest request,
                IMetaDataService mds) =>
            {
                var result = await mds.GetEntriesAsync(request);
                return Ok(result);
            }).WithParameterValidation();
    }
}