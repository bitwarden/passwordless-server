using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Fido2NetLib;
using FluentAssertions;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Integration.Tests.Helpers;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.Integration.Tests.Endpoints.SignIn;

public class SignInTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly HttpClient _httpClient;

    const string OriginUrl = "https://bitwarden.com/products/passwordless/";
    const string RpId = "bitwarden.com";

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
        _httpClient = factory.CreateClient()
            .AddPublicKey()
            .AddSecretKey()
            .AddUserAgent();
    }

    [Fact]
    public async Task I_can_retrieve_assertion_options_to_begin_sign_in()
    {
        // Arrange
        var request = new SignInBeginDTO { Origin = OriginUrl, RPID = RpId };

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
        var tokenRequest = TokenGenerator.Generate();
        var tokenResponse = await _httpClient.PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = OriginUrl,
            RPID = RpId
        };
        var registrationBeginResponse = await _httpClient.PostAsJsonAsync("/register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var driver = WebDriverFactory.GetWebDriver(OriginUrl);
        var registerResult = driver.ExecuteScript($"{await BrowserCredentialsHelper.GetCreateCredentialFunctions()} " +
                                                  $"return await createCredential({sessionResponse!.Data.ToJson()});")?.ToString() ?? string.Empty;

        var parsedRegisterResult = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(registerResult);
        var registerCompleteResponse = await _httpClient.PostAsJsonAsync("/register/complete", new RegistrationCompleteDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
            Session = sessionResponse.Session,
            Response = parsedRegisterResult
        });
        await registerCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var signInBeginResponse = await _httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = OriginUrl, RPID = RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        var signInResult = driver.ExecuteScript($"{await BrowserCredentialsHelper.GetGetCredentialFunctions()} " +
                                                $"return await getCredential({signInBegin!.Data.ToJson()});")?.ToString() ?? string.Empty;
        var parsedSignInResult = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(signInResult);

        // Act
        var signInCompleteResponse = await _httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
            Response = parsedSignInResult,
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
        var tokenRequest = TokenGenerator.Generate();
        var tokenResponse = await _httpClient.PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = OriginUrl,
            RPID = RpId
        };
        var registrationBeginResponse = await _httpClient.PostAsJsonAsync("/register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var driver = WebDriverFactory.GetWebDriver(OriginUrl);

        var registerResult = driver.ExecuteScript($"{await BrowserCredentialsHelper.GetCreateCredentialFunctions()} " +
                                                  $"return await createCredential({sessionResponse!.Data.ToJson()});")?.ToString() ?? string.Empty;

        var parsedRegisterResult = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(registerResult);
        var registerCompleteResponse = await _httpClient.PostAsJsonAsync("/register/complete", new RegistrationCompleteDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
            Session = sessionResponse.Session,
            Response = parsedRegisterResult
        });
        await registerCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var signInBeginResponse = await _httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = OriginUrl, RPID = RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        var signInResult = driver.ExecuteScript($"{await BrowserCredentialsHelper.GetGetCredentialFunctions()} " +
                                                $"return await getCredential({signInBegin!.Data.ToJson()});")?.ToString() ?? string.Empty;
        var parsedSignInResult = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(signInResult);

        // Act
        var signInCompleteResponse = await _httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
            Response = parsedSignInResult,
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
    public async Task UnknownCredentialThrows()
    {
        var options = await _httpClient.PostAsJsonAsync("/signin/begin", new { Origin = "https://localhost", RPID = "localhost" });

        var response = await options.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        var payload = new
        {
            Origin = "https://localhost",
            RPID = "localhost",
            Session = response.Session,
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

        var result = await _httpClient.PostAsJsonAsync("/signin/complete", payload);
        var body = await result.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        AssertHelper.AssertEqualJson("""
                                     {
                                       "type": "https://docs.passwordless.dev/guide/errors.html#unknown_credential",
                                       "title": "We don't recognize the passkey you sent us.",
                                       "status": 400,
                                       "credentialId": "LcVLKA2QkfwzvuSTxIIyFVTJ9IopE57xTYvJ_0Nx9nk",
                                       "errorCode": "unknown_credential"
                                     }
                                     """, body);
    }
}