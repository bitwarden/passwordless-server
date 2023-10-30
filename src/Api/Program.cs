using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Passwordless.Api;
using Passwordless.Api.Authorization;
using Passwordless.Api.Endpoints;
using Passwordless.Api.HealthChecks;
using Passwordless.Api.Helpers;
using Passwordless.Api.Middleware;
using Passwordless.Common.Configuration;
using Passwordless.Common.Middleware.SelfHosting;
using Passwordless.Common.Services.Mail;
using Passwordless.Common.Utils;
using Passwordless.Service;
using Passwordless.Service.EventLog;
using Passwordless.Service.Features;
using Passwordless.Service.Mail;
using Passwordless.Service.Storage.Ef;
using Serilog;
using Serilog.Sinks.Datadog.Logs;

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
        // setup serilog logging
        var apiKey = ddConfig.GetValue<string>("ApiKey");
        if (!string.IsNullOrEmpty(apiKey))
        {
            config.WriteTo.DatadogLogs(
                ddConfig.GetValue<string>("ApiKey"),
                configuration: new DatadogConfiguration(ddConfig.GetValue<string>("url")));
        }
    }
});

var services = builder.Services;

services.AddProblemDetails();
services
    .AddAuthentication(Constants.Scheme)
    .AddCustomSchemes();

services.AddAuthorization(options => options.AddPasswordlessPolicies());
services.AddOptions<ManagementOptions>()
    .BindConfiguration("PasswordlessManagement");

services.AddCors(options
    => options.AddPolicy("default", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

services.AddHttpContextAccessor();
services.AddScoped<ITenantProvider, TenantProvider>();

services.ConfigureHttpJsonOptions(options =>
{
    // Already has the built in web defaults
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

    options.SerializerOptions.Converters.Add(new AutoNumberToStringConverter());
});

services.AddDatabase(builder.Configuration);
services.AddTransient<ISharedManagementService, SharedManagementService>();
services.AddScoped<UserCredentialsService>();
services.AddScoped<IApplicationService, ApplicationService>();
services.AddScoped<IFido2ServiceFactory, DefaultFido2ServiceFactory>();
services.AddScoped<ITokenService, TokenService>();
services.AddSingleton<ISystemClock, SystemClock>();
services.AddScoped<IRequestContext, RequestContext>();
builder.AddMail();
builder.Services.AddSingleton<IMailService, DefaultMailService>();

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

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    app.UseDevelopmentEndpoints();
}
else
{
    app.UseHsts();
    app.MapGet("/",
        () =>
            "Hey, this place is for computers. Check out our human documentation instead: https://docs.passwordless.dev");
}

if (isSelfHosted)
{
    app.UseMiddleware<HttpOverridesMiddleware>();

    // When self-hosting. Migrate latest database changes during startup
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DbGlobalContext>();
    dbContext.Database.Migrate();

    if (!await dbContext.ApiKeys.AnyAsync())
    {
        var passwordlessConfiguration = builder.Configuration.GetRequiredSection("Passwordless");
        var apiKey = passwordlessConfiguration.GetValue<string>("ApiKey");
        var apiSecret = passwordlessConfiguration.GetValue<string>("ApiSecret");
        var appName = ApiKeyUtils.GetAppId(apiKey);
        await dbContext.SeedDefaultApplicationAsync(appName, apiKey, apiSecret);
        await dbContext.SaveChangesAsync();
    }
}

app.UseCors("default");
app.UseSecurityHeaders();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AcceptHeaderMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<EventLogContextMiddleware>();
app.UseMiddleware<EventLogStorageCommitMiddleware>();
app.UseMiddleware<FriendlyExceptionsMiddleware>();
app.MapSigninEndpoints();
app.MapRegisterEndpoints();
app.MapAliasEndpoints();
app.MapAccountEndpoints();
app.MapCredentialsEndpoints();
app.MapUsersEndpoints();
app.MapHealthEndpoints();
app.MapEventLogEndpoints();

app.MapPasswordlessHealthChecks();

app.Run();

public partial class Program
{
}