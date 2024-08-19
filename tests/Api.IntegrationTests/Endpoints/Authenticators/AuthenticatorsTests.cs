using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Authenticators;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Authenticators;

public class AuthenticatorsTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    [Fact]
    public async Task I_can_retrieve_configured_authenticators_when_attestation_is_allowed()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(app.AppId);

        // Act
        var actual = await client.GetAsync("authenticators/list?isAllowed=true");

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task I_can_retrieve_configured_authenticators_with_expected_result()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(app.AppId);
        var request =
            new AddAuthenticatorsRequest(new List<Guid> { Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F") }, true);
        _ = await client.PostAsJsonAsync("authenticators/add", request);

        // Act
        var actual =
            await client.GetFromJsonAsync<IReadOnlyCollection<ConfiguredAuthenticatorResponse>>(
                "authenticators/list?isAllowed=true");

        // Assert
        actual.Should().NotBeNull();
        actual!.Count.Should().Be(1);
        actual.First().AaGuid.Should().Be(Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F"));
    }

    [Fact]
    public async Task I_receive_forbidden_when_retrieving_configured_authenticators_when_attestation_is_not_allowed()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);

        // Act
        var actual = await client.GetAsync("authenticators/list?isAllowed=true");

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_can_whitelist_authenticators_when_attestation_is_allowed()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(app.AppId);

        var request =
            new AddAuthenticatorsRequest(new List<Guid> { Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F") }, true);

        // Act
        var actual = await client.PostAsJsonAsync("authenticators/add", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_receive_forbidden_when_whitelisting_authenticators_when_attestation_is_not_allowed()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);

        var request =
            new AddAuthenticatorsRequest(new List<Guid> { Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F") }, true);

        // Act
        var actual = await client.PostAsJsonAsync("authenticators/add", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_can_delist_authenticators_when_attestation_is_allowed()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(app.AppId);

        var whitelistRequest =
            new AddAuthenticatorsRequest(new List<Guid> { Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F") }, true);
        _ = await client.PostAsJsonAsync("authenticators/add", whitelistRequest);
        var request = new RemoveAuthenticatorsRequest(
            new List<Guid> { Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F") });

        // Act
        var actual = await client.PostAsJsonAsync("authenticators/remove", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_receive_forbidden_when_delisting_authenticators_when_attestation_is_not_allowed()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var app = await client.CreateApplicationAsync();
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);

        var request = new RemoveAuthenticatorsRequest(new List<Guid>
        {
            Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F")
        });

        // Act
        var actual = await client.PostAsJsonAsync("authenticators/remove", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}