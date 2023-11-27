using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Api.Integration.Tests.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;
using Xunit;

namespace Passwordless.Api.Integration.Tests.Endpoints.App;

public class AppTests : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly PasswordlessApiFactory _factory;

    private static readonly Faker<AppCreateDTO> AppCreateGenerator = new Faker<AppCreateDTO>()
        .RuleFor(x => x.AdminEmail, x => x.Person.Email);

    public AppTests(PasswordlessApiFactory apiFactory)
    {
        _factory = apiFactory;
        _client = apiFactory.CreateClient()
            .AddManagementKey();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("1")]
    public async Task I_cannot_create_an_account_with_an_invalid_name(string name)
    {
        // Arrange
        var request = AppCreateGenerator.Generate();

        // Act
        var response = await _client.PostAsJsonAsync($"/admin/apps/{name}/create", request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        content.Should().NotBeNull();
        content!.Title.Should().Be("accountName needs to be alphanumeric and start with a letter");
    }

    [Fact]
    public async Task I_can_create_an_account_with_a_valid_name()
    {
        // Arrange
        const string accountName = "anders";
        var request = AppCreateGenerator.Generate();

        // Act
        var response = await _client.PostAsJsonAsync($"/admin/apps/{accountName}/create", request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AccountKeysCreation>();
        content.Should().NotBeNull();
        content!.Message.Should().Be("Store keys safely. They will only be shown to you once.");
        content.ApiKey1.Should().NotBeNullOrWhiteSpace();
        content.ApiKey2.Should().NotBeNullOrWhiteSpace();
        content.ApiSecret1.Should().NotBeNullOrWhiteSpace();
        content.ApiSecret2.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task I_can_create_an_app_and_its_features_will_be_set_correctly()
    {
        // Arrange
        var name = $"app{Guid.NewGuid():N}";
        var request = AppCreateGenerator.Generate();

        // Act
        var res = await _client.PostAsJsonAsync($"/admin/apps/{name}/create", request);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();

        var appFeature = await scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>()
            .Create(name)
            .GetAppFeaturesAsync();

        appFeature.Should().NotBeNull();
        appFeature!.Tenant.Should().Be(name);
        appFeature.EventLoggingIsEnabled.Should().BeFalse();
        appFeature.EventLoggingRetentionPeriod.Should().Be(365);
        appFeature.DeveloperLoggingEndsAt.Should().BeNull();
    }

    [Fact]
    public async Task I_can_set_event_logging_retention_period()
    {
        // Arrange
        const int expectedEventLoggingRetentionPeriod = 30;
        
        var name = $"app{Guid.NewGuid():N}";
        var appCreateRequest = AppCreateGenerator.Generate();
        var appCreateResponse = await _client.PostAsJsonAsync($"/admin/apps/{name}/create", appCreateRequest);
        var appCreateDto = await appCreateResponse.Content.ReadFromJsonAsync<AccountKeysCreation>();
        var setFeatureRequest = new SetFeaturesDto { EventLoggingRetentionPeriod = expectedEventLoggingRetentionPeriod };
        using var appHttpClient = _factory.CreateClient().AddSecretKey(appCreateDto!.ApiSecret1);
        
        // Act
        var setFeatureResponse = await appHttpClient.PostAsJsonAsync("/apps/features", setFeatureRequest);
        
        // Assert
        setFeatureResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();

        var appFeature = await scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>().Create(name).GetAppFeaturesAsync();
        appFeature.Should().NotBeNull();
        appFeature!.EventLoggingRetentionPeriod.Should().Be(expectedEventLoggingRetentionPeriod);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(91)]
    public async Task I_can_not_set_the_event_logging_retention_period_to_an_invalid_value(int invalidRetentionPeriod)
    {
        // Arrange
        var name = $"app{Guid.NewGuid():N}";
        var appCreateRequest = AppCreateGenerator.Generate();
        var appCreateResponse = await _client.PostAsJsonAsync($"/admin/apps/{name}/create", appCreateRequest);
        var appCreateDto = await appCreateResponse.Content.ReadFromJsonAsync<AccountKeysCreation>();
        var setFeatureRequest = new SetFeaturesDto { EventLoggingRetentionPeriod = invalidRetentionPeriod };
        using var appHttpClient = _factory.CreateClient().AddSecretKey(appCreateDto!.ApiSecret1);

        // Act
        var setFeatureResponse = await appHttpClient.PostAsJsonAsync("/apps/features", setFeatureRequest);

        // Assert
        setFeatureResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await setFeatureResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
    }

    [Fact]
    public async Task I_can_manage_an_apps_features()
    {
        // Arrange
        const int expectedEventLoggingRetentionPeriod = 30;
        
        var name = $"app{Guid.NewGuid():N}";
        var appCreateRequest = AppCreateGenerator.Generate();
        var appCreateResponse = await _client.PostAsJsonAsync($"/admin/apps/{name}/create", appCreateRequest);
        _ = await appCreateResponse.Content.ReadFromJsonAsync<AccountKeysCreation>();
        var manageFeatureRequest = new ManageFeaturesDto { EventLoggingRetentionPeriod = expectedEventLoggingRetentionPeriod, EventLoggingIsEnabled = true };
        
        // Act
        var manageFeatureResponse = await _client.PostAsJsonAsync($"/admin/apps/{name}/features", manageFeatureRequest);
        
        // Assert
        manageFeatureResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();

        var appFeature = await scope.ServiceProvider.GetRequiredService<ITenantStorageFactory>().Create(name).GetAppFeaturesAsync();
        appFeature.Should().NotBeNull();
        appFeature!.EventLoggingRetentionPeriod.Should().Be(expectedEventLoggingRetentionPeriod);
        appFeature.EventLoggingIsEnabled.Should().BeTrue();
        appFeature.DeveloperLoggingEndsAt.Should().BeNull();
    }

    [Fact]
    public async Task I_can_get_an_apps_features()
    {
        // Arrange
        const int expectedEventLoggingRetentionPeriod = 30;
        
        var name = $"app{Guid.NewGuid():N}";
        var appCreateRequest = AppCreateGenerator.Generate();
        var appCreateResponse = await _client.PostAsJsonAsync($"/admin/apps/{name}/create", appCreateRequest);
        _ = await appCreateResponse.Content.ReadFromJsonAsync<AccountKeysCreation>();
        var manageAppFeatureRequest = new ManageFeaturesDto { EventLoggingRetentionPeriod = expectedEventLoggingRetentionPeriod, EventLoggingIsEnabled = true };
        _ = await _client.PostAsJsonAsync($"/admin/apps/{name}/features", manageAppFeatureRequest);
        
        // Act
        var getAppFeatureResponse = await _client.GetAsync($"/admin/apps/{name}/features");
        
        //Assert
        getAppFeatureResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var appFeature = await getAppFeatureResponse.Content.ReadFromJsonAsync<AppFeatureDto>();
        appFeature.Should().NotBeNull();
        appFeature!.EventLoggingRetentionPeriod.Should().Be(expectedEventLoggingRetentionPeriod);
        appFeature.EventLoggingIsEnabled.Should().BeTrue();
        appFeature.DeveloperLoggingEndsAt.Should().BeNull();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}