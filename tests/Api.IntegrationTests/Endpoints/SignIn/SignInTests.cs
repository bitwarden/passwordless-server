using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Api.IntegrationTests.Helpers.User;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;
using Xunit;

namespace Passwordless.Api.IntegrationTests.Endpoints.SignIn;

public class SignInTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly HttpClient _httpClient;
    private readonly PasswordlessApiFactory _factory;

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


    public SignInTests(PasswordlessApiFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient()
            .AddPublicKey()
            .AddSecretKey()
            .AddUserAgent();
    }

    [Fact]
    public async Task I_can_retrieve_assertion_options_to_begin_sign_in()
    {
        // Arrange
        var request = new SignInBeginDTO { Origin = PasswordlessApiFactory.OriginUrl, RPID = PasswordlessApiFactory.RpId };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/signin/begin", request);

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
        using var driver = WebDriverFactory.GetWebDriver(PasswordlessApiFactory.OriginUrl);
        await UserHelpers.RegisterNewUser(_httpClient, driver);

        var signInBeginResponse = await _httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = PasswordlessApiFactory.OriginUrl, RPID = PasswordlessApiFactory.RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();
        var authenticatorAssertionRawResponse = await driver.GetCredentialsAsync(signInBegin!.Data);

        // Act
        var signInCompleteResponse = await _httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = PasswordlessApiFactory.OriginUrl,
            RPID = PasswordlessApiFactory.RpId,
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
        using var driver = WebDriverFactory.GetWebDriver(PasswordlessApiFactory.OriginUrl);
        await UserHelpers.RegisterNewUser(_httpClient, driver);

        var signInBeginResponse = await _httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = PasswordlessApiFactory.OriginUrl, RPID = PasswordlessApiFactory.RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        var authenticatorAssertionRawResponse = await driver.GetCredentialsAsync(signInBegin!.Data);

        // Act
        var signInCompleteResponse = await _httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = PasswordlessApiFactory.OriginUrl,
            RPID = PasswordlessApiFactory.RpId,
            Response = authenticatorAssertionRawResponse,
            Session = signInBegin.Session
        });

        // Assert
        signInCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInTokenResponse = await signInCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        signInTokenResponse.Should().NotBeNull();
        signInTokenResponse!.Token.Should().StartWith("verify_");

        var verifySignInResponse = await _httpClient.PostAsJsonAsync("/signin/verify", new SignInVerifyDTO { Token = signInTokenResponse.Token });
        verifySignInResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task I_receive_an_error_message_when_sending_an_unrecognized_passkey()
    {
        // Arrange
        using var options = await _httpClient.PostAsJsonAsync("/signin/begin", new { Origin = PasswordlessApiFactory.OriginUrl, RPID = PasswordlessApiFactory.RpId });
        var response = await options.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();
        var payloadWithUnrecognizedPasskey = new
        {
            Origin = PasswordlessApiFactory.OriginUrl,
            RPID = PasswordlessApiFactory.RpId,
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
        using var result = await _httpClient.PostAsJsonAsync("/signin/complete", payloadWithUnrecognizedPasskey);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await result.Content.ReadAsStringAsync();
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
        const string applicationName = "testRemoveToken";
        var serverTime = new DateTimeOffset(new DateTime(2023, 1, 1));
        _factory.TimeProvider.SetUtcNow(serverTime);
        using var client = _factory.CreateClient().AddManagementKey();
        using var createApplicationMessage = await CreateAppFunctions.CreateApplication(client, applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<AccountKeysCreation>();
        client.AddPublicKey(accountKeysCreation!.ApiKey1);
        client.AddSecretKey(accountKeysCreation.ApiSecret1);
        using var driver = WebDriverFactory.GetWebDriver(PasswordlessApiFactory.OriginUrl);
        await UserHelpers.RegisterNewUser(client, driver);
        _factory.TimeProvider.SetUtcNow(serverTime.AddDays(31));

        // Act
        await UserHelpers.SignInUser(client, driver);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var tokenKeys = await scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>().Create(applicationName).GetTokenKeys();
        tokenKeys.Should().NotBeNull();
        tokenKeys.Any(x => x.CreatedAt < (DateTime.UtcNow.AddDays(-30))).Should().BeFalse();
        tokenKeys.Any(x => x.CreatedAt >= (DateTime.UtcNow.AddDays(-30))).Should().BeTrue();
    }
}