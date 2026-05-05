using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.MagicLinks;

namespace Passwordless.AdminConsole.Tests.DataFactory;

public class FakeMagicLinkSignInManager : MagicLinkSignInManager<ConsoleAdmin>
{
    public const string SuccessToken = "successtoken";
    public const string FailToken = "failtoken";

    public List<string> SentEmails { get; } = new();

    public FakeMagicLinkSignInManager() : this(new FakeUserManager()) { }

    public FakeMagicLinkSignInManager(UserManager<ConsoleAdmin> userManager)
        : base(
            new Mock<IPasswordlessClient>().Object,
            new Mock<IMagicLinkBuilder>().Object,
            userManager,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ConsoleAdmin>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<MagicLinkSignInManager<ConsoleAdmin>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<ConsoleAdmin>>().Object,
            new Mock<IEventLogger>().Object)
    {

    }

    public override async Task<SignInResult> PasswordlessSignInAsync(string token, bool isPersistent)
    {
        switch (token)
        {
            case SuccessToken:
                return SignInResult.Success;
            default:
                return SignInResult.Failed;
        }
    }

    public override Task SendEmailForSignInAsync(string email, string? returnUrl)
    {
        SentEmails.Add(email);
        return Task.CompletedTask;
    }
}
