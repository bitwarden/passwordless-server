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

namespace Passwordless.Api.Integration.Tests;

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
            .AddSecretKey();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
    }

    [Fact]
    public async Task Server_returns_encoded_assertion_options_to_be_used_for_sign_in()
    {
        var request = new SignInBeginDTO { Origin = OriginUrl, RPID = RpId };

        var response = await _httpClient.PostAsJsonAsync("/signin/begin", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInResponse = await response.Content.ReadFromJsonAsync<SessionResponse<Fido2NetLib.AssertionOptions>>();

        signInResponse.Should().NotBeNull();
        signInResponse!.Session.Should().StartWith("session_");
        signInResponse.Data.RpId.Should().Be(request.RPID);
        signInResponse.Data.Status.Should().Be("ok");
    }

    [Fact]
    public async Task Client_can_call_sign_in_complete_with_passkey()
    {
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
        var registerResult = driver.ExecuteScript(GetRegisterScript(sessionResponse!.Data.ToJson()))?.ToString() ?? string.Empty;
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

        var signInResult = driver.ExecuteScript(GetSignInScript(signInBegin!.Data.ToJson()))?.ToString() ?? string.Empty;
        var parsedSignInResult = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(signInResult);

        var signInCompleteResponse = await _httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
            Response = parsedSignInResult,
            Session = signInBegin.Session
        });

        signInCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInTokenResponse = await signInCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        signInTokenResponse.Should().NotBeNull();
        signInTokenResponse!.Token.Should().StartWith("verify_");
    }

    [Fact]
    public async Task Client_can_call_verify_after_signing_in_to_ensure_token_is_valid_from_server()
    {
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
        var registerResult = driver.ExecuteScript(GetRegisterScript(sessionResponse!.Data.ToJson()))?.ToString() ?? string.Empty;
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

        var signInResult = driver.ExecuteScript(GetSignInScript(signInBegin!.Data.ToJson()))?.ToString() ?? string.Empty;
        var parsedSignInResult = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(signInResult);

        var signInCompleteResponse = await _httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
            Response = parsedSignInResult,
            Session = signInBegin.Session
        });

        signInCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var signInTokenResponse = await signInCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        signInTokenResponse.Should().NotBeNull();
        signInTokenResponse!.Token.Should().StartWith("verify_");

        var verifySignInResponse = await _httpClient.PostAsJsonAsync("/signin/verify", new SignInVerifyDTO { Token = signInTokenResponse.Token });
        verifySignInResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static string GetSignInScript(string jsonResponse) => $$"""
        let signin = {{jsonResponse}};
        
        signin.challenge = base64UrlToArrayBuffer(signin.challenge);
        signin.allowCredentials?.forEach((cred) => {
            cred.id = base64UrlToArrayBuffer(cred.id);
        });
        
        const credential = await navigator.credentials.get({
            publicKey: signin
        });
        
        return JSON.stringify({
            id: credential.id,
            rawId: arrayBufferToBase64Url(new Uint8Array(credential.rawId)),
            type: credential.type,
            extensions: credential.getClientExtensionResults(),
            response: {
                authenticatorData: arrayBufferToBase64Url(credential.response.authenticatorData),
                clientDataJSON: arrayBufferToBase64Url(credential.response.clientDataJSON),
                signature: arrayBufferToBase64Url(credential.response.signature)
            }
        });
        
        function base64UrlToArrayBuffer(base64UrlString) {
            // improvement: Remove BufferSource-type and add proper types upstream
            if (typeof base64UrlString !== 'string') {
                const msg = "Cannot convert from Base64Url to ArrayBuffer: Input was not of type string";
                console.error(msg, base64UrlString);
                throw new TypeError(msg);
            }
        
            const base64Unpadded = base64UrlToBase64(base64UrlString);
            const paddingNeeded = (4 - (base64Unpadded.length % 4)) % 4;
            const base64Padded = base64Unpadded.padEnd(base64Unpadded.length + paddingNeeded, "=");
        
            const binary = window.atob(base64Padded);
            const bytes = new Uint8Array(binary.length);
            for (let i = 0; i < binary.length; i++) {
                bytes[i] = binary.charCodeAt(i);
            }
        
            return bytes;
        }
        
        function base64UrlToBase64(base64Url) {
            return base64Url.replace(/-/g, '+').replace(/_/g, '/');
        }
        
        function base64ToBase64Url(base64) {
            return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=*$/g, '');
        }
        
        function arrayBufferToBase64Url(buffer) {
            const uint8Array = (() => {
                if (Array.isArray(buffer)) return Uint8Array.from(buffer);
                if (buffer instanceof ArrayBuffer) return new Uint8Array(buffer);
                if (buffer instanceof Uint8Array) return buffer;
        
                const msg = "Cannot convert from ArrayBuffer to Base64Url. Input was not of type ArrayBuffer, Uint8Array or Array";
                console.error(msg, buffer);
                throw new Error(msg);
            })();
        
            let string = '';
            for (let i = 0; i < uint8Array.byteLength; i++) {
                string += String.fromCharCode(uint8Array[i]);
            }
        
            const base64String = window.btoa(string);
            return base64ToBase64Url(base64String);
        }
        """;

    private static string GetRegisterScript(string jsonResponse) => $$"""
       let registration = {{jsonResponse}};
       
       registration.challenge = base64UrlToArrayBuffer(registration.challenge);
       registration.user.id = base64UrlToArrayBuffer(registration.user.id);
       registration.excludeCredentials?.forEach((cred) => {
           cred.id = base64UrlToArrayBuffer(cred.id);
       });
       
       const result = await navigator.credentials.create({
           publicKey: registration,
       });
       
       const credential = result;
       const attestationResponse = credential.response;
       
       return JSON.stringify({
               id: credential.id,
               rawId: arrayBufferToBase64Url(credential.rawId),
               type: credential.type,
               extensions: result.getClientExtensionResults(),
               response: {
                   attestationObject: arrayBufferToBase64Url(attestationResponse.attestationObject),
                   clientDataJSON: arrayBufferToBase64Url(attestationResponse.clientDataJSON),
               }
           });
       
       function base64UrlToArrayBuffer(base64UrlString) {
           // improvement: Remove BufferSource-type and add proper types upstream
           if (typeof base64UrlString !== 'string') {
               const msg = "Cannot convert from Base64Url to ArrayBuffer: Input was not of type string";
               console.error(msg, base64UrlString);
               throw new TypeError(msg);
           }
       
           const base64Unpadded = base64UrlToBase64(base64UrlString);
           const paddingNeeded = (4 - (base64Unpadded.length % 4)) % 4;
           const base64Padded = base64Unpadded.padEnd(base64Unpadded.length + paddingNeeded, "=");
       
           const binary = window.atob(base64Padded);
           const bytes = new Uint8Array(binary.length);
           for (let i = 0; i < binary.length; i++) {
               bytes[i] = binary.charCodeAt(i);
           }
       
           return bytes;
       }
       
       function base64UrlToBase64(base64Url) {
           return base64Url.replace(/-/g, '+').replace(/_/g, '/');
       }
       
       function base64ToBase64Url(base64) {
           return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=*$/g, '');
       }
       
       function arrayBufferToBase64Url(buffer) {
           const uint8Array = (() => {
               if (Array.isArray(buffer)) return Uint8Array.from(buffer);
               if (buffer instanceof ArrayBuffer) return new Uint8Array(buffer);
               if (buffer instanceof Uint8Array) return buffer;
       
               const msg = "Cannot convert from ArrayBuffer to Base64Url. Input was not of type ArrayBuffer, Uint8Array or Array";
               console.error(msg, buffer);
               throw new Error(msg);
           })();
       
           let string = '';
           for (let i = 0; i < uint8Array.byteLength; i++) {
               string += String.fromCharCode(uint8Array[i]);
           }
       
           const base64String = window.btoa(string);
           return base64ToBase64Url(base64String);
       }
       """;
}