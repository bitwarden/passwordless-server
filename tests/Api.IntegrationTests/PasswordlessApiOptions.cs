using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests;

public class PasswordlessApiOptions
{
    public IReadOnlyDictionary<string, string?> Settings { get; init; } = new Dictionary<string, string?>();

    public ITestOutputHelper? TestOutput { get; init; }
}