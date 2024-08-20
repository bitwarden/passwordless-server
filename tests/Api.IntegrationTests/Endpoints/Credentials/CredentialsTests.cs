using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fido2NetLib;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Api.Models;
using Passwordless.Common.Models.Credentials;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Credentials;

[Collection(ApiCollectionFixture.Fixture)]
public class CredentialsTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
{
    private readonly Faker<RegisterToken> _tokenGenerator = RequestHelpers.GetRegisterTokenGeneratorRules();

    [Fact]
    public async Task I_can_view_a_list_of_registered_users_credentials()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddUserAgent();
        var app = await client.CreateApplicationAsync();
        client.AddPublicKey(app.ApiKey1).AddSecretKey(app.ApiSecret1);

        const string originUrl = PasswordlessApi.OriginUrl;
        const string rpId = PasswordlessApi.RpId;
        var tokenRequest = _tokenGenerator.Generate();
        using var tokenResponse = await client.PostAsJsonAsync("register/token", tokenRequest);
        var registerTokenResponse =
            await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();
        var registrationBeginRequest =
            new FidoRegistrationBeginDTO { Token = registerTokenResponse!.Token, Origin = originUrl, RPID = rpId };
        using var registrationBeginResponse = await client.PostAsJsonAsync("register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content
            .ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var authenticatorAttestationRawResponse =
            await BrowserCredentialsHelper.CreateCredentialsAsync(sessionResponse!.Data, originUrl);

        _ = await client.PostAsJsonAsync("register/complete",
            new RegistrationCompleteDTO
            {
                Origin = originUrl,
                RPID = rpId,
                Session = sessionResponse.Session,
                Response = authenticatorAttestationRawResponse
            });

        // Act
        using var credentialsResponse = await client.GetAsync($"credentials/list?userId={tokenRequest.UserId}");

        // Assert
        credentialsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var credentialsList = await credentialsResponse.Content.ReadFromJsonAsync<ListResponse<StoredCredential>>();

        credentialsList.Should().NotBeNull();
        credentialsList!.Values.Should().NotBeNullOrEmpty();
        credentialsList.Values.Should().HaveCount(1);
    }

    [Fact]
    public async Task I_am_told_to_pass_the_user_id_when_getting_credential_list_using_get_with_secret_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddUserAgent();
        var app = await client.CreateApplicationAsync();
        client.AddPublicKey(app.ApiKey1).AddSecretKey(app.ApiSecret1);

        // Act
        using var credentialsResponse = await client.GetAsync("credentials/list");

        // Assert
        credentialsResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await credentialsResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Required parameter \"string UserId\" was not provided from query string.");
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task I_am_told_to_pass_the_user_id_when_getting_credential_list_using_post_with_secret_key()
    {
        // Arrange
        await using var api = apiFixture.CreateApi(new PasswordlessApiOptions { TestOutput = testOutput });
        using var client = api.CreateClient().AddUserAgent();
        var app = await client.CreateApplicationAsync();
        client.AddPublicKey(app.ApiKey1).AddSecretKey(app.ApiSecret1);

        var request = new GetCredentialsRequest(null!);

        // Act
        using var credentialsResponse = await client.PostAsJsonAsync("credentials/list", request);

        // Assert
        credentialsResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await credentialsResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }
}