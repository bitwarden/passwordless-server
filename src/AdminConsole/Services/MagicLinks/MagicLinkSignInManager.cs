using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.Mail;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkSignInManager<TUser> : SignInManager<TUser> where TUser : class
{
    private readonly IMagicLinkBuilder _magicLinkBuilder;
    private readonly IMailService _mailService;
    private readonly IEventLogger _eventLogger;

    public MagicLinkSignInManager(
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
        _magicLinkBuilder = magicLinkBuilder;
        _mailService = mailService;
        _eventLogger = eventLogger;
    }

    public async Task<SignInResult> SendEmailForSignInAsync(string email, string? returnUrl)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            Logger.LogInformation("User {email} not found.", email);
            return SignInResult.Failed;
        }

        var magicLink = await _magicLinkBuilder.GetLinkAsync(email, returnUrl);
        await _mailService.SendPasswordlessSignInAsync(magicLink, email);

        if (user is ConsoleAdmin admin)
            _eventLogger.LogCreateLoginViaMagicLinkEvent(admin);

        return SignInResult.Success;
    }

    public async Task<SignInResult> PasswordlessSignInAsync(TUser user, string token, bool isPersistent)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var attempt = await CheckPasswordlessSignInAsync(user, token);
        return attempt.Succeeded
            ? await SignInOrTwoFactorAsync(user, isPersistent, bypassTwoFactor: true)
            : attempt;
    }

    public async Task<SignInResult> PasswordlessSignInAsync(string email, string token, bool isPersistent)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            return SignInResult.Failed;
        }

        return await PasswordlessSignInAsync(user, token, isPersistent);
    }

    public virtual async Task<SignInResult> CheckPasswordlessSignInAsync(TUser user, string token)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var error = await PreSignInCheck(user);
        if (error != null)
        {
            return error;
        }

        // convert back from url safe
        //token = token.Replace("-", "+").Replace("_", "/");
        if (await UserManager.VerifyUserTokenAsync(user, Options.Tokens.PasswordResetTokenProvider,
            SignInPurposes.MagicLink, token))
        {
            return SignInResult.Success;
        }

        Logger.LogWarning(2, "User {userId} failed to provide the correct token.",
            await UserManager.GetUserIdAsync(user));
        return SignInResult.Failed;
    }
}