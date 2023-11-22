using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Fido2NetLib;
using FluentAssertions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.VirtualAuth;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Integration.Tests.Helpers;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.Integration.Tests.Endpoints.Register;

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
    public async Task I_can_retrieve_token_to_start_registration()
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
    public async Task I_can_retrieve_the_credential_create_options_and_session_token_for_creating_a_new_user()
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
    public async Task I_can_use_a_passkey_to_register_a_new_user()
    {
        // Arrange
        const string originUrl = "https://bitwarden.com/products/passwordless/";
        const string rpId = "bitwarden.com";

        var tokenRequest = TokenGenerator.Generate();
        using var tokenResponse = await _client.AddSecretKey().PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO { Token = registerTokenResponse!.Token, Origin = originUrl, RPID = rpId };
        using var registrationBeginResponse = await _client
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

        var result = driver.ExecuteScript($"{await BrowserCredentialsHelper.GetCreateCredentialFunctions()} " +
                                          $"return await createCredential({sessionResponse!.Data.ToJson()});");

        var resultString = result?.ToString() ?? string.Empty;

        var parsedResult = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(resultString);

        // Act
        using var registerCompleteResponse = await _client.PostAsJsonAsync("/register/complete",
            new RegistrationCompleteDTO { Origin = originUrl, RPID = rpId, Session = sessionResponse.Session, Response = parsedResult });

        // Assert
        registerCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerCompleteToken = await registerCompleteResponse.Content.ReadFromJsonAsync<TokenResponse>();
        registerCompleteToken!.Token.Should().StartWith("verify_");
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}