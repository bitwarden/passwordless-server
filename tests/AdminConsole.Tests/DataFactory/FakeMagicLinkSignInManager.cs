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
    public FakeMagicLinkSignInManager()
        : base(
            new Mock<IPasswordlessClient>().Object,
            new Mock<IMagicLinkBuilder>().Object,
            new FakeUserManager(),
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ConsoleAdmin>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<MagicLinkSignInManager<ConsoleAdmin>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<ConsoleAdmin>>().Object,
            new Mock<IEventLogger>().Object)
    {

    }
}