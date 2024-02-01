using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.IntegrationTests.Endpoints.Authenticators;

public class AuthenticatorsTests(PasswordlessApiFactory passwordlessApiFactory)
    : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client = passwordlessApiFactory.CreateClient();

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
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);
        await _client.EnableAttestation(applicationName);

        // Act
        var actual = await _client.GetAsync("authenticators/list?isAllowed=true");

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task I_can_retrieve_configured_authenticators_with_expected_result()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);
        await _client.EnableAttestation(applicationName);
        var request = new AddAuthenticatorsRequest(new List<Guid>
        {
            Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F")
        }, true);
        _ = await _client.PostAsJsonAsync("authenticators/add", request);

        // Act
        var actual = await _client.GetFromJsonAsync<IReadOnlyCollection<ConfiguredAuthenticatorResponse>>("authenticators/list?isAllowed=true");

        // Assert
        actual.Should().NotBeNull();
        actual!.Count.Should().Be(1);
        actual.First().AaGuid.Should().Be(Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F"));
    }

    [Fact]
    public async Task I_receive_forbidden_when_retrieving_configured_authenticators_when_attestation_is_not_allowed()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);

        // Act
        var actual = await _client.GetAsync("authenticators/list?isAllowed=true");

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_can_whitelist_authenticators_when_attestation_is_allowed()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);
        await _client.EnableAttestation(applicationName);

        var request = new AddAuthenticatorsRequest(new List<Guid>
        {
            Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F")
        }, true);

        // Act
        var actual = await _client.PostAsJsonAsync("authenticators/add", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_receive_forbidden_when_whitelisting_authenticators_when_attestation_is_not_allowed()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);

        var request = new AddAuthenticatorsRequest(new List<Guid>
        {
            Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F")
        }, true);

        // Act
        var actual = await _client.PostAsJsonAsync("authenticators/add", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_can_delist_authenticators_when_attestation_is_allowed()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);
        await _client.EnableAttestation(applicationName);

        var whitelistRequest = new AddAuthenticatorsRequest(new List<Guid>
        {
            Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F")
        }, true);
        _ = await _client.PostAsJsonAsync("authenticators/add", whitelistRequest);
        var request = new RemoveAuthenticatorsRequest(
            new List<Guid> { Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F") });

        // Act
        var actual = await _client.PostAsJsonAsync("authenticators/remove", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_receive_forbidden_when_delisting_authenticators_when_attestation_is_not_allowed()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);

        var request = new RemoveAuthenticatorsRequest(new List<Guid>
        {
            Guid.Parse("973446CA-E21C-9A9B-99F5-9B985A67AF0F")
        });

        // Act
        var actual = await _client.PostAsJsonAsync("authenticators/remove", request);

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}