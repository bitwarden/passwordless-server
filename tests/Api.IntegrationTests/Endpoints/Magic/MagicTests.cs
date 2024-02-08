using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.MagicLinks.Models;
using Xunit;

namespace Passwordless.Api.IntegrationTests.Endpoints.Magic;

public class MagicTests(PasswordlessApiFactory apiFactory) : IClassFixture<PasswordlessApiFactory>, IDisposable
{
    private readonly HttpClient _client = apiFactory.CreateClient();
    private readonly Faker<SendMagicLinkRequest> _requestFaker = RequestHelpers.GetMagicLinkRequestRules();

    [Fact]
    public async Task I_can_send_a_magic_link_email()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(appCreated!.ApiSecret1);
        await _client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        apiFactory.TimeProvider.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_cannot_send_a_magic_link_email_if_the_feature_is_disabled()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(appCreated!.ApiSecret1);
        _ = await _client.DisableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        apiFactory.TimeProvider.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_receive_a_validation_error_when_the_url_does_not_contain_the_token_template()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(appCreated!.ApiSecret1);
        _ = await _client.EnableMagicLinks("a_user");
        var request = _requestFaker
            .RuleFor(x => x.UrlTemplate, faker => faker.Internet.Url())
            .Generate();

        // Skip all limitations for new applications
        apiFactory.TimeProvider.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        responseDetails.Should().NotBeNull();
        var magicLinkUrlError = responseDetails!.Errors.FirstOrDefault(x => x.Key.Equals(nameof(request.UrlTemplate), StringComparison.CurrentCultureIgnoreCase));
        magicLinkUrlError.Should().NotBeNull();
        magicLinkUrlError.Value.Should().Contain($"You have provided a {nameof(request.UrlTemplate)} without a <token> template. Please include it like so: https://www.example.com?token=<token>");
    }

    [Fact]
    public async Task I_receive_a_validation_error_when_an_invalid_url_is_sent()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(appCreated!.ApiSecret1);
        await _client.EnableMagicLinks("a_user");
        var request = _requestFaker
            .RuleFor(x => x.UrlTemplate, () => "<token>")
            .Generate();

        // Skip all limitations for new applications
        apiFactory.TimeProvider.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        responseDetails.Should().NotBeNull();
        var magicLinkUrlError = responseDetails!.Errors.FirstOrDefault(x => x.Key.Equals(nameof(request.UrlTemplate), StringComparison.CurrentCultureIgnoreCase));
        magicLinkUrlError.Should().NotBeNull();
        magicLinkUrlError.Value.Should().Contain($"You have provided a {nameof(request.UrlTemplate)} that cannot be converted to a URL.");
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}