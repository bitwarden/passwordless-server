using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.BackgroundServices;
using Passwordless.AdminConsole.Components;
using Passwordless.AdminConsole.Components.Account;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Endpoints;
using Passwordless.AdminConsole.HealthChecks;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.RateLimiting;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.AuthenticatorData;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.AspNetCore;
using Passwordless.Common.Configuration;
using Passwordless.Common.HealthChecks;
using Passwordless.Common.Logging;
using Passwordless.Common.Middleware.SelfHosting;
using Passwordless.Common.Services.Mail;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    await RunAppAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

async Task RunAppAsync()
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    if (builder.Configuration.IsSelfHosted())
    {
        builder.AddSelfHostingConfiguration();
    }

    builder.WebHost.ConfigureKestrel(c => c.AddServerHeader = false);

    builder.AddSerilog();

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

    builder.Services.AddScoped<IPostSignInHandlerService, PostSignInHandlerService>();
    services.ConfigureApplicationCookie(o =>
    {
        o.Events.OnSignedIn = async context =>
        {
            var handler = context.HttpContext.RequestServices.GetRequiredService<IPostSignInHandlerService>();
            await handler.HandleAsync();
        };
        o.Cookie.Name = "AdminConsoleSignIn";
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

    services.AddTransient<IAuthorizationHandler, HasAppHandler>();
    services.AddTransient<IAuthorizationHandler, StepUpHandler>();
    services.AddAuthorization(c =>
    {
        c.AddPolicy(CustomPolicy.HasAppRole, b =>
        {
            b.RequireAuthenticatedUser();
            b.AddRequirements(new HasAppRoleRequirement());
        });
        
        c.AddPolicy(CustomPolicy.StepUp, b =>
        {
            b.RequireAuthenticatedUser();
            b.AddRequirements(new StepUpRequirement(CustomClaimTypes.StepUp));
        });
    });

    services.AddSingleton<StepUpPurpose>();

    services.AddHostedService<OnboardingCleanupBackgroundService>();
    services.AddHostedService<ApplicationCleanupBackgroundService>();

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
    builder.Services.AddRateLimiting();

    builder.AddPasswordlessHealthChecks();

    services.AddScoped<ISetupService, SetupService>();

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
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    if (builder.Configuration.IsSelfHosted())
    {
        app.UseMiddleware<HttpOverridesMiddleware>();
        app.ExecuteMigration();
    }

    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSerilog(withUserAgent: true);
    app.UseRouting();
    app.UseAuthentication();
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments(HealthCheckEndpoints.Path),
        appBuilder =>
        {
            appBuilder.UseMiddleware<CurrentContextMiddleware>();
            appBuilder.UseMiddleware<EventLogStorageCommitMiddleware>();
        });
    app.UseAuthorization();
    app.UseAntiforgery();
    app.UseRateLimiter();
    app.MapPasswordless()
        .LoginRoute?.AddEndpointFilter<LoginEndpointFilter>();
    app.MapRazorPages();

    app.MapRazorComponents<App>();

    app.MapAccountEndpoints();
    app.MapApplicationEndpoints();
    app.MapBillingEndpoints();

    app.MapPasswordlessHealthChecks();

    // Apply migrations
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsoleDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    app.Run();
}