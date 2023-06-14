using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.AspNetCore.Services;
using Passwordless.AspNetCore.Services.Implementations;
using Passwordless.Net;

namespace Passwordless.AspNetCore.Tests.Services;

public class PasswordlessServiceTests
{
    private readonly Mock<IPasswordlessClient> _mockPasswordlessClient;
    private readonly TestUserStore _testUserStore;
    private readonly PasswordlessAspNetCoreOptions _options;
    private readonly Mock<IUserClaimsPrincipalFactory<TestUser>> _mockUserClaimsPrincipalFactory;
    private readonly Mock<ICustomizeRegisterOptions> _mockCustomizeRegisterOptions;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public PasswordlessServiceTests()
    {
        _mockPasswordlessClient = new Mock<IPasswordlessClient>();
        _testUserStore = new TestUserStore();
        _options = new PasswordlessAspNetCoreOptions();
        _mockUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<TestUser>>();
        _mockCustomizeRegisterOptions = new Mock<ICustomizeRegisterOptions>();
        _mockServiceProvider = new Mock<IServiceProvider>();
    }

    private PasswordlessService<TestUser> CreateSut()
    {
        var mockOptions = new Mock<IOptions<PasswordlessAspNetCoreOptions>>();
        mockOptions.Setup(o => o.Value)
            .Returns(_options);

        return new PasswordlessService<TestUser>(
            _mockPasswordlessClient.Object,
            _testUserStore,
            NullLogger<PasswordlessService<TestUser>>.Instance,
            mockOptions.Object,
            _mockUserClaimsPrincipalFactory.Object,
            _mockCustomizeRegisterOptions.Object,
            _mockServiceProvider.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_Works()
    {
        var expectedResponse = new RegisterTokenResponse
        {
            Token = "test_token",
        };

        _options.Register.Discoverable = true;

        _mockPasswordlessClient
            .Setup(s => s.CreateRegisterToken(
                It.Is<RegisterOptions>(o => o.UserId != null && o.Discoverable == true)))
            .ReturnsAsync(expectedResponse);

        var sut = CreateSut();

        var result = await sut.RegisterUserAsync(new PasswordlessRegisterRequest(
            "test_username",
            "Test User",
            new HashSet<string> { "test" }), CancellationToken.None);

        var okResult = Assert.IsAssignableFrom<Ok<RegisterTokenResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);

        Assert.Single(_testUserStore.InnerStore.Where(kvp => kvp.Value.Username == "test_username"));
    }

    [Fact]
    public async Task RegisterUserAsync_CanNotCreateUser_ReturnsValidationProblems()
    {
        var mockUserStore = new Mock<IUserStore<TestUser>>();

        mockUserStore
            .Setup(s => s.SetUserNameAsync(It.IsAny<TestUser>(), "test_username", CancellationToken.None))
            .Callback<TestUser, string, CancellationToken>(
                (user, username, token) => user.Username = username);

        mockUserStore
            .Setup(s => s.CreateAsync(It.IsAny<TestUser>(), CancellationToken.None))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "failed",
                Description = "Failed to create user",
            }));

        // Have to manually create the sut in this test because we want a mocked IUserStore
        var sut = new PasswordlessService<TestUser>(
            _mockPasswordlessClient.Object,
            mockUserStore.Object,
            NullLogger<PasswordlessService<TestUser>>.Instance,
            Options.Create(_options),
            _mockUserClaimsPrincipalFactory.Object,
            _mockCustomizeRegisterOptions.Object,
            _mockServiceProvider.Object);

        var result = await sut.RegisterUserAsync(
            new PasswordlessRegisterRequest("test_username", "Test User", new HashSet<string> { "" }),
            CancellationToken.None);

        var problemResult = Assert.IsAssignableFrom<ProblemHttpResult>(result);
        var validationProblems = Assert.IsAssignableFrom<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.True(validationProblems.Errors.TryGetValue("failed", out var failedErrors));
        var failedError = Assert.Single(failedErrors);
        Assert.Equal("Failed to create user", failedError);
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(Guid? userId, string? authenticationType = "test")
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
    }

    [Fact]
    public async Task LoginUserAsync_UsesDefaultSchemeIfNoneSpecified()
    {
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Username = "login_test_user",
        };

        await _testUserStore.CreateAsync(user);

        _mockPasswordlessClient
            .Setup(s => s.VerifyToken("test_token"))
            .ReturnsAsync(new VerifiedUser
            {
                UserId = user.Id.ToString(),
            });

        _mockUserClaimsPrincipalFactory
            .Setup(s => s.CreateAsync(user))
            .ReturnsAsync(CreateClaimsPrincipal(user.Id));

        var sut = CreateSut();

        var result = await sut.LoginUserAsync(new PasswordlessLoginRequest("test_token"), CancellationToken.None);
        var signInResult = Assert.IsAssignableFrom<SignInHttpResult>(result);

        Assert.Null(signInResult.AuthenticationScheme);
    }

    [Fact]
    public async Task LoginUserAsync_UsesOurOptionIfSpecified()
    {
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Username = "login_test_user_2",
        };

        _options.SignInScheme = "our_scheme";

        await _testUserStore.CreateAsync(user);

        _mockPasswordlessClient
            .Setup(s => s.VerifyToken("test_token"))
            .ReturnsAsync(new VerifiedUser
            {
                UserId = user.Id.ToString(),
            });

        _mockUserClaimsPrincipalFactory
            .Setup(s => s.CreateAsync(user))
            .ReturnsAsync(CreateClaimsPrincipal(user.Id));

        var sut = CreateSut();

        var result = await sut.LoginUserAsync(new PasswordlessLoginRequest("test_token"), CancellationToken.None);
        var signInResult = Assert.IsAssignableFrom<SignInHttpResult>(result);

        Assert.Equal("our_scheme", signInResult.AuthenticationScheme);
    }

    [Fact]
    public async Task LoginUserAsync_TriesAuthenticationOptionsIfOursIsNull()
    {
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Username = "login_test_user_3",
        };

        _options.SignInScheme = null;

        _mockServiceProvider
            .Setup(s => s.GetService(typeof(IOptions<AuthenticationOptions>)))
            .Returns(Options.Create(new AuthenticationOptions
            {
                DefaultSignInScheme = "auth_options_scheme",
            }));

        await _testUserStore.CreateAsync(user);

        _mockPasswordlessClient
            .Setup(s => s.VerifyToken("test_token"))
            .ReturnsAsync(new VerifiedUser
            {
                UserId = user.Id.ToString(),
            });

        _mockUserClaimsPrincipalFactory
            .Setup(s => s.CreateAsync(user))
            .ReturnsAsync(CreateClaimsPrincipal(user.Id));

        var sut = CreateSut();

        var result = await sut.LoginUserAsync(new PasswordlessLoginRequest("test_token"), CancellationToken.None);
        var signInResult = Assert.IsAssignableFrom<SignInHttpResult>(result);

        Assert.Equal("auth_options_scheme", signInResult.AuthenticationScheme);
    }

    [Fact]
    public async Task LoginUserAsync_PasswordlessClientReturnsNull_ReturnsUnauthorized()
    {
        _mockPasswordlessClient
            .Setup(s => s.VerifyToken("test_token"))
            .Returns(Task.FromResult<VerifiedUser?>(null));

        var sut = CreateSut();

        var result = await sut.LoginUserAsync(new PasswordlessLoginRequest("test_token"), CancellationToken.None);
        Assert.IsAssignableFrom<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task LoginUserAsync_UserDoesNotExist_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();

        _mockPasswordlessClient
            .Setup(s => s.VerifyToken("test_token"))
            .ReturnsAsync(new VerifiedUser
            {
                UserId = userId.ToString(),
            });

        var sut = CreateSut();

        var result = await sut.LoginUserAsync(new PasswordlessLoginRequest("test_token"), CancellationToken.None);
        Assert.IsAssignableFrom<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task AddCredentialAsync_ReturnsRegisterTokenResponse()
    {
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Username = "add_credential_test_1",
        };

        await _testUserStore.CreateAsync(user);

        _mockPasswordlessClient
            .Setup(s => s.CreateRegisterToken(
                It.Is<RegisterOptions>(o => o.UserId == user.Id.ToString()
                    && o.Username == "add_credential_test_1")))
            .ReturnsAsync(new RegisterTokenResponse
            {
                Token = "test_register_token",
            });

        var sut = CreateSut();

        var result = await sut.AddCredentialAsync(
            new PasswordlessAddCredentialRequest("My Test Key"),
            CreateClaimsPrincipal(user.Id),
            CancellationToken.None);

        var okResult = Assert.IsAssignableFrom<Ok<RegisterTokenResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("test_register_token", okResult.Value.Token);
    }

    [Fact]
    public async Task AddCredentialAsync_UserCanNotBeFound_ReturnsUnauthorized()
    {
        var sut = CreateSut();

        var result = await sut.AddCredentialAsync(
            new PasswordlessAddCredentialRequest("My Test Key"),
            CreateClaimsPrincipal(Guid.NewGuid()),
            CancellationToken.None);

        Assert.IsAssignableFrom<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task AddCredentialAsync_UserIdCanNotBeFound_ReturnsUnauthorized()
    {
        var sut = CreateSut();

        var result = await sut.AddCredentialAsync(
            new PasswordlessAddCredentialRequest("My Test Key"),
            CreateClaimsPrincipal(null),
            CancellationToken.None);

        Assert.IsAssignableFrom<UnauthorizedHttpResult>(result);
    }
}

public sealed class TestUserStore : IUserEmailStore<TestUser>
{
    public Dictionary<Guid, TestUser> InnerStore { get; } = new Dictionary<Guid, TestUser>();

    public Task<IdentityResult> CreateAsync(TestUser user, CancellationToken cancellationToken = default)
    {
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }
        InnerStore.Add(user.Id, user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(TestUser user, CancellationToken cancellationToken = default)
    {
        var didRemove = InnerStore.Remove(user.Id);
        var result = didRemove ? IdentityResult.Success : IdentityResult.Failed(new IdentityError
            {
                Code = "error",
                Description = "Could not delete",
            });

        return Task.FromResult(result);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<TestUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var user = InnerStore.Values.FirstOrDefault(u => u.Email.Equals(normalizedEmail, StringComparison.InvariantCultureIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<TestUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!InnerStore.TryGetValue(Guid.Parse(userId), out var user))
        {
            return Task.FromResult<TestUser?>(null);
        }

        return Task.FromResult<TestUser?>(user);
    }

    public Task<TestUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetEmailAsync(TestUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(TestUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetNormalizedEmailAsync(TestUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetNormalizedUserNameAsync(TestUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetUserIdAsync(TestUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(TestUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(user.Username);
    }

    public Task SetEmailAsync(TestUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email!;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(TestUser user, bool confirmed, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetNormalizedEmailAsync(TestUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetNormalizedUserNameAsync(TestUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetUserNameAsync(TestUser user, string? userName, CancellationToken cancellationToken = default)
    {
        user.Username = userName!;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(TestUser user, CancellationToken cancellationToken = default)
    {
        InnerStore[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }
}

public class TestUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
}
