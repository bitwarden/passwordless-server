using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Tests.DataFactory;

public class FakeUserManager : UserManager<ConsoleAdmin>
{
    public FakeUserManager()
        : base(new Mock<IUserStore<ConsoleAdmin>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ConsoleAdmin>>().Object,
            new IUserValidator<ConsoleAdmin>[0],
            new IPasswordValidator<ConsoleAdmin>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ConsoleAdmin>>>().Object)
    { }

    public override Task<IdentityResult> CreateAsync(ConsoleAdmin user, string password)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public override Task<IdentityResult> AddToRoleAsync(ConsoleAdmin user, string role)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public override Task<string> GenerateEmailConfirmationTokenAsync(ConsoleAdmin user)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

}