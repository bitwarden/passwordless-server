using AdminConsole.Db;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace AdminConsole;

public class CurrentContext : ICurrentContext
{
    public string Tenant { get; set; }
    public PasswordlessOptions Options { get; set; }
}

public interface ICurrentContext
{
    string Tenant { get; set; }
    PasswordlessOptions Options { get; set; }
}

public class CurrentContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<PasswordlessOptions> defaultOptions;

    public CurrentContextMiddleware(RequestDelegate next, IOptions<PasswordlessOptions> defaultOptions)
    {
        _next = next;
        this.defaultOptions = defaultOptions;
    }

    // IMessageWriter is injected into InvokeAsync
    public async Task InvokeAsync(HttpContext httpContext, ICurrentContext currentcontext, ConsoleDbContext dbContext, IOptionsSnapshot<PasswordlessOptions> optionsSnapshot)
    {
        var name = httpContext.GetRouteData();

        name.Values.TryGetValue("app", out var tenantName);

        currentcontext.Tenant = tenantName as string;

        // load options from db
        var appConfig = dbContext.Applications.FirstOrDefault(t => t.Id == currentcontext.Tenant);

        if (appConfig != null && !string.IsNullOrEmpty(currentcontext.Tenant))
        {
            // use the options from the database
            currentcontext.Options = new PasswordlessOptions()
            {

                ApiUrl = optionsSnapshot.Value.ApiUrl ?? appConfig.ApiUrl,
                ApiKey = appConfig.ApiKey,
                ApiSecret = appConfig.ApiSecret,

            };
        }
        else
        {
            currentcontext.Options = this.defaultOptions.Value;
        }

        // Override optionsSnapshot with values from the database
        optionsSnapshot.Value.ApiKey = currentcontext.Options.ApiKey;
        optionsSnapshot.Value.ApiSecret = currentcontext.Options.ApiSecret;
        optionsSnapshot.Value.ApiUrl = currentcontext.Options.ApiUrl;

        await _next(httpContext);
    }
}