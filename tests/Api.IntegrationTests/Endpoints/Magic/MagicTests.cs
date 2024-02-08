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
using Xunit.Abstractions;

namespace Passwordless.Api.IntegrationTests.Endpoints.Magic;

public class MagicTests(ITestOutputHelper testOutput, PasswordlessApiFixture apiFixture)
    : IClassFixture<PasswordlessApiFixture>
{
    private readonly Faker<SendMagicLinkRequest> _requestFaker = RequestHelpers.GetMagicLinkRequestRules();

    [Fact]
    public async Task I_can_send_a_magic_link_email()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_cannot_send_a_magic_link_email_if_the_feature_is_disabled()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        _ = await client.DisableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_receive_a_validation_error_when_the_url_does_not_contain_the_token_template()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        _ = await client.EnableMagicLinks("a_user");
        var request = _requestFaker
            .RuleFor(x => x.UrlTemplate, faker => faker.Internet.Url())
            .Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

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
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker
            .RuleFor(x => x.UrlTemplate, () => "<token>")
            .Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

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
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName, "definitely-not@what-faker-will.generate");
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task I_can_send_a_magic_link_email_to_an_admin_address_if_the_application_even_if_the_application_is_too_new()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        const string emailAddress = "admin@email.com";
        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName, emailAddress);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker
            .RuleFor(x => x.EmailAddress, emailAddress)
            .Generate();

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_cannot_send_too_many_magic_link_emails_in_a_short_time()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        // Act
        var responseStatusCodes = new List<HttpStatusCode>();
        for (var i = 0; i < 100; i++)
        {
            using var response = await client.PostAsJsonAsync("magic-link/send", request);
            responseStatusCodes.Add(response.StatusCode);
        }

        // Assert
        responseStatusCodes.Should().Contain(HttpStatusCode.NoContent);
        responseStatusCodes.Should().Contain(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task I_can_send_a_magic_link_email_after_enough_time_passed_since_the_rate_limit_was_exceeded()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        for (var i = 0; i < 100; i++)
        {
            using var _ = await client.PostAsJsonAsync("magic-link/send", request);
        }

        api.Time.Advance(TimeSpan.FromMinutes(30));

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task I_cannot_send_too_many_magic_link_emails_in_a_month()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        // Act
        var responseStatusCodes = new List<HttpStatusCode>();
        for (var i = 0; i < 5000; i++)
        {
            using var response = await client.PostAsJsonAsync("magic-link/send", request);
            api.Time.Advance(TimeSpan.FromMinutes(1));
            responseStatusCodes.Add(response.StatusCode);
        }

        // Assert
        responseStatusCodes.Should().Contain(HttpStatusCode.NoContent);
        responseStatusCodes.Should().Contain(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task I_can_send_a_magic_link_email_after_enough_time_passed_since_the_monthly_quota_was_exceeded()
    {
        // Arrange
        await using var api = await apiFixture.CreateApiAsync(testOutput);
        using var client = api.CreateClient();

        var applicationName = CreateAppHelpers.GetApplicationName();
        using var appCreateResponse = await client.CreateApplicationAsync(applicationName);
        var appCreated = await appCreateResponse.Content.ReadFromJsonAsync<CreateAppResultDto>();
        client.AddSecretKey(appCreated!.ApiSecret1);
        await client.EnableMagicLinks("a_user");
        var request = _requestFaker.Generate();

        // Skip all limitations for new applications
        api.Time.Advance(TimeSpan.FromDays(365));

        for (var i = 0; i < 5000; i++)
        {
            using var _ = await client.PostAsJsonAsync("magic-link/send", request);
            api.Time.Advance(TimeSpan.FromMinutes(1));
        }

        api.Time.Advance(TimeSpan.FromDays(30));

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}