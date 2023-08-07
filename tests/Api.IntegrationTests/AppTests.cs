using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.IntegrationTests;

[Collection("AccountTestes")]
public class AppTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AppTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("1")]
    public async Task CreateAccountWithInvalidName(string name)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/apps/create")
        {
            Content = JsonContent.Create(new
            {
                AppId = name,
                AdminEmail = "anders@passwordless.dev",
            }),
        };
        request.Headers.Add("ManagementKey", "dev_test_key");
        var res = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateAccountWithValidName()
    {
        var name = "anders";
        var request = new HttpRequestMessage(HttpMethod.Post, "/apps/create")
        {
            Content = JsonContent.Create(new
            {
                AppId = name,
                AdminEmail = "anders@passwordless.dev",
            }),
        };
        request.Headers.Add("ManagementKey", "dev_test_key");
        var res = await _client.SendAsync(request);

        using (var scope = _factory.Services.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>();
            var storage = factory.Create(name);
            var info = await storage.GetAccountInformation();
            Assert.Equal(info.Tenant, name);
        }
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}