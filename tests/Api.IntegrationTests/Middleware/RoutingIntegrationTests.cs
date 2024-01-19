using System.Net;
using System.Net.Http.Json;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Middleware;

public class RoutingIntegrationTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly HttpClient _client;

    public RoutingIntegrationTests(ITestOutputHelper testOutput, PasswordlessApiFactory apiFactory)
    {
        apiFactory.TestOutput = testOutput;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task I_receive_a_404_when_i_use_a_badly_formatted_api_secret_with_a_non_existing_endpoint()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        _ = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey("e=mc2trooper");

        // Act
        using var actual = await _client.GetAsync("/non-existent-endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
    }

    [Fact]
    public async Task I_receive_a_404_when_i_use_a_badly_formatted_api_key_with_a_non_existing_endpoint()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        _ = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddPublicKey("e=mc2trooper");

        // Act
        using var actual = await _client.GetAsync("/non-existent-endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
    }
}