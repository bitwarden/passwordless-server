using System.Net;
using System.Net.Http.Json;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Middleware;

public class AuthorizationIntegrationTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly HttpClient _client;

    public AuthorizationIntegrationTests(ITestOutputHelper testOutput, PasswordlessApiFactory apiFactory)
    {
        apiFactory.TestOutput = testOutput;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task ExistentEndpointProtectedByApiKey_Returns_Unauthorized_WhenBadApiKeyIsProvided()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        _ = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddPublicKey($"{Guid.NewGuid():N}:public:{Guid.NewGuid():N}");

        // Act
        using var actual = await _client.PostAsJsonAsync("/register/begin", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }

    [Fact]
    public async Task ExistentEndpointProtectedByApiKey_Returns_Unauthorized_WhenBadFormatApiKeyIsProvided()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        _ = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddPublicKey("e=mc2trooper");

        // Act
        using var actual = await _client.PostAsJsonAsync("/register/begin", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }

    [Fact]
    public async Task ExistentEndpointProtectedByApiSecret_Returns_Unauthorized_WhenBadApiSecretIsProvided()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        _ = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey($"{Guid.NewGuid():N}:secret:{Guid.NewGuid():N}");

        // Act
        using var actual = await _client.PostAsJsonAsync("/register/token", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }

    [Fact]
    public async Task ExistentEndpointProtectedByApiSecret_Returns_Unauthorized_WhenBadFormatApiSecretIsProvided()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        _ = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey("e=mc2trooper");

        // Act
        using var actual = await _client.PostAsJsonAsync("/register/token", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }
}