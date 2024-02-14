using System.Net.Http.Json;
using Fido2NetLib;
using OpenQA.Selenium;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.User;

public static class UserHelpers
{

    public static Task<HttpResponseMessage> RegisterNewUser(this HttpClient httpClient, WebDriver driver) =>
        httpClient.RegisterNewUser(driver, RequestHelpers.GetRegisterTokenGeneratorRules().Generate());

    public static async Task<HttpResponseMessage> RegisterNewUser(this HttpClient httpClient, WebDriver driver, RegisterToken registerToken)
    {
        if (!httpClient.HasPublicKey()) throw new Exception("ApiKey was not provided. Please add ApiKey to headers.");
        if (!httpClient.HasSecretKey()) throw new Exception("ApiSecret was not provided. Please add ApiSecret to headers.");

        using var tokenResponse = await httpClient.PostAsJsonAsync("/register/token", registerToken);
        var registerTokenResponse = await tokenResponse.Content.ReadFromJsonAsync<RegisterEndpoints.RegisterTokenResponse>();

        var registrationBeginRequest = new FidoRegistrationBeginDTO
        {
            Token = registerTokenResponse!.Token,
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId
        };
        using var registrationBeginResponse = await httpClient.PostAsJsonAsync("/register/begin", registrationBeginRequest);
        var sessionResponse = await registrationBeginResponse.Content.ReadFromJsonAsync<SessionResponse<CredentialCreateOptions>>();

        var authenticatorAttestationRawResponse = await driver.CreateCredentialsAsync(sessionResponse!.Data);
        return await httpClient.PostAsJsonAsync("/register/complete", new RegistrationCompleteDTO
        {
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId,
            Session = sessionResponse.Session,
            Response = authenticatorAttestationRawResponse
        });
    }

    public static async Task<HttpResponseMessage> SignInUser(this HttpClient httpClient, WebDriver driver)
    {
        if (!httpClient.HasPublicKey()) throw new Exception("ApiKey was not provided. Please add ApiKey to headers.");
        if (!httpClient.HasSecretKey()) throw new Exception("ApiSecret was not provided. Please add ApiSecret to headers.");

        using var signInBeginResponse = await httpClient.PostAsJsonAsync("/signin/begin", new SignInBeginDTO { Origin = PasswordlessApi.OriginUrl, RPID = PasswordlessApi.RpId });
        var signInBegin = await signInBeginResponse.Content.ReadFromJsonAsync<SessionResponse<AssertionOptions>>();

        var authenticatorAssertionRawResponse = await driver.GetCredentialsAsync(signInBegin!.Data);

        return await httpClient.PostAsJsonAsync("/signin/complete", new SignInCompleteDTO
        {
            Origin = PasswordlessApi.OriginUrl,
            RPID = PasswordlessApi.RpId,
            Response = authenticatorAssertionRawResponse,
            Session = signInBegin.Session
        });
    }
}