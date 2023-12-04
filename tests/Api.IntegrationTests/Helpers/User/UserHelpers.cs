using System.Net.Http.Json;
using Bogus;
using Fido2NetLib;
using Passwordless.Api.Endpoints;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.User;

public static class UserHelpers
{
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
    
    const string OriginUrl = "https://bitwarden.com/products/passwordless/";
    const string RpId = "bitwarden.com";
    
    public static async Task<HttpResponseMessage> RegisterNewUser(HttpClient httpClient)
    {
        if (!httpClient.HasPublicKey()) throw new Exception("ApiKey was not provided. Please add ApiKey to headers.");
        if (!httpClient.HasSecretKey()) throw new Exception("ApiSecret was not provided. Please add ApiSecret to headers.");
        
        var tokenRequest = TokenGenerator.Generate();
        using var tokenResponse = await httpClient.PostAsJsonAsync("/register/token", tokenRequest);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = OriginUrl,
            RPID = RpId
        };
        var registrationBeginResponse = await httpClient.PostAsJsonAsync("/register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();
        using var driver = WebDriverFactory.GetWebDriver(OriginUrl);
        var authenticatorAttestationRawResponse = await driver.CreateCredentialsAsync(sessionResponse!.Data);
        return await httpClient.PostAsJsonAsync("/register/complete", new RegistrationCompleteDTO
        {
            Origin = OriginUrl,
            RPID = RpId,
            Session = sessionResponse.Session,
            Response = authenticatorAttestationRawResponse
        });
    }
}