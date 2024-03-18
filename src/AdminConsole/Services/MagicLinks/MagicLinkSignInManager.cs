using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkSignInManager<TUser>(
    MagicClient magicClient,
    IMagicLinkBuilder magicLinkBuilder,
    UserManager<TUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<TUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<TUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<TUser> confirmation,
    IEventLogger eventLogger)
    : SignInManager<TUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    where TUser : class
{
    public async Task SendEmailForSignInAsync(string email, string? returnUrl)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user is not ConsoleAdmin admin)
        {
            // naive noise against timing attacks
            await Task.Delay(new Random().Next(100, 300));
            return;
        };

        var urlTemplate = magicLinkBuilder.GetUrlTemplate(returnUrl);
        try
        {
            await magicClient.SendMagicLinkAsync(admin.Id, admin.Email!, urlTemplate);
        }
        catch (PasswordlessApiException e)
        {
            Console.WriteLine(e);
            throw;
        }

        eventLogger.LogCreateLoginViaMagicLinkEvent(admin);
    }

    public async Task<SignInResult> PasswordlessSignInAsync(string token, bool isPersistent)
    {
        try
        {
            var verifiedUser = await magicClient.VerifyAuthenticationTokenAsync(token);

            var user = await UserManager.FindByIdAsync(verifiedUser.UserId);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            await SignInAsync(user, isPersistent, "magic");
            return SignInResult.Success;
        }
        catch (PasswordlessApiException e)
        {
            // Most likely the token had expired, we just return that it failed.
            return SignInResult.Failed;
        }
    }
}