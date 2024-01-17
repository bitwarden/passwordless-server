using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
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
    public async Task NonExistentEndpoint_Returns_NotFound_WhenBadFormatApiSecretHeadersAreProvided()
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
    public async Task NonExistentEndpoint_Returns_NotFound_WhenBadFormatApiKeyHeadersAreProvided()
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