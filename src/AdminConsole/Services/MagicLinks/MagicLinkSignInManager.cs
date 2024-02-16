using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkSignInManager<TUser> : SignInManager<TUser> where TUser : class
{
    private readonly MagicClient _magicClient;
    private readonly IMagicLinkBuilder _magicLinkBuilder;
    private readonly IEventLogger _eventLogger;

    public MagicLinkSignInManager(
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
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _magicClient = magicClient;
        _magicLinkBuilder = magicLinkBuilder;
        _eventLogger = eventLogger;
    }

    public async Task<SignInResult> SendEmailForSignInAsync(string email, string? returnUrl)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user is not ConsoleAdmin admin) return SignInResult.Success;
        
        var urlTemplate = _magicLinkBuilder.GetUrlTemplate();
        try
        {
            await _magicClient.SendMagicLinkAsync(admin.Id, admin.Email, urlTemplate);
        }
        catch (PasswordlessApiException e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        _eventLogger.LogCreateLoginViaMagicLinkEvent(admin);

        return SignInResult.Success;

    }

    public async Task<SignInResult> PasswordlessSignInAsync(string email, string token, bool isPersistent)
    {
        var verifiedUser = await _magicClient.VerifyAuthenticationTokenAsync(token);
        // todo: error handling
        
        var user = await UserManager.FindByIdAsync(verifiedUser.UserId);
        if (user == null)
        {
            return SignInResult.Failed;
        }
        
        await SignInAsync(user, isPersistent, "magic");
        return SignInResult.Success;
    }
}