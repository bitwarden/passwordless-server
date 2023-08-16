using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.IntegrationTests;

[Collection("AccountTestes")]
public class AppTests : BackendTests
{
    private readonly HttpClient _client;

    public AppTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
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

    [Fact]
    public async Task CreateAccountWithAppFeatures()
    {
        var name = $"app{Guid.NewGuid():N}";
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

        Assert.True(res.IsSuccessStatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>();
            var storage = factory.Create(name);
            var info = await storage.GetAppFeaturesAsync();
            Assert.Equal(info.Tenant, name);
            Assert.False(info.AuditLoggingIsEnabled);
            Assert.Equal(365, info.AuditLoggingRetentionPeriod);
            Assert.Null(info.DeveloperLoggingEndsAt);
        }
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task SetFeaturesAsync_Modifies_Features()
    {
        var app = await CreateAppAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/apps/features")
        {
            Content = JsonContent.Create(new SetFeaturesDto
            {
                AuditLoggingIsEnabled = true,
                AuditLoggingRetentionPeriod = 30
            })
        };
        request.Headers.Add("ApiSecret", app.Result.ApiSecret1);
        var res = await _client.SendAsync(request);

        Assert.True(res.IsSuccessStatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>();
            var storage = factory.Create(app.AppId);
            var info = await storage.GetAppFeaturesAsync();
            Assert.Equal(info.Tenant, app.AppId);
            Assert.True(info.AuditLoggingIsEnabled);
            Assert.Equal(30, info.AuditLoggingRetentionPeriod);
            Assert.Null(info.DeveloperLoggingEndsAt);
        }
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
    }
}