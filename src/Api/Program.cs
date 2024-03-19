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
using Passwordless.Api.OpenApi.Filters;
using Passwordless.Api.Reporting.Background;
using Passwordless.Common.Configuration;
using Passwordless.Common.Middleware.SelfHosting;
using Passwordless.Common.Services.Mail;
using Passwordless.Common.Utils;
using Passwordless.Service;
using Passwordless.Service.EventLog;
using Passwordless.Service.Features;
using Passwordless.Service.MDS;
using Passwordless.Service.Storage.Ef;
using Serilog;
using Serilog.Sinks.Datadog.Logs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Passwordless.Api.xml"), true);
    swagger.DocInclusionPredicate((docName, apiDesc) =>
    {
        var policy = (AuthorizationPolicy?)apiDesc.ActionDescriptor.EndpointMetadata.FirstOrDefault(x => x.GetType() == typeof(AuthorizationPolicy));
        if (policy == null)
        {
            return false;
        }
        return !policy.AuthenticationSchemes.Contains(Constants.ManagementKeyAuthenticationScheme);
    });
    swagger.OperationFilter<AuthorizationOperationFilter>();
    swagger.SupportNonNullableReferenceTypes();
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v4",
        Title = "Passwordless.dev API",
        TermsOfService = new Uri("https://bitwarden.com/terms/"),
        Contact = new OpenApiContact
        {
            Email = "support@passwordless.dev",
            Name = "Support",
            Url = new Uri("https://bitwarden.com/contact/")
        }
    });
});

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
    },
    false,
    // Pass log events down to other logging providers (e.g. Microsoft) after Serilog, so
    // that they can be processed in a uniform way in tests.
    true
);

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

services.AddRateLimiter(options =>
{
    // Reject with 429 instead of the default 503
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddMagicRateLimiterPolicy();
});

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
    app.UseDevelopmentEndpoints();
}
else
{
    app.UseHsts();
    app.MapGet("/",
        () =>
            "Hey, this place is for computers. Check out our human documentation instead: https://docs.passwordless.dev");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.ConfigObject.ShowExtensions = true;
    c.ConfigObject.ShowCommonExtensions = true;
});

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
app.UseWhen(o =>
{
    return !o.Request.Path.StartsWithSegments("/health");
}, c =>
{
    c.UseMiddleware<EventLogStorageCommitMiddleware>();
});
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseWhen(o =>
{
    return !o.Request.Path.StartsWithSegments("/health");
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

app.MapPasswordlessHealthChecks();

app.Run();

public partial class Program;