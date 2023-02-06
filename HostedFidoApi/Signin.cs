using ApiHelpers;
using Service;
using Service.Helpers;
using Service.Models;
using Service.Storage;
using System.Diagnostics;

public static class SigninEndpoints
{
    public static void MapSigninEndpoints(this WebApplication app)
    {

        app.MapPost("/signin/begin", async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                app.Logger.LogInformation("/signin/begin called");
                var sw = Stopwatch.StartNew();
                var payload = await req.ReadFromJsonAsync<SignInBeginDTO>(Json.Options);


                var accountname = await accountService.ValidatePublicApiKey(req.GetPublicApiKey());
                var service = await Fido2ServiceEndpoints.Create(accountname, app.Logger, app.Configuration, storage);

                var result = await service.SignInBegin(payload);
                sw.Stop();
                app.Logger.LogInformation("event=signin/begin account={account} duration={duration}", accountname, sw.ElapsedMilliseconds);

                return Results.Json(result, Json.Options);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default").WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/complete", async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                app.Logger.LogInformation("/signin/complete called");
                var sw = Stopwatch.StartNew();
                var payload = await req.ReadFromJsonAsync<SignInCompleteDTO>(Json.Options);


                var accountname = await accountService.ValidatePublicApiKey(req.GetPublicApiKey());
                var service = await Fido2ServiceEndpoints.Create(accountname, app.Logger, app.Configuration, storage);

                var (deviceInfo, country) = Helpers.GetDeviceInfo(req);
                var result = await service.SignInComplete(payload, deviceInfo, country);
                sw.Stop();
                app.Logger.LogInformation("event=signin/complete account={account} duration={duration}", accountname, sw.ElapsedMilliseconds);

                return Results.Json(result, Json.Options);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default").WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/verify", async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                app.Logger.LogInformation("/signin/verify called");
                var sw = Stopwatch.StartNew();
                var payload = await req.ReadFromJsonAsync<SignInVerifyDTO>(Json.Options);


                var accountname = await accountService.ValidateSecretApiKey(req.GetApiSecret());
                var service = await Fido2ServiceEndpoints.Create(accountname, app.Logger, app.Configuration, storage);

                var result = await service.SignInVerify(payload);
                sw.Stop();
                app.Logger.LogInformation("event=signin/verify account={account} success={success} duration={duration}", accountname, result.Success, sw.ElapsedMilliseconds);

                return Results.Json(result, Json.Options);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");
    }
}
