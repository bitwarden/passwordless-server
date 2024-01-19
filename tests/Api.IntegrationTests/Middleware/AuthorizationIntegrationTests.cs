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
    public async Task I_receive_a_403_when_i_use_a_invalid_api_key_with_an_existing_endpoint()
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
    public async Task I_receive_a_403_when_i_use_a_badly_formatted_api_key_with_an_existing_endpoint()
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
    public async Task I_receive_a_403_when_i_use_a_invalid_api_secret_with_an_existing_endpoint()
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
    public async Task I_receive_a_403_when_i_use_a_badly_formatted_api_secret_with_an_existing_endpoint()
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