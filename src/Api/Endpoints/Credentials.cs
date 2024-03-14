using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Api.OpenApi;
using Passwordless.Service;
using Passwordless.Service.Helpers;

namespace Passwordless.Api.Endpoints;

public static class CredentialsEndpoints
{
    public static void MapCredentialsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/credentials")
            .RequireCors("default")
            .RequireSecretKey()
            .WithTags(OpenApiTags.Credentials);

        group.MapPost("/delete", async (CredentialsDeleteDTO payload,
                UserCredentialsService userCredentialsService) =>
        {
            await userCredentialsService.DeleteCredentialAsync(payload.CredentialId);

            return Results.NoContent();
        });

        group.MapMethods("/list", new[] { "post", "get" }, async (HttpRequest req, UserCredentialsService userCredentialService) =>
        {
            string? userId;
            if (req.Method == "POST")
            {
                var payload = await req.ReadFromJsonAsync<CredentialsListDTO>();

                // if payload is empty, throw exception
                if (payload == null)
                {
                    throw new ApiException("Payload is empty", 400);
                }

                userId = payload.UserId;
            }
            else
            {
                userId = req.Query["userId"].SingleOrDefault();
            }

            // if payload is empty, throw exception
            if (userId == null)
            {
                throw new ApiException("Please supply UserId in the query string value", 400);
            }

            var result = await userCredentialService.GetAllCredentialsAsync(userId);

            var res = ListResponse.Create(result);

            return Results.Ok(res);
        });
    }
}