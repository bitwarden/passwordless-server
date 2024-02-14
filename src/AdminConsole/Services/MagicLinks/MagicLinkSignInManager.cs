using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkSignInManager<TUser> : SignInManager<TUser> where TUser : class
{
    private readonly MagicClient _magicClient;
    private readonly IMagicLinkBuilder _magicLinkBuilder;
    private readonly IMailService _mailService;
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
        IMailService mailService,
        IEventLogger eventLogger)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _magicClient = magicClient;
        _magicLinkBuilder = magicLinkBuilder;
        _mailService = mailService;
        _eventLogger = eventLogger;
    }

    public async Task<SignInResult> SendEmailForSignInAsync(string email, string? returnUrl)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user is not ConsoleAdmin admin) return SignInResult.Success;
        
        var url = "https://localhost:8002/account/magic?token=<token>";
        var urlTemplate = _magicLinkBuilder.GetUrlTemplate();
        try
        {
            await _magicClient.SendMagicLinkAsync(admin.Id, admin.Email, url);
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
        var verifiedUser = await _magicClient.VerifyTokenAsync(token);
        // todo: error handling
        
        var user = await UserManager.FindByIdAsync(verifiedUser.UserId);
        if (user == null)
        {
            return SignInResult.Failed;
        }
        // SignInOrTwoFactorAsync
        await SignInAsync(user, isPersistent, "magic");
        return SignInResult.Success;
    }
    
        
    // public async Task<SignInResult> OLD_METHOD(string email, string? returnUrl)
    // {
    //     // var magicLink = await _magicLinkBuilder.GetLinkAsync(email, returnUrl);
    //     // await _mailService.SendPasswordlessSignInAsync(magicLink, email);
    //     //
    //     // var user = await UserManager.FindByEmailAsync(email);
    //     // if (user is ConsoleAdmin admin)
    //     //     _eventLogger.LogCreateLoginViaMagicLinkEvent(admin);
    //     //
    //     // return SignInResult.Success;
    //
    // }

}