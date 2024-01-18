using Fido2NetLib;
using Passwordless.Api.Authorization;
using Passwordless.Service.MDS;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class MetaDataServiceEndpoints
{
    public static void MapMetaDataServiceEndpoints(this WebApplication app)
    {
        var parent = app
            .MapGroup("/mds")
            .RequireSecretKey()
            .RequireCors("default");

        parent.MapGet(
            "/attestation-types",
            async (IMetaDataService r) =>
            {
                var result = await r.GetAttestationTypesAsync();
                return Ok(result);
            });
        
        parent.MapGet(
            "/certification-statuses",
            async (IMetaDataService r) =>
            {
                var result = await r.GetCertificationStatusesAsync();
                return Ok(result);
            });
        
        parent.MapGet(
            "/entries",
            async (IMetaDataService r) =>
            {
                var result = await r.GetEntriesAsync();
                return Ok(result);
            });
    }
}