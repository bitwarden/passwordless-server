using System.Text.Json;
using Fido2NetLib;
using OpenQA.Selenium;

namespace Passwordless.Api.IntegrationTests.Helpers;

public static class BrowserCredentialsHelper
{
    private const string ConvertersFile = "Helpers/Js/converters.js";
    private const string CreateCredentialsFile = "Helpers/Js/createCredential.js";
    private const string GetCredentialFile = "Helpers/Js/getCredential.js";

    private static Task<string> ReadConvertersFileAsync() => File.ReadAllTextAsync(ConvertersFile);
    private static Task<string> ReadCreateCredentialFileAsync() => File.ReadAllTextAsync(CreateCredentialsFile);
    private static Task<string> ReadGetCredentialFileAsync() => File.ReadAllTextAsync(GetCredentialFile);

    private async static Task<string> GetCreateCredentialFunctions()
    {
        var tasks = new[] { ReadConvertersFileAsync(), ReadCreateCredentialFileAsync() };
        await Task.WhenAll(tasks);

        var result = tasks.Aggregate(string.Empty, (functions, task) => string.Concat(functions, task.Result));
        return result;
    }

    public async static Task<AuthenticatorAttestationRawResponse> CreateCredentialsAsync(CredentialCreateOptions options, string originUrl)
    {
        using var driver = WebDriverFactory.GetWebDriver(originUrl);
            
        return await driver.CreateCredentialsAsync(options);
    }

    public async static Task<AuthenticatorAttestationRawResponse> CreateCredentialsAsync(this IJavaScriptExecutor webDriver, CredentialCreateOptions options) =>
        JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(
            (webDriver.ExecuteScript($"{await GetCreateCredentialFunctions()} return await createCredential({options.ToJson()});").ToString()
             ?? string.Empty))!;

    private async static Task<string> GetGetCredentialFunctions()
    {
        var tasks = new[] { ReadConvertersFileAsync(), ReadGetCredentialFileAsync() };
        await Task.WhenAll(tasks);

        return tasks.Aggregate(string.Empty, (functions, task) => string.Concat(functions, task.Result));
    }

    public async static Task<AuthenticatorAssertionRawResponse> GetCredentialsAsync(this IJavaScriptExecutor webDriver, AssertionOptions options) =>
        JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(
            (webDriver.ExecuteScript($"{await GetGetCredentialFunctions()} return await getCredential({options.ToJson()});").ToString()
             ?? string.Empty))!;
}