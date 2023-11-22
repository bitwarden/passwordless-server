namespace Passwordless.Api.Integration.Tests.Helpers;

public static class BrowserCredentialsHelper
{
    private const string ConvertersFile = "Helpers/Js/converters.js";
    private const string CreateCredentialsFile = "Helpers/Js/createCredential.js";
    private const string GetCredentialFile = "Helpers/Js/getCredential.js";

    private static Task<string> ReadConvertersFileAsync() => File.ReadAllTextAsync(ConvertersFile);
    private static Task<string> ReadCreateCredentialFileAsync() => File.ReadAllTextAsync(CreateCredentialsFile);
    private static Task<string> ReadGetCredentialFileAsync() => File.ReadAllTextAsync(GetCredentialFile);

    public async static Task<string> GetCreateCredentialFunctions()
    {
        var tasks = new[] { ReadConvertersFileAsync(), ReadCreateCredentialFileAsync() };
        await Task.WhenAll(tasks);

        var result = tasks.Aggregate(string.Empty, (functions, task) => string.Concat(functions, task.Result));
        return result;
    }

    public async static Task<string> GetGetCredentialFunctions()
    {
        var tasks = new[] { ReadConvertersFileAsync(), ReadGetCredentialFileAsync() };
        await Task.WhenAll(tasks);

        return tasks.Aggregate(string.Empty, (functions, task) => string.Concat(functions, task.Result));
    }
}