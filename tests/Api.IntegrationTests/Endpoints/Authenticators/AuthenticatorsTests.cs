using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Authenticators;

[Collection(ApiCollectionFixture.Fixture)]
public class AuthenticatorsTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
{
    private static readonly Faker<RegisterToken> TokenGenerator = new Faker<RegisterToken>()
        .RuleFor(x => x.UserId, Guid.NewGuid().ToString())
        .RuleFor(x => x.DisplayName, x => x.Person.FullName)
        .RuleFor(x => x.Username, x => x.Person.Email)
        .RuleFor(x => x.Attestation, "Direct")
        .RuleFor(x => x.Discoverable, true)
        .RuleFor(x => x.UserVerification, "Preferred")
        .RuleFor(x => x.Aliases, x => new HashSet<string> { x.Person.FirstName })
        .RuleFor(x => x.AliasHashing, false)
        .RuleFor(x => x.ExpiresAt, DateTime.UtcNow.AddDays(1))
        .RuleFor(x => x.TokenId, Guid.Empty);

    [Fact]
    public async Task I_can_retrieve_configured_authenticators_when_attestation_is_allowed()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(applicationName);

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
        var applicationName = CreateAppHelpers.GetApplicationName();

        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(applicationName);
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
        var applicationName = CreateAppHelpers.GetApplicationName();

        var app = await client.CreateApplicationAsync(applicationName);
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
        var applicationName = CreateAppHelpers.GetApplicationName();

        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(applicationName);

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
        var applicationName = CreateAppHelpers.GetApplicationName();

        var app = await client.CreateApplicationAsync(applicationName);
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
        var applicationName = CreateAppHelpers.GetApplicationName();

        var app = await client.CreateApplicationAsync(applicationName);
        client.AddSecretKey(app.ApiSecret1);
        client.AddPublicKey(app.ApiKey1);
        await client.EnableAttestation(applicationName);

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
        var applicationName = CreateAppHelpers.GetApplicationName();

        var app = await client.CreateApplicationAsync(applicationName);
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