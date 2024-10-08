using System.Net;
using System.Net.Http.Json;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Middleware;

[Collection(ApiCollectionFixture.Fixture)]
public class AuthorizationIntegrationTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
{

    [Fact]
    public async Task I_receive_a_403_when_i_use_a_invalid_api_key_with_an_existing_endpoint()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {

            TestOutput = testOutput
        });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await client.CreateApplicationAsync(applicationName);
        client.AddPublicKey($"{Guid.NewGuid():N}:public:{Guid.NewGuid():N}");

        // Act
        using var actual = await client.PostAsJsonAsync("/register/begin", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }

    [Fact]
    public async Task I_receive_a_403_when_i_use_a_badly_formatted_api_key_with_an_existing_endpoint()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {

            TestOutput = testOutput
        });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await client.CreateApplicationAsync(applicationName);
        client.AddPublicKey("e=mc2trooper");

        // Act
        using var actual = await client.PostAsJsonAsync("/register/begin", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }

    [Fact]
    public async Task I_receive_a_403_when_i_use_a_invalid_api_secret_with_an_existing_endpoint()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {

            TestOutput = testOutput
        });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey($"{Guid.NewGuid():N}:secret:{Guid.NewGuid():N}");

        // Act
        using var actual = await client.PostAsJsonAsync("/register/token", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }

    [Fact]
    public async Task I_receive_a_403_when_i_use_a_badly_formatted_api_secret_with_an_existing_endpoint()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions
        {

            TestOutput = testOutput
        });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey("e=mc2trooper");

        // Act
        using var actual = await client.PostAsJsonAsync("/register/token", string.Empty);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, actual.StatusCode);
    }
}