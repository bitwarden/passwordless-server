using System.Net.Http.Json;
using Bogus;
using Fido2NetLib;
using OpenQA.Selenium;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Infra;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.User;

public static class UserHelpers
{
    public static readonly Faker<RegisterToken> TokenGenerator = new Faker<RegisterToken>()
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

    public static Task<HttpResponseMessage> RegisterNewUser(this HttpClient httpClient, WebDriver driver) =>
        httpClient.RegisterNewUser(driver, TokenGenerator.Generate());

    public static async Task<HttpResponseMessage> RegisterNewUser(this HttpClient httpClient, WebDriver driver, RegisterToken registerToken)
    {
        if (!httpClient.HasPublicKey()) throw new Exception("ApiKey was not provided. Please add ApiKey to headers.");
        if (!httpClient.HasSecretKey()) throw new Exception("ApiSecret was not provided. Please add ApiSecret to headers.");

        using var tokenResponse = await httpClient.PostAsJsonAsync("/register/token", registerToken);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = TestApi.OriginUrl,
            RPID = TestApi.RpId
        };
        using var registrationBeginResponse = await httpClient.PostAsJsonAsync("/register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var authenticatorAttestationRawResponse = await driver.CreateCredentialsAsync(sessionResponse!.Data);
        return await httpClient.PostAsJsonAsync("/register/complete", new RegistrationCompleteDTO
        {
            Origin = TestApi.OriginUrl,
            RPID = TestApi.RpId,
            Session = sessionResponse.Session,
            Response = authenticatorAttestationRawResponse
        });
    }

    public static async Task<HttpResponseMessage> SignInUser(this HttpClient httpClient, WebDriver driver)
    {
        if (!httpClient.HasPublicKey()) throw new Exception("ApiKey was not provided. Please add ApiKey to headers.");
        if (!httpClient.HasSecretKey()) throw new Exception("ApiSecret was not provided. Please add ApiSecret to headers.");

        using var signInBeginResponse = await httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = TestApi.OriginUrl, RPID = TestApi.RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<AssertionOptions>>();

        var authenticatorAssertionRawResponse = await driver.GetCredentialsAsync(signInBegin!.Data);

        return await httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = TestApi.OriginUrl,
            RPID = TestApi.RpId,
            Response = authenticatorAssertionRawResponse,
            Session = signInBegin.Session
        });
    }
}