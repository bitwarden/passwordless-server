using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Fido2NetLib;
using FluentAssertions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.VirtualAuth;
using Passwordless.Api.Endpoints;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.Integration.Tests;

public class RegisterTests : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client;

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

    public RegisterTests(PasswordlessApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
    }

    [Fact]
    public async Task Server_can_successfully_retrieve_token_for_registration()
    {
        // Arrange
        var request = TokenGenerator.Generate();

        // Act
        var response = await _client.AddSecretKey().PostAsJsonAsync("/register/token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerTokenResponse = await response.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();
        registerTokenResponse.Should().NotBeNull();
        registerTokenResponse!.Token.Should().StartWith("register_");
    }

    [Fact]
    public async Task Client_Can_Begin_Registration_Of_User_Credential_With_Server()
    {
        // Arrange
        var tokenRequest = TokenGenerator.Generate();
        var tokenResponse = await _client.AddSecretKey().PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = "https://integration-tests.passwordless.dev",
            RPID = Environment.MachineName
        };

        // Act
        using var registrationBeginResponse = await _client.AddPublicKey().PostAsJsonAsync("/register/begin", registrationBeginRequest);

        // Assert
        registrationBeginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();
        sessionResponse.Should().NotBeNull();
        sessionResponse!.Data.User.DisplayName.Should().Be(tokenRequest.DisplayName);
        sessionResponse.Data.User.Name.Should().Be(tokenRequest.Username);
    }

    [Fact]
    public async Task Client_Can_Complete_Registration_Of_User_Credential_With_Passkey_To_Server()
    {
        // Arrange
        const string originUrl = "https://bitwarden.com/products/passwordless/";
        const string rpId = "bitwarden.com";

        var tokenRequest = TokenGenerator.Generate();
        var tokenResponse = await _client.AddSecretKey().PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO { Token = registerTokenResponse!.Token, Origin = originUrl, RPID = rpId };
        var registrationBeginResponse = await _client
            .AddPublicKey()
            .PostAsJsonAsync("/register/begin", registrationBeginRequest);

        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var virtualAuth = new VirtualAuthenticatorOptions()
            .SetIsUserVerified(true)
            .SetHasUserVerification(true)
            .SetIsUserConsenting(true)
            .SetTransport(VirtualAuthenticatorOptions.Transport.INTERNAL)
            .SetProtocol(VirtualAuthenticatorOptions.Protocol.CTAP2)
            .SetHasResidentKey(true);

        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--headless");
        var driver = new ChromeDriver(options);
        driver.Url = originUrl;
        driver.AddVirtualAuthenticator(virtualAuth);
        var result = driver.ExecuteScript(GetScript(sessionResponse!.Data.ToJson()));

        var resultString = result?.ToString() ?? string.Empty;

        var parsedResult = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(resultString);

        // Act
        var registerCompleteResponse = await _client.PostAsJsonAsync("/register/complete",
            new RegistrationCompleteDTO { Origin = originUrl, RPID = rpId, Session = sessionResponse.Session, Response = parsedResult });

        // Assert
        registerCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerCompleteToken = await registerCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        registerCompleteToken!.Token.Should().StartWith("verify_");
    }

    private static string GetScript(string jsonResponse) => $$"""
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

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}