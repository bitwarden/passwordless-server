using System.Net;
using System.Net.Http.Json;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Middleware;

public class RoutingIntegrationTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    [Fact]
    public async Task I_receive_a_404_when_i_use_a_badly_formatted_api_secret_with_a_non_existing_endpoint()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions
        {

            TestOutput = testOutput
        });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey("e=mc2trooper");

        // Act
        using var actual = await client.GetAsync("/non-existent-endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
    }

    [Fact]
    public async Task I_receive_a_404_when_i_use_a_badly_formatted_api_key_with_a_non_existing_endpoint()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(new PasswordlessApiOptions
        {

            TestOutput = testOutput
        });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await client.CreateApplicationAsync(applicationName);
        client.AddPublicKey("e=mc2trooper");

        // Act
        using var actual = await client.GetAsync("/non-existent-endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
    }
}