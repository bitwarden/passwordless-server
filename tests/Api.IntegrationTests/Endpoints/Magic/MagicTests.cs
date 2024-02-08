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

    [Fact]
    public async Task I_cannot_send_a_magic_link_email_to_a_non_admin_address_if_the_application_is_too_new()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName, "definitely-not@what-faker-will.generate");
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(appCreated!.ApiSecret1);
        await _client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_can_send_a_magic_link_email_to_an_admin_address_if_the_application_even_if_the_application_is_too_new()
    {
        // Arrange
        const string emailAddress = "admin@email.com";
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName, emailAddress);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _client.AddSecretKey(appCreated!.ApiSecret1);
        await _client.EnableMagicLinks("a_user");
        var request = _requestFaker
            .RuleFor(x => x.EmailAddress, emailAddress)
            .Generate();

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_cannot_send_too_many_magic_link_emails_in_a_minute()
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
        var responseStatusCodes = await Task.WhenAll(
            Enumerable.Range(0, 100).Select(async _ =>
                {
                    using var response = await _client.PostAsJsonAsync("magic-link/send", request);
                    return response.StatusCode;
                }
            )
        );

        // Assert
        responseStatusCodes.Should().Contain(HttpStatusCode.NoContent);
        responseStatusCodes.Should().Contain(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task I_cannot_send_too_many_magic_link_emails_in_a_month()
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
        var responseStatusCodes = await Task.WhenAll(
            Enumerable.Range(0, 5000).Select(async _ =>
                {
                    using var response = await _client.PostAsJsonAsync("magic-link/send", request);
                    apiFactory.TimeProvider.Advance(TimeSpan.FromMinutes(1));
                    return response.StatusCode;
                }
            )
        );

        // Assert
        responseStatusCodes.Should().Contain(HttpStatusCode.NoContent);
        responseStatusCodes.Should().Contain(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task I_can_send_a_magic_link_email_after_enough_time_passed_since_the_quota_was_exceeded()
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

        await Task.WhenAll(
            Enumerable.Range(0, 5000).Select(async _ =>
                {
                    using var response = await _client.PostAsJsonAsync("magic-link/send", request);
                    apiFactory.TimeProvider.Advance(TimeSpan.FromMinutes(1));
                    return response.StatusCode;
                }
            )
        );

        apiFactory.TimeProvider.Advance(TimeSpan.FromDays(30));

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}