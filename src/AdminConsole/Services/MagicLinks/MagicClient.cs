using Microsoft.Extensions.Options;
using Passwordless.Common.MagicLinks.Models;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicClient : PasswordlessClient
{
    private readonly HttpClient _http;

    public MagicClient(HttpClient http, IOptions<PasswordlessOptions> options) : base(http,
        new PasswordlessOptions { ApiUrl = options.Value.ApiUrl, ApiSecret = options.Value.ApiSecret })
    {
        _http = new HttpClient(new PasswordlessHttpHandler(http, true), true)
        {
            BaseAddress = new Uri(options.Value.ApiUrl),
            DefaultRequestHeaders = { { "ApiSecret", options.Value.ApiSecret } }
        };
    }

    public async Task SendMagicLinkAsync(string userId, string emailAddress, string urlTemplate)
    {
        using var response = await _http.PostAsJsonAsync("magic-links/send",
            new SendMagicLinkRequest
            {
                UserId = userId,
                EmailAddress = emailAddress,
                UrlTemplate = urlTemplate,
                TimeToLive = Convert.ToInt32(TimeSpan.FromHours(2).TotalSeconds)
            });

        var xx = response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
    }
}