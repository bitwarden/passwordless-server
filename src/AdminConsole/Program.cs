using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Components;
using Passwordless.AdminConsole.Components.Account;
using Passwordless.AdminConsole.Controller;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.AuthenticatorData;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.AspNetCore;
using Passwordless.Common.Configuration;
using Passwordless.Common.Middleware.SelfHosting;
using Passwordless.Common.Services.Mail;
using Serilog;
using Serilog.Sinks.Datadog.Logs;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    RunTheApp();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

void RunTheApp()
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    bool isSelfHosted = builder.Configuration.GetValue<bool>("SelfHosted");

    if (isSelfHosted)
    {
        builder.AddSelfHostingConfiguration();
    }

    builder.WebHost.ConfigureKestrel(c => c.AddServerHeader = false);

    builder.Host.UseSerilog((ctx, sp, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console();

        if (builder.Environment.IsDevelopment())
        {
            config.WriteTo.Seq("http://localhost:5341");
        }

        var ddApiKey = Environment.GetEnvironmentVariable("DD_API_KEY");
        if (!string.IsNullOrEmpty(ddApiKey))
        {
            var ddSite = Environment.GetEnvironmentVariable("DD_SITE") ?? "datadoghq.eu";
            var ddUrl = $"https://http-intake.logs.{ddSite}";
            var ddConfig = new DatadogConfiguration(ddUrl);

            if (!string.IsNullOrEmpty(ddApiKey))
            {
                config.WriteTo.DatadogLogs(
                    ddApiKey,
                    configuration: ddConfig);
            }
        }
    });

    IServiceCollection services = builder.Services;

    if (builder.Environment.IsDevelopment())
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
    }

    services.AddRazorPages(options =>
    {
        options.AddTenantRouting();
        options.Conventions.AuthorizeFolder("/");
        options.Conventions.AuthorizeFolder("/App", CustomPolicy.HasAppRole);
        options.Conventions.AddPageRoute("/Organization/Create", "/signup");
    });

    services.AddScoped<ICurrentContext, CurrentContext>();
    services.AddTransient<IActionContextAccessor, ActionContextAccessor>();

    builder.AddManagementApi();

    builder.Services.AddSingleton<IAuthenticatorDataProvider, AuthenticatorDataProvider>();

    builder.AddDatabase();

    services.ConfigureApplicationCookie(o =>
    {
        o.Cookie.Name = "AdminConsoleSignIn";
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

    services.AddTransient<IAuthorizationHandler, HasAppHandler>();
    services.AddAuthorization(c =>
    {
        c.AddPolicy(CustomPolicy.HasAppRole, b =>
        {
            b.RequireAuthenticatedUser();
            b.AddRequirements(new HasAppRoleRequirement());
        });
    });

    services.AddHostedService<TimedHostedService>();
    services.AddHostedService<ApplicationDeletionBackgroundService>();

    services.AddTransient<IScopedPasswordlessClient, ScopedPasswordlessClient>();
    services.AddHttpClient<IScopedPasswordlessClient, ScopedPasswordlessClient>((provider, client) =>
    {
        var options = provider.GetRequiredService<IOptions<PasswordlessManagementOptions>>();

        client.BaseAddress = new Uri(options.Value.InternalApiUrl);
    });

    // Magic link SigninManager
    services.AddTransient<IMagicLinkBuilder, MagicLinkBuilder>();
    services.AddTransient<MagicLinkSignInManager<ConsoleAdmin>>();

    // Setup mail service & provider
    builder.AddMail();
    services.AddSingleton<IMailService, DefaultMailService>();

    // Work around to get LinkGeneration to work with /{app}/-links.
    var defaultLinkGeneratorDescriptor = services.Single(s => s.ServiceType == typeof(LinkGenerator));
    services.Remove(defaultLinkGeneratorDescriptor);
    services.AddSingleton<LinkGenerator>(serviceProvider => new LinkGeneratorDecorator(serviceProvider, defaultLinkGeneratorDescriptor.ImplementationType!));

    // Used for Razor/Blazor experiment
    builder.Services.AddRazorComponents();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    builder.Services.AddAntiforgery();

    WebApplication app;
    try
    {
        app = builder.Build();
    }
    catch (HostAbortedException)
    {
        // https://github.com/dotnet/efcore/issues/29809
        Log.Logger.Information(".NET Migrations: Graceful exit.");
        return;
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    if (isSelfHosted)
    {
        app.UseMiddleware<HttpOverridesMiddleware>();
        app.ExecuteMigration();
    }

    app.UseCSP();
    app.UseSecurityHeaders();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.MapHealthEndpoints();
    app.UseAuthentication();
    app.UseMiddleware<CurrentContextMiddleware>();
    app.UseMiddleware<EventLogStorageCommitMiddleware>();
    app.UseAuthorization();
    app.UseAntiforgery();
    app.MapPasswordless()
        .LoginRoute?.AddEndpointFilter<LoginEndpointFilter>();
    app.MapRazorPages();

    if (app.Environment.IsDevelopment())
    {
        // Scan the App component for Blazor page components and map the routes.
        app.MapRazorComponents<App>();
    }

    app.MapAccountEndpoints();

    app.Run();
}