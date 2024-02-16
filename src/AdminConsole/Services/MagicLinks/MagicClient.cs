using Microsoft.Extensions.Options;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicClient : PasswordlessClient
{

    private readonly HttpClient _http;
    private readonly PasswordlessOptions _options;

    public MagicClient(HttpClient http, IOptions<PasswordlessOptions> options) : base(http, new PasswordlessOptions { ApiUrl = options.Value.ApiUrl, ApiSecret = options.Value.ApiSecret })
    {
        _http = new HttpClient(new PasswordlessHttpHandler(http, true), true)
        {
            BaseAddress = new Uri(options.Value.ApiUrl),
            DefaultRequestHeaders =
            {
                {"ApiSecret", options.Value.ApiSecret}
            }
        };

        _options = options.Value;
    }

    public async Task SendMagicLinkAsync(string userId, string emailAddress, string urlTemplate)
    {
        using var response = await _http.PostAsJsonAsync("/magic-link/send", new
        {
            userId,
            emailAddress,
            urlTemplate
        });

        var xx = response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
    }

}