using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Net.Tests;

public class PasswordlessClientTests
{

    private const string SkipReason = "Manual test, you must be running the Api project.";
    // private const string SkipReason = null;

    private readonly PasswordlessClient _sut;

    public PasswordlessClientTests()
    {
        var services = new ServiceCollection();

        services.AddPasswordlessSdk(options =>
        {
            options.ApiUrl = "https://localhost:7002";
            options.ApiSecret = "test:secret:a679563b331846c79c20b114a4f56d02";
        });

        var provider = services.BuildServiceProvider();

        _sut = (PasswordlessClient)provider.GetRequiredService<IPasswordlessClient>();
    }

    [Fact(Skip = SkipReason)]
    public async Task CreateRegisterToken_ThrowsExceptionWhenBad()
    {
        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await _sut.CreateRegisterTokenAsync(new RegisterOptions
        {
            UserId = null!,
            Username = null!,
        }));
    }

    [Fact(Skip = SkipReason)]
    public async Task VerifyToken_DoesNotThrowOnBadToken()
    {
        var verifiedUser = await _sut.VerifyTokenAsync("bad_token");

        Assert.Null(verifiedUser);
    }

    [Fact(Skip = SkipReason)]
    public async Task DeleteUserAsync_BadUserId_ThrowsException()
    {
        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(
            async () => await _sut.DeleteUserAsync(null!));
    }

    [Fact(Skip = SkipReason)]
    public async Task ListAsiases_BadUserId_ThrowsException()
    {
        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(
            async () => await _sut.ListAliasesAsync(null!));
    }

    [Fact(Skip = SkipReason)]
    public async Task ListCredentials_BadUserId_ThrowsException()
    {
        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(
            async () => await _sut.ListCredentialsAsync(null!));
    }
}