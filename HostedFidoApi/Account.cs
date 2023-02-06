using ApiHelpers;
using Microsoft.AspNetCore.Http.Extensions;
using Service;
using Service.Helpers;
using Service.Models;
using Service.Storage;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/account/available", async (HttpContext ctx, HttpRequest req, AccountService accountService) => {
            try
            {
                app.Logger.LogInformation("/account/available called");
                var payload = await req.ReadFromJsonAsync<AccountAvailable>(Json.Options);

                // if payload is empty, throw exception
                if (payload == null)
                {
                    throw new ApiException("Payload is empty. Must contain Accountname", 400);
                }

                if (payload.AccountName.Length < 3)
                {
                    return Results.Json(false, statusCode: 409);
                }

                var result = await accountService.IsAvailable(payload.AccountName);

                app.Logger.LogInformation("event=account/available account={account} available={available}", payload.AccountName, result);

                if (result)
                {
                    return Results.Json(result, statusCode: 200);
                }
                else
                {
                    return Results.Json(result, statusCode: 409);
                }
            }
            catch(ApiException e) {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");
        

        app.MapPost("/account/create", async (HttpContext ctx, HttpRequest req, AccountService service) =>
        {
            try
            {
                app.Logger.LogInformation("/account/create called");
                var payload = await req.ReadFromJsonAsync<AccontCreateDTO>(Json.Options);

                // if payload is empty, throw exception
                if (payload == null)
                {
                    throw new ApiException("Payload is empty. Must contain Accountname and AdminEmail", 400);
                }

                var result = await service.GenerateAccount(payload.AccountName, payload.AdminEmail);

                app.Logger.LogInformation("event=account/create account={account} adminemail={adminemail}", payload.AccountName, payload.AdminEmail);

                
                return Results.Json(result, Json.Options);
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");

        app.MapPost("/account/delete", async (HttpContext ctx, HttpRequest req,  AccountService service) =>
        {
            try
            {
                app.Logger.LogInformation("/account/delete called");
                
                var accountname = await service.ValidateSecretApiKey(req.GetApiSecret());
                var accountInfo = await service.GetAccountInformation(accountname);


                var urib = new UriBuilder(req.GetDisplayUrl());
                urib.Path = "";

                var link = $"{urib.Uri.AbsoluteUri}account/delete/cancel/{accountname}";
                app.Logger.LogInformation("event=account/delete account={account} cancelUrl={cancelUrl}", accountname, link);

                // freeze accounts
                var sendConfirmationEmailInput = new EmailAboutAccountDeletion
                {
                    CancelLink = link,
                    Emails = accountInfo.AdminEmails,
                    AccountName = accountname,
                    Message = $"Your Passwordless.dev account '{accountname}' has been frozen because the account/delete endpoint was called. Your data will be deleted after 14 days. To stop this, please visist the URL: " + link
                };

                // Send warning email with url to abort
                await service.SendAbortEmail(app.Logger, app.Configuration, sendConfirmationEmailInput);
                await service.FreezeAccount(accountname);

                return Results.Json(new { Message = "Deletion process has been started. NO data has been removed. The account has been frozen, use the link to unfreeze and abort the operation.", cancelUrl = link });
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");

        app.MapGet("/account/delete/cancel/{accountname}", async (string accountname, HttpContext ctx, HttpRequest req, AccountService service) =>
        {
            try
            {
                app.Logger.LogInformation("event=account/delete/cancel account={account}", accountname);

                await service.UnFreezeAccount(accountname);

                return Results.Json("Your account will not be deleted since the process was aborted with the cancellation link");
            }
            catch (ApiException e)
            {
                return ErrorHelper.FromException(e);
            }
        }).RequireCors("default");

    }
}
