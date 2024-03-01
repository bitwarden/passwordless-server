using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.IntegrationTests.Helpers;
using Passwordless.Api.IntegrationTests.Helpers.App;
using Passwordless.Common.MagicLinks.Models;
using Passwordless.Common.Models.Apps;
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
        magicLinkUrlError.Value.Should().Contain($"You have provided a {nameof(request.UrlTemplate)} without a {SendMagicLinkRequest.TokenTemplate} template. Please include it like so: https://www.example.com?token={SendMagicLinkRequest.TokenTemplate}");
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
            .RuleFor(x => x.UrlTemplate, () => SendMagicLinkRequest.TokenTemplate)
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

        var details = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        details.Should().NotBeNull();
        details!.Type.Should().Contain("magic_link_email_admin_address_only");
    }

    [Fact]
    public async Task I_can_send_a_magic_link_email_to_an_admin_address_even_if_the_application_is_too_new()
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
        await using var api = await apiFixture.CreateApiAsync(testOutput, false);
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
        var unsuccessfulResponse = default(HttpResponseMessage);
        for (var i = 0; i < 1_000_000; i++)
        {
            var response = await client.PostAsJsonAsync("magic-link/send", request);
            if (!response.IsSuccessStatusCode)
            {
                unsuccessfulResponse = response;
                break;
            }
            else
            {
                response.Dispose();
            }
        }

        // Assert
        unsuccessfulResponse.Should().NotBeNull();
        unsuccessfulResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        unsuccessfulResponse.Dispose();
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
        var unsuccessfulResponse = default(HttpResponseMessage);
        for (var i = 0; i < 1_000_000; i++)
        {
            var response = await client.PostAsJsonAsync("magic-link/send", request);
            if (!response.IsSuccessStatusCode)
            {
                unsuccessfulResponse = response;
                break;
            }
            else
            {
                response.Dispose();
            }

            api.Time.Advance(TimeSpan.FromMinutes(1));
        }

        // Assert
        unsuccessfulResponse.Should().NotBeNull();
        unsuccessfulResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var details = await unsuccessfulResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        details.Should().NotBeNull();
        details!.Type.Should().Contain("magic_link_email_quota_exceeded");
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

        for (var i = 0; i < 1_000_000; i++)
        {
            using var initialResponse = await client.PostAsJsonAsync("magic-link/send", request);
            if (!initialResponse.IsSuccessStatusCode)
                break;

            api.Time.Advance(TimeSpan.FromMinutes(1));
        }

        api.Time.Advance(TimeSpan.FromDays(31));

        // MemoryCache does not support time travel, So reset manually
        api.ResetCache();

        // Act
        using var response = await client.PostAsJsonAsync("magic-link/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}