using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Fido2NetLib;
using Fido2NetLib.Cbor;
using Fido2NetLib.Objects;
using FluentAssertions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.VirtualAuth;
using Passwordless.Api.Endpoints;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.Integration.Tests;

public class RegisterTests : IClassFixture<PasswordlessApiFactory>
{
    private readonly PasswordlessApiFactory _apiFactory;
    private readonly HttpClient _client;

    private static readonly Faker<RegisterToken> _tokenGenerator = new Faker<RegisterToken>()
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
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
    }

    [Fact]
    public async Task Server_Can_Successfully_Retrieve_Token_For_Registration()
    {
        var request = _tokenGenerator.Generate();

        var response = await _client.AddSecretKey().PostAsJsonAsync("/register/token", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerTokenResponse = await response.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();
        registerTokenResponse.Should().NotBeNull();
        registerTokenResponse!.Token.Should().StartWith("register_");
    }

    [Fact]
    public async Task Client_Can_Begin_Registration_Of_User_Credential_With_Server()
    {
        var tokenRequest = _tokenGenerator.Generate();
        var tokenResponse = await _client.AddSecretKey().PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = "https://integration-tests.passwordless.dev",
            RPID = Environment.MachineName
        };

        var registrationBeginResponse = await _client.AddPublicKey().PostAsJsonAsync("/register/begin", registrationBeginRequest);

        registrationBeginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();
        sessionResponse.Should().NotBeNull();
        sessionResponse!.Data.User.DisplayName.Should().Be(tokenRequest.DisplayName);
        sessionResponse.Data.User.Name.Should().Be(tokenRequest.Username);

        //TODO might want to assert more here
    }

    [Fact]
    public async Task Client_Can_Complete_Registration_Of_User_Credential_With_Passkey_To_Server()
    {
        var tokenRequest = _tokenGenerator.Generate();
        var tokenResponse = await _client.AddSecretKey().PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = "https://integration-tests.passwordless.dev",
            RPID = Environment.MachineName
        };
        var registrationBeginResponse = await _client
            .AddPublicKey()
            .PostAsJsonAsync("/register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var virtualAuth = new VirtualAuthenticatorOptions()
            .SetIsUserVerified(true)
            .SetHasUserVerification(true)
            .SetIsUserConsenting(true)
            .SetTransport(VirtualAuthenticatorOptions.Transport.USB)
            .SetProtocol(VirtualAuthenticatorOptions.Protocol.U2F)
            .SetHasResidentKey(true);

        var driver = new ChromeDriver();
        driver.AddVirtualAuthenticator(virtualAuth);

        var response = driver.ExecuteScript($"await navigator.credentials.create({{ publicKey: {sessionResponse!.Data.ToJson()} }})");

        var registerCompleteResponse = await _client.PostAsJsonAsync("/register/complete",
            new RegistrationCompleteDTO
            {
                Origin = registrationBeginRequest.Origin,
                RPID = registrationBeginRequest.RPID,
                Session = sessionResponse.Session,
                Response = new AuthenticatorAttestationRawResponse
                {
                    Type = PublicKeyCredentialType.PublicKey,
                    Id = new byte[] { 0xf1, 0xd0 },
                    RawId = new byte[] { 0xf1, 0xd0 },
                    Response = new AuthenticatorAttestationRawResponse.ResponseData
                    {
                        AttestationObject = new CborMap {
                            { "fmt", "none" },
                            { "attStmt", new CborMap() },
                            //{ "authData", authData }
                        }.Encode(),
                        ClientDataJson = JsonSerializer.SerializeToUtf8Bytes(new
                        {
                            type = "webauthn.create",
                            challenge = sessionResponse.Data.Challenge,
                            origin = registrationBeginRequest.Origin
                        })
                    },
                }
            });

        registerCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerCompleteToken = await registerCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        registerCompleteToken!.Token.Should().StartWith("register_");
    }
}