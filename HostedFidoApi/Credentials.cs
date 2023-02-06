using ApiHelpers;
using Service;
using Service.Helpers;
using Service.Storage;

public static class CredentialsEndpoints
{
    public static void MapCredentialsEndpoints(this WebApplication app)
    {
        app.MapPost("/credentials/delete", async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                app.Logger.LogInformation("/credentials/delete called");
                var payload = await req.ReadFromJsonAsync<CredentialsDeleteDTO>(Json.Options);

                // if payload is empty, throw exception
                if (payload == null)
                {
                    throw new ApiException("Payload is empty", 400);
                }


                var accountname = await accountService.ValidateSecretApiKey(req.GetApiSecret());
                var service = new UserCredentialsService(accountname, app.Configuration, storage);
                await service.DeleteCredential(payload.CredentialId);

                app.Logger.LogInformation("event=credentials/delete account={0}", accountname);


                return Results.Ok();
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");


        app.MapMethods("/credentials/list", new[] { "post", "get" }, async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                app.Logger.LogInformation("/credentials/list called");

                string userId = "";
                if (req.Method == "POST")
                {
                    var payload = await req.ReadFromJsonAsync<CredentialsListDTO>(Json.Options);

                    // if payload is empty, throw exception
                    if (payload == null)
                    {
                        throw new ApiException("Payload is empty", 400);
                    }

                    userId = payload.UserId;
                }
                else
                {
                    userId = req.Query["userId"];
                }

                // if payload is empty, throw exception
                if (userId == null)
                {
                    throw new ApiException("Please supply UserId", 400);
                }


                var accountname = await accountService.ValidateSecretApiKey(req.GetApiSecret());
                var service = new UserCredentialsService(accountname, app.Configuration, storage);
                var result = await service.GetAllCredentials(userId);

                app.Logger.LogInformation("event=credentials/list account={0}", accountname);


                return Results.Json(result, Json.Options);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");
    }
}