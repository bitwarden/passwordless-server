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
    private readonly Faker<SendMagicLinkRequest> _requestFaker = new Faker<SendMagicLinkRequest>()
        .RuleFor(x => x.UserId, () => Guid.NewGuid().ToString())
        .RuleFor(x => x.UserEmail, faker => faker.Person.Email)
        .RuleFor(x => x.MagicLinkUrl, faker => $"{faker.Internet.Url()}?token=<token>");

    [Fact]
    public async Task I_can_send_a_magic_link_email()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = _client.EnableManuallyGenerateAccessTokenEndpoint(applicationName, "a_user");
        _client.AddSecretKey(appCreated!.ApiSecret1);
        var request = _requestFaker.Generate();

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var magicLinkResponse = response.Content.ReadFromJsonAsync<SendMagicLinkResponse>();
        magicLinkResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task I_cannot_send_a_magic_link_email_if_the_feature_is_disabled()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = _client.DisableManuallyGenerateAccessTokenEndpoint(applicationName, "a_user");
        _client.AddSecretKey(appCreated!.ApiSecret1);
        var request = _requestFaker.Generate();

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
        _ = _client.EnableManuallyGenerateAccessTokenEndpoint(applicationName, "a_user");
        _client.AddSecretKey(appCreated!.ApiSecret1);
        var request = _requestFaker
            .RuleFor(x => x.MagicLinkUrl, faker => faker.Internet.Url())
            .Generate();

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        responseDetails.Should().NotBeNull();
        var magicLinkUrlError = responseDetails!.Errors.FirstOrDefault(x => x.Key.Equals("MagicLinkUrl", StringComparison.CurrentCultureIgnoreCase));
        magicLinkUrlError.Should().NotBeNull();
        magicLinkUrlError.Value.Should().ContainMatch("Value must contain the `<token>` template.");
    }

    [Fact]
    public async Task I_receive_a_validation_error_when_an_invalid_url_is_sent()
    {
        // Arrange
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await _client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        _ = _client.EnableManuallyGenerateAccessTokenEndpoint(applicationName, "a_user");
        _client.AddSecretKey(appCreated!.ApiSecret1);
        var request = _requestFaker
            .RuleFor(x => x.MagicLinkUrl, () => "<token>")
            .Generate();

        // Act
        using var response = await _client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        responseDetails.Should().NotBeNull();
        var magicLinkUrlError = responseDetails!.Errors.FirstOrDefault(x => x.Key.Equals("MagicLinkUrl", StringComparison.CurrentCultureIgnoreCase));
        magicLinkUrlError.Should().NotBeNull();
        magicLinkUrlError.Value.Should().ContainMatch("Value must be a valid url.");
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}