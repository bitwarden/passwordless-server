using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fido2NetLib;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Infra;
using Passwordless.Api.Models;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Credentials;

public class CredentialsTests : IClassFixture<TestApiFixture>, IDisposable
{
    private readonly TestApi _testApi;

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

    public CredentialsTests(ITestOutputHelper testOutput, TestApiFixture testApiFixture)
    {
        _testApi = testApiFixture.CreateApi(testOutput: testOutput);
    }

    [Fact]
    public async Task I_can_view_a_list_of_registered_users_credentials()
    {
        // Arrange
        const string originUrl = "https://bitwarden.com/products/passwordless/";
        const string rpId = "bitwarden.com";
        var tokenRequest = TokenGenerator.Generate();
        using var tokenResponse = await _testApi.Client.PostAsJsonAsync("register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();
        var registrationBeginRequest = new FidoRegistrationBeginDTO { Token = registerTokenResponse!.Token, Origin = originUrl, RPID = rpId };
        using var registrationBeginResponse = await _testApi.Client.PostAsJsonAsync("register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var authenticatorAttestationRawResponse = await BrowserCredentialsHelper.CreateCredentialsAsync(sessionResponse!.Data, originUrl);

        _ = await _testApi.Client.PostAsJsonAsync("register/complete",
            new RegistrationCompleteDTO { Origin = originUrl, RPID = rpId, Session = sessionResponse.Session, Response = authenticatorAttestationRawResponse });

        // Act
        using var credentialsResponse = await _testApi.Client.GetAsync($"credentials/list?userId={tokenRequest.UserId}");

        // Assert
        credentialsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var credentialsList = await credentialsResponse.Content.ReadFromJsonAsync<ListResponse<StoredCredential>>();

        credentialsList.Should().NotBeNull();
        credentialsList!.Values.Should().NotBeNullOrEmpty();
        credentialsList.Values.Should().HaveCount(1);
    }

    [Fact]
    public async Task I_am_told_to_pass_the_user_id_when_getting_credential_list_with_secret_key()
    {
        // Act
        using var credentialsResponse = await _testApi.Client.GetAsync("credentials/list");

        // Assert
        credentialsResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await credentialsResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Please supply UserId in the query string value");
    }

    public void Dispose()
    {
        _testApi.Dispose();
    }
}