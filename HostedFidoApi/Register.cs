using ApiHelpers;
using Service;
using Service.Helpers;
using Service.Models;
using Service.Storage;
using System.Diagnostics;

public static class RegisterEndpoints
{
    public static void MapRegisterEndpoints(this WebApplication app)
    {


        app.MapPost("/register/token", async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                var sw = Stopwatch.StartNew();
                app.Logger.LogInformation("/register/token called");
                var payload = await req.ReadFromJsonAsync<RegisterTokenDTO>(Json.Options);
                var accountname = await accountService.ValidateSecretApiKey(req.GetApiSecret());
                var service = await Fido2ServiceEndpoints.Create(accountname, app.Logger, app.Configuration, storage);
                var result = await service.CreateToken(payload);
                sw.Stop();
                app.Logger.LogInformation("event=register/token account={account} duration={duration}", accountname, sw.ElapsedMilliseconds);

                
                return Results.Text(result);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");

        app.MapPost("/register/begin", async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                app.Logger.LogInformation("/register/begin called");
                var sw = Stopwatch.StartNew();
                var payload = await req.ReadFromJsonAsync<FidoRegistrationBeginDTO>(Json.Options);


                var accountname = await accountService.ValidatePublicApiKey(req.GetPublicApiKey());
                var service = await Fido2ServiceEndpoints.Create(accountname, app.Logger, app.Configuration, storage);
                var result = await service.RegisterBegin(payload);
                sw.Stop();
                app.Logger.LogInformation("event=register/begin account={account} duration={duration}", accountname, sw.ElapsedMilliseconds);



                return Results.Json(result, Json.Options);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default").WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/register/complete", async (HttpContext ctx, HttpRequest req, IStorage storage, AccountService accountService) =>
        {
            try
            {
                app.Logger.LogInformation("/register/complete called");
                var sw = Stopwatch.StartNew();
                var payload = await req.ReadFromJsonAsync<RegistrationCompleteDTO>(Json.Options);


                var accountname = await accountService.ValidatePublicApiKey(req.GetPublicApiKey());
                var service = await Fido2ServiceEndpoints.Create(accountname, app.Logger, app.Configuration, storage);

                var (deviceInfo, country) = Helpers.GetDeviceInfo(req);
                var result = await service.RegisterComplete(payload, deviceInfo, country);
                sw.Stop();
                app.Logger.LogInformation("event=register/complete account={account} duration={duration}", accountname, sw.ElapsedMilliseconds);
                // Avoid serializing the certificate
                if (result.Data.Result is not null)
                {
                    result.Data.Result.AttestationCertificateChain = null;
                    result.Data.Result.AttestationCertificate = null;
                }
                return Results.Json(result, Json.Options);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default").WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true));
    }
}
