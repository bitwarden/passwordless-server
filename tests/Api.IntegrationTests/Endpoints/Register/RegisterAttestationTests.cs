using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fido2NetLib;
using Fido2NetLib.Objects;
using FluentAssertions;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.IntegrationTests.Endpoints.Register;

public class RegisterAttestationTests(PasswordlessApiFactory passwordlessApiFactory)
    : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client = passwordlessApiFactory.CreateClient();

    private static readonly Faker<RegisterToken> TokenGenerator = new Faker<RegisterToken>()
        .RuleFor(x => x.UserId, Guid.NewGuid().ToString())
        .RuleFor(x => x.DisplayName, x => x.Person.FullName)
        .RuleFor(x => x.Username, x => x.Person.Email)
        .RuleFor(x => x.Attestation, "None")
        .RuleFor(x => x.Discoverable, true)
        .RuleFor(x => x.UserVerification, "Preferred")
        .RuleFor(x => x.Aliases, x => new HashSet<string> { x.Person.FirstName })
        .RuleFor(x => x.AliasHashing, false)
        .RuleFor(x => x.ExpiresAt, DateTime.UtcNow.AddDays(1))
        .RuleFor(x => x.TokenId, Guid.Empty);

    /// <summary>
    /// Verify that the user is able to use supported attestation methods when attestation is allowed.
    /// </summary>
    [Theory]
    [InlineData("none", AttestationConveyancePreference.None)]
    [InlineData("indirect", AttestationConveyancePreference.Indirect)]
    [InlineData("direct", AttestationConveyancePreference.Direct)]
    public async Task I_can_use_supported_attestation_methods_to_register_a_new_user_when_attestation_is_allowed(string attestation, AttestationConveyancePreference expectedAttestation)
    {
        // Arrange
        var tokenRequest = TokenGenerator
            .RuleFor(x => x.Attestation, attestation)
            .Generate();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);
        await _client.EnableAttestation(applicationName);

        // Act
        var tokenResponse = await _client.PostAsJsonAsync("/register/token", tokenRequest);

        // Assert
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Arrange Begin
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = PasswordlessApiFactory.OriginUrl,
            RPID = PasswordlessApiFactory.RpId
        };

        // Act Begin
        using var registrationBeginResponse = await _client.PostAsJsonAsync("/register/begin", registrationBeginRequest);

        // Assert Begin
        registrationBeginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();
        sessionResponse.Should().NotBeNull();
        sessionResponse!.Data.Attestation.Should().Be(expectedAttestation);
    }

    /// <summary>
    /// Verify that the user is able to use 'none' attestation method when attestation is disallowed.
    /// </summary>
    [Theory]
    [InlineData("none", AttestationConveyancePreference.None)]
    public async Task I_can_use_supported_none_attestation_method_to_register_a_new_user_when_attestation_is_disallowed(string attestation, AttestationConveyancePreference expectedAttestation)
    {
        // Arrange
        var tokenRequest = TokenGenerator
            .RuleFor(x => x.Attestation, attestation)
            .Generate();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);

        // Act
        var tokenResponse = await _client.PostAsJsonAsync("/register/token", tokenRequest);

        // Assert
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Arrange Begin
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = PasswordlessApiFactory.OriginUrl,
            RPID = PasswordlessApiFactory.RpId
        };

        // Act Begin
        using var registrationBeginResponse = await _client.PostAsJsonAsync("/register/begin", registrationBeginRequest);

        // Assert Begin
        registrationBeginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();
        sessionResponse.Should().NotBeNull();
        sessionResponse!.Data.Attestation.Should().Be(expectedAttestation);
    }

    /// <summary>
    /// Verify that the user is not able to use other than 'none' attestation method when attestation is disallowed.
    /// </summary>
    [Theory]
    [InlineData("indirect")]
    [InlineData("direct")]
    public async Task I_cannot_use_other_than_none_attestation_method_to_register_a_new_user_when_attestation_is_disallowed(string attestation)
    {
        // Arrange
        var tokenRequest = TokenGenerator
            .RuleFor(x => x.Attestation, attestation)
            .Generate();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        _client.AddPublicKey(accountKeysCreation!.ApiKey1);

        // Act
        var tokenResponse = await _client.PostAsJsonAsync("/register/token", tokenRequest);

        // Assert
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}