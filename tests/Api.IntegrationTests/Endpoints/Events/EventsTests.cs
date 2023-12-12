using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Passwordless.Api.Endpoints;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Constants;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Service.Models;
using Xunit;

namespace Passwordless.Api.IntegrationTests.Endpoints.Events;

public class EventsTests(PasswordlessApiFactory passwordlessApiFactory) : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client = passwordlessApiFactory.CreateClient();

    [Fact]
    public async Task I_can_view_the_event_for_a_user_retrieving_the_api_keys()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var createApplicationMessage = await _client.CreateApplication(applicationName);
        var accountKeysCreation = await createApplicationMessage.Content.ReadFromJsonAsync<AccountKeysCreation>();
        _client.AddSecretKey(accountKeysCreation!.ApiSecret1);
        await _client.EnableEventLogging(applicationName);
        _ = await _client.GetAsync($"/admin/apps/{applicationName}/api-keys");

        // Act
        using var getApplicationEventsResponse = await _client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeysEnumerated.ToString());
    }

    [Fact]
    public async Task I_can_view_the_event_for_a_user_creating_an_api_key()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        _ = await _client.CreateApplication(applicationName);
        await _client.EnableEventLogging(applicationName);
        using var createApiKeyResponse = await _client.PostAsJsonAsync($"/admin/apps/{applicationName}/api-keys",
            new CreateApiKeyDto(ApiKeyTypes.Secret, ApiKeyScopes.SecretScopes.ToHashSet()));
        var createApiKey = await createApiKeyResponse.Content.ReadFromJsonAsync<CreateApiKeyResultDto>();
        _client.AddSecretKey(createApiKey!.ApiKey);

        // Act
        using var getApplicationEventsResponse = await _client.GetAsync("events?pageNumber=1");

        // Assert
        getApplicationEventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationEvents = await getApplicationEventsResponse.Content.ReadFromJsonAsync<EventLog.GetEventLogEventsResponse>();
        applicationEvents.Should().NotBeNull();
        applicationEvents!.Events.Should().NotBeEmpty();
        applicationEvents.Events.Should().Contain(x => x.EventType == EventType.AdminApiKeyCreated.ToString());

    }

    public void Dispose()
    {
        _client.Dispose();
    }
}