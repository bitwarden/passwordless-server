using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fido2NetLib;
using FluentAssertions;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Api.IntegrationTests.Helpers.User;
using Passwordless.Service.Models;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Register;

public class RegisterTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    private readonly Faker<RegisterToken> _tokenGenerator = RequestHelpers.GetRegisterTokenGeneratorRules();

    [Fact]
    public async Task I_can_retrieve_token_to_start_registration()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        var request = _tokenGenerator.Generate();

        // Act
        using var response = await client.PostAsJsonAsync("/register/token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerTokenResponse = await response.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();
        registerTokenResponse.Should().NotBeNull();
        registerTokenResponse!.Token.Should().StartWith("register_");
    }

    [Fact]
    public async Task I_can_retrieve_the_credential_create_options_and_session_token_for_creating_a_new_user()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        var tokenRequest = _tokenGenerator.Generate();
        var tokenResponse = await client.PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId
        };

        // Act
        using var registrationBeginResponse = await client.PostAsJsonAsync("/register/begin", registrationBeginRequest);

        // Assert
        registrationBeginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();
        sessionResponse.Should().NotBeNull();
        sessionResponse!.Data.User.DisplayName.Should().Be(tokenRequest.DisplayName);
        sessionResponse.Data.User.Name.Should().Be(tokenRequest.Username);
    }

    [Fact]
    public async Task I_can_use_a_passkey_to_register_a_new_user()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        using var driver = WebDriverFactory.GetDriver(PasswordlessApi.OriginUrl);

        // Act
        using var registerCompleteResponse = await UserHelpers.RegisterNewUser(client, driver);

        // Assert
        registerCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerCompleteToken = await registerCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        registerCompleteToken!.Token.Should().StartWith("verify_");
    }
}