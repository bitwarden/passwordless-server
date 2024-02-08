using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Api.IntegrationTests.Helpers.User;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;
using Xunit;
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.SignIn;

public class SignInTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    [Fact]
    public async Task I_can_retrieve_assertion_options_to_begin_sign_in()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        var request = new SignInBeginDTO { Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId };

        // Act
        using var response = await client.PostAsJsonAsync("/signin/begin", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInResponse = await response.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        signInResponse.Should().NotBeNull();
        signInResponse!.Session.Should().StartWith("session_");
        signInResponse.Data.RpId.Should().Be(request.RPID);
        signInResponse.Data.Status.Should().Be("ok");
    }

    [Fact]
    public async Task I_can_retrieve_my_passkey_after_registering_and_receive_a_sign_in_token()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var httpClient = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        using var driver = WebDriverFactory.GetDriver(PasswordlessApi.OriginUrl);
        await httpClient.RegisterNewUser(driver);

        var signInBeginResponse = await httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();
        var authenticatorAssertionRawResponse = await driver.GetCredentialsAsync(signInBegin!.Data);

        // Act
        using var signInCompleteResponse = await httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId,
            Response = authenticatorAssertionRawResponse,
            Session = signInBegin.Session
        });

        // Assert
        signInCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInTokenResponse = await signInCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        signInTokenResponse.Should().NotBeNull();
        signInTokenResponse!.Token.Should().StartWith("verify_");
    }

    [Fact]
    public async Task I_can_retrieve_my_passkey_after_registering_and_receive_a_valid_sign_in_token()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var httpClient = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        using var driver = WebDriverFactory.GetDriver(PasswordlessApi.OriginUrl);
        await httpClient.RegisterNewUser(driver);

        var signInBeginResponse = await httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        var authenticatorAssertionRawResponse = await driver.GetCredentialsAsync(signInBegin!.Data);

        // Act
        using var signInCompleteResponse = await httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId,
            Response = authenticatorAssertionRawResponse,
            Session = signInBegin.Session
        });

        // Assert
        signInCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInTokenResponse = await signInCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        signInTokenResponse.Should().NotBeNull();
        signInTokenResponse!.Token.Should().StartWith("verify_");

        var verifySignInResponse = await httpClient.PostAsJsonAsync("/signin/verify", new SignInVerifyDTO { Token = signInTokenResponse.Token });
        verifySignInResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task I_receive_an_error_message_when_sending_an_unrecognized_passkey()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var _httpClient = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        using var options = await _httpClient.PostAsJsonAsync("/signin/begin", new { Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId });
        var response = await options.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();
        var payloadWithUnrecognizedPasskey = new
        {
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId,
            Session = response!.Session,
            Response = new
            {
                Id = "LcVLKA2QkfwzvuSTxIIyFVTJ9IopE57xTYvJ_0Nx9nk",
                RawId = "LcVLKA2QkfwzvuSTxIIyFVTJ9IopE57xTYvJ_0Nx9nk",
                Response = new
                {
                    AuthenticatorData = "8egiesVpgMwlnLcY0N3ldtvrzKGUPb763GkSYC4CzTkFAAAAAA",
                    ClientDataJson =
                        "eyJ0eXBlIjoid2ViYXV0aG4uZ2V0IiwiY2hhbGxlbmdlIjoiYmhPUllmRlR5S1hfdEtDQkFQbVVKdyIsIm9yaWdpbiI6Imh0dHBzOi8vYWRtaW4ucGFzc3dvcmRsZXNzLmRldiIsImNyb3NzT3JpZ2luIjpmYWxzZX0",
                    Signature =
                        "MEUCIQDiTSGMfKb_qZQDB0J8KFFZeAOrvCZEF2yi6MgoUNwkTgIgHYfe8LKPg-INMK9NxJfPaCdNsRUtP2DMhDKraJAOvzk"
                },
                type = "public-key"
            }
        };

        // Act
        using var completeResponse = await _httpClient.PostAsJsonAsync("/signin/complete", payloadWithUnrecognizedPasskey);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await completeResponse.Content.ReadAsStringAsync();
        AssertHelper.AssertEqualJson(
            // lang=json
            """
             {
               "type": "https://docs.passwordless.dev/guide/errors.html#unknown_credential",
               "title": "We don't recognize the passkey you sent us.",
               "status": 400,
               "credentialId": "LcVLKA2QkfwzvuSTxIIyFVTJ9IopE57xTYvJ_0Nx9nk",
               "errorCode": "unknown_credential"
             }
             """, body);
    }

    [Fact]
    public async Task An_expired_apps_token_keys_should_be_removed_when_a_request_is_made()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var httpClient = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        var applicationName = $"test{Guid.NewGuid():N}";
        using var client = api.CreateClient().AddManagementKey();
        using var createApplicationMessage = await client.CreateApplicationAsync(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddPublicKey(accountKeysCreation!.ApiKey1);
        client.AddSecretKey(accountKeysCreation.ApiSecret1);
        using var driver = WebDriverFactory.GetDriver(PasswordlessApi.OriginUrl);
        await client.RegisterNewUser(driver);
        api.Time.Advance(TimeSpan.FromDays(31));

        // Act
        using var response = await client.SignInUser(driver);

        // Assert
        using var scope = api.Services.CreateScope();
        var tokenKeys = await scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>().Create(applicationName).GetTokenKeys();
        tokenKeys.Should().NotBeNull();
        tokenKeys.Any(x => x.CreatedAt < (DateTime.UtcNow.AddDays(-30))).Should().BeFalse();
        tokenKeys.Any(x => x.CreatedAt >= (DateTime.UtcNow.AddDays(-30))).Should().BeTrue();
    }

    [Fact]
    public async Task I_receive_a_sign_in_token_for_a_valid_user_id()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var httpClient = api.CreateClient().AddPublicKey().AddSecretKey().AddUserAgent();

        using var client = api.CreateClient().AddManagementKey();
        using var createApplicationMessage = await client.CreateApplicationAsync();
        var userId = $"user{Guid.NewGuid():N}";
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddPublicKey(accountKeysCreation!.ApiKey1)
            .AddSecretKey(accountKeysCreation.ApiSecret1)
            .AddUserAgent();

        // Act
        using var signInGenerateTokenResponse = await client.PostAsJsonAsync("signin/generate-token", new SigninTokenRequest(userId)
        {
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId
        });

        // Assert
        signInGenerateTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var generateToken = await signInGenerateTokenResponse.Content.ReadFromJsonAsync<SigninEndpoints.SigninTokenResponse>();
        generateToken.Should().NotBeNull();
        generateToken!.Token.Should().StartWith("verify_");

        var verifySignInResponse = await client.PostAsJsonAsync("/signin/verify", new SignInVerifyDTO { Token = generateToken.Token, Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId });
        verifySignInResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}