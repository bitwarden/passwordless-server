using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.IntegrationTests;

[Collection("Credentials")]
public class CredentialsTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CredentialsTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CredentialsList()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>();
        var context = _factory.CreateDbContext(scope, "test");

        context.ApiKeys.Add(new ApiKeyDesc()
        {
            Id = "cdb5",
            ApiKey = "Ojo30xHbjABSZ65iQfpecA==:jmqnuC9B/tSTAr+B9Ow+fA==",
            Scopes = new[] { "token_register", "token_verify" },
            IsLocked = false,
            Tenant = "test",
        });

        context.Credentials.Add(new EFStoredCredential()
        {
            DescriptorId = "test"u8.ToArray(),
            UserHandle = "1"u8.ToArray(),
            Tenant = "test",
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/credentials/list?userId=1");
        request.Headers.Add("ApiSecret", "test:secret:d1a64120edda4006ba8558b02d43cdb5");
        var httpResponse = await _client.SendAsync(request);
        using var credentialsResponse = await httpResponse.Content.ReadFromJsonAsync<JsonDocument>();
        // Assert
        Assert.NotNull(credentialsResponse);
        var credentials = credentialsResponse.RootElement.GetProperty("values").Deserialize<StoredCredential[]>();
        Assert.NotNull(credentials);
        Assert.Single(credentials);
    }
}