using System.Net;
using System.Net.Http.Json;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests;

[Collection("AccountTestes")]
public class AppTests : BackendTests
{
    private readonly HttpClient _client;

    public AppTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
        _client = factory.CreateClient();
    }
    

    [Fact]
    public async Task GetFeaturesAsync_Returns_ExpectedResult()
    {
        var app = await CreateAppAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/admin/apps/{app.AppId}/features");
        request.Headers.Add("ManagementKey", "dev_test_key");
        var actualResponse = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, actualResponse.StatusCode);

        var actual = await actualResponse.Content.ReadFromJsonAsync<AppFeatureDto>();

        Assert.Equal(365, actual.EventLoggingRetentionPeriod);
        Assert.True(actual.EventLoggingIsEnabled);
        Assert.Null(actual.DeveloperLoggingEndsAt);
    }

    [Fact]
    public async Task ManageFeaturesAsync_Returns_BadRequest_WhenEventLoggingRetentionPeriodIsNegative()
    {
        var app = await CreateAppAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/admin/apps/{app.AppId}/features")
        {
            Content = JsonContent.Create(new SetFeaturesDto
            {
                EventLoggingRetentionPeriod = -1
            })
        };
        request.Headers.Add("ManagementKey", "dev_test_key");
        var res = await _client.SendAsync(request);

        Assert.False(res.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}