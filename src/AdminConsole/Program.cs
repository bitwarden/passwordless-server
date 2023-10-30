using System.Reflection;
using Datadog.Trace;
using Datadog.Trace.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Passwordless;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;
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

        IConfigurationSection ddConfig = ctx.Configuration.GetSection("Datadog");
        if (ddConfig.Exists())
        {
            var version = assembly.GetInformationalVersion() ??
                              assembly.GetName().Version?.ToString() ??
                              "unknown version"; // 1.2.3-ci-SHA
            
            // TRY TO GET DataDog Azure App Service Extension to use a version configured from code
            // instead of DD_VERSION environment variable
            // Doesn't seem to work though? 

            // Configure the DataDog Tracer with version
            var settings = TracerSettings.FromDefaultSources();
            settings.ServiceVersion = version;
            Tracer.Configure(settings);

            var ddKey = ddConfig.GetValue<string>("ApiKey");
            if (!string.IsNullOrWhiteSpace(ddKey))
            {
                config.WriteTo.DatadogLogs(
                    apiKey: ddKey,
                    tags: new[] { "version:" + version },
                    configuration: new DatadogConfiguration(ddConfig.GetValue<string>("url")));
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

    services.AddHttpClient();
    builder.AddManagementApi();

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
        var options = provider.GetRequiredService<IOptions<PasswordlessOptions>>();

        client.BaseAddress = new Uri(options.Value.ApiUrl);
    }).AddHttpMessageHandler<PasswordlessDelegatingHandler>();

    // Magic link SigninManager
    services.AddTransient<MagicLinkSignInManager<ConsoleAdmin>>();

    // Setup mail service & provider
    builder.AddMail();
    services.AddSingleton<IMailService, DefaultMailService>();

    // Work around to get LinkGeneration to work with /{app}/-links.
    var defaultLinkGeneratorDescriptor = services.Single(s => s.ServiceType == typeof(LinkGenerator));
    services.Remove(defaultLinkGeneratorDescriptor);
    services.AddSingleton<LinkGenerator>(serviceProvider => new LinkGeneratorDecorator(serviceProvider, defaultLinkGeneratorDescriptor.ImplementationType!));

    // Plan Features
    services.Configure<PlansOptions>(builder.Configuration.GetRequiredSection(PlansOptions.RootKey));

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
    app.MapPasswordless()
        .LoginRoute?.AddEndpointFilter<LoginEndpointFilter>();
    app.MapRazorPages();
    app.Run();
}