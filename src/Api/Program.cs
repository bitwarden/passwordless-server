using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Passwordless.Api;
using Passwordless.Api.Authorization;
using Passwordless.Api.Email;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Extensions;
using Passwordless.Api.HealthChecks;
using Passwordless.Api.Helpers;
using Passwordless.Api.Middleware;
using Passwordless.Api.OpenApi;
using Passwordless.Api.OpenApi.Filters;
using Passwordless.Api.RateLimiting;
using Passwordless.Api.Reporting.Background;
using Passwordless.Common.Configuration;
using Passwordless.Common.HealthChecks;
using Passwordless.Common.Logging;
using Passwordless.Common.Middleware.SelfHosting;
using Passwordless.Common.Services.Mail;
using Passwordless.Service;
using Passwordless.Service.EventLog;
using Passwordless.Service.Features;
using Passwordless.Service.MDS;
using Passwordless.Service.Storage.Ef;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

if (builder.Configuration.IsSelfHosted())
{
    builder.AddSelfHostingConfiguration();
}

builder.WebHost.ConfigureKestrel(c => c.AddServerHeader = false);

builder.AddSerilog();

var services = builder.Services;

services.AddProblemDetails();
services
    .AddAuthentication()
    .AddCustomSchemes();

services.AddAuthorization();
services.AddOptions<ManagementOptions>()
    .BindConfiguration("PasswordlessManagement");

services.AddCors(options
    => options.AddPolicy("default", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

services.AddHttpContextAccessor();
services.AddScoped<ITenantProvider, TenantProvider>();

services.AddRateLimiting();

services.ConfigureHttpJsonOptions(options =>
{
    // Already has the built in web defaults
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

    options.SerializerOptions.Converters.Add(new AutoNumberToStringConverter());
});

services.AddDatabase(builder.Configuration);
services.AddTransient<ISharedManagementService, SharedManagementService>();
services.AddScoped<UserCredentialsService>();
services.AddScoped<IReportingService, ReportingService>();
services.AddScoped<IApplicationService, ApplicationService>();
services.AddScoped<IFido2Service, Fido2Service>();
services.AddScoped<ITokenService, TokenService>();
services.AddSingleton<ISystemClock, TimeProviderSystemClockAdapter>();
services.AddScoped<IRequestContext, RequestContext>();
services.AddSingleton<IMetaDataService, MetaDataService>();

services.AddReportingBackgroundServices();
services.AddHostedService<DispatchedEmailCleanupService>();

builder.AddMail();

services.AddSingleton<Microsoft.Extensions.Internal.ISystemClock, TimeProviderSystemClockAdapter>();
services.AddMemoryCache();
services.AddDistributedMemoryCache();
builder.AddMetaDataService();

builder.Services.AddTransient<IAuthenticationConfigurationService, AuthenticationConfigurationService>();

services.AddSingleton(sp =>
    // TODO: Remove this and use proper Ilogger<YourType>
    sp.GetRequiredService<ILoggerFactory>().CreateLogger("NonTyped"));

services.AddScoped<IFeatureContextProvider, FeatureContextProvider>();
services.AddEventLogging();

if (builder.Environment.IsDevelopment())
{
    services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.AddPasswordlessHealthChecks();

builder.Services.AddMagicLinks();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
    app.MapGet("/",
        () =>
            "Hey, this place is for computers. Check out our human documentation instead: https://docs.passwordless.dev");
}

app.UseOpenApi();

if (builder.Configuration.IsSelfHosted())
{
    app.UseMiddleware<HttpOverridesMiddleware>();

    // When self-hosting. Migrate latest database changes during startup.
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DbGlobalContext>();

    dbContext.Database.Migrate();
}

app.UseCors("default");
app.UseSecurityHeaders();
app.UseStaticFiles();
app.UseWhen(o =>
{
    if (o.Request.Path == "/")
    {
        return false;
    }
    return !o.Request.Path.StartsWithSegments(HealthCheckEndpoints.Path);
}, c =>
{
    c.UseMiddleware<EventLogStorageCommitMiddleware>();
});
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilog();
app.UseWhen(o =>
{
    if (o.Request.Path == "/")
    {
        return false;
    }
    return !o.Request.Path.StartsWithSegments(HealthCheckEndpoints.Path);
}, c =>
{
    c.UseMiddleware<EventLogContextMiddleware>();
});
app.UseMiddleware<FriendlyExceptionsMiddleware>();
app.MapSigninEndpoints();
app.MapRegisterEndpoints();
app.MapAliasEndpoints();
app.MapAccountEndpoints();
app.MapCredentialsEndpoints();
app.MapUsersEndpoints();
app.MapMagicEndpoints();
app.MapHealthEndpoints();
app.MapEventLogEndpoints();
app.MapReportingEndpoints();
app.MapMetaDataServiceEndpoints();
app.MapAuthenticatorsEndpoints();
app.MapAuthenticationConfigurationEndpoints();

app.MapPasswordlessHealthChecks();

// Apply migrations and seed data
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DbGlobalContext>();

    await dbContext.Database.MigrateAsync();

    await dbContext.SeedDefaultApplicationAsync(
        "test",
        "test:public:2e728aa5986f4ba8b073a5b28a939795",
        "test:secret:a679563b331846c79c20b114a4f56d02"
    );

    await dbContext.SaveChangesAsync();
}

app.Run();

/// <summary>
/// Application entrypoint.
/// </summary>
public partial class Program;