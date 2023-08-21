using System.Reflection;
using AdminConsole;
using AdminConsole.Authorization;
using AdminConsole.Billing;
using AdminConsole.Db;
using AdminConsole.Helpers;
using AdminConsole.Identity;
using AdminConsole.Services;
using AdminConsole.Services.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.Common.Services.Mail;
using Passwordless.Net;
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
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            var ddKey = ddConfig.GetValue<string>("ApiKey");
            if (!string.IsNullOrWhiteSpace(ddKey))
            {
                config.WriteTo.DatadogLogs(
                    apiKey: ddKey,
                    tags: new[] { "version:" + version },
                    service: "pass-admin-console",
                    configuration: new DatadogConfiguration(ddConfig.GetValue<string>("url")));
            }
        }
    });

    IServiceCollection services = builder.Services;

    if (builder.Environment.IsDevelopment())
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
    }
    else
    {
        services
            .AddDataProtection()
            .PersistKeysToDbContext<ConsoleDbContext>();
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
    services.AddManagementApi();

    // Database information
    services.AddDatabase(builder);

    // Identity
    services
        .AddIdentity<ConsoleAdmin, IdentityRole>()
        .AddEntityFrameworkStores<ConsoleDbContext>()
        .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders()
        .AddPasswordless(builder.Configuration.GetSection("Passwordless"));

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

    services.Configure<PasswordlessClientOptions>(builder.Configuration.GetRequiredSection("Passwordless"));

    services.AddTransient<IScopedPasswordlessClient, ScopedPasswordlessClient>();
    services.AddHttpClient<IScopedPasswordlessClient, ScopedPasswordlessClient>((provider, client) =>
    {
        var options = provider.GetRequiredService<IOptions<PasswordlessOptions>>();

        client.BaseAddress = new Uri(options.Value.ApiUrl);
    });

    // Magic link SigninManager
    services.AddTransient<MagicLinkSignInManager<ConsoleAdmin>>();

    // Setup mail service & provider
    builder.AddMail();
    services.AddSingleton<IMailService, DefaultMailService>();

    services.AddScoped<UsageService>();
    services.AddScoped<DataService>();
    services.AddScoped<InvitationService>();
    services.AddScoped<ApplicationService>();
    services.AddBilling(builder);

    // Work around to get LinkGeneration to work with /{app}/-links.
    var defaultLinkGeneratorDescriptor = services.Single(s => s.ServiceType == typeof(LinkGenerator));
    services.Remove(defaultLinkGeneratorDescriptor);
    services.AddSingleton<LinkGenerator>(serviceProvider => new LinkGeneratorDecorator(serviceProvider, defaultLinkGeneratorDescriptor.ImplementationType!));

    // Plan Features
    services.Configure<PlansOptions>(builder.Configuration.GetRequiredSection(PlansOptions.RootKey));
    WebApplication app = builder.Build();


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

    if (builder.Configuration.GetValue<bool>("SelfHosted"))
    {
        // When self-hosting. Migrate latest database changes during startup
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ConsoleDbContext>();
        dbContext.Database.Migrate();
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
    app.UseAuthorization();
    app.MapPasswordless();
    app.MapRazorPages();
    app.Run();
}