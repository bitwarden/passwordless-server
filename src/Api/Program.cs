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
using Passwordless.Api.RateLimiting;
using Passwordless.Api.Reporting.Background;
using Passwordless.Common.Configuration;
using Passwordless.Common.HealthChecks;
using Passwordless.Common.Middleware.SelfHosting;
using Passwordless.Common.Services.Mail;
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
    swagger.OperationFilter<ExtendedStatusDescriptionsOperationFilter>();
    swagger.OperationFilter<ExternalDocsOperationFilter>();
    swagger.SupportNonNullableReferenceTypes();
    swagger.SwaggerDoc("v4", new OpenApiInfo
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
    swagger.SwaggerGeneratorOptions.IgnoreObsoleteActions = true;
});

if (builder.Configuration.IsSelfHosted())
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
    }
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

app.UseSwagger(c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
{
    httpReq.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
}));

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v4/swagger.json", "v4");
    c.ConfigObject.ShowExtensions = true;
    c.ConfigObject.ShowCommonExtensions = true;
    c.DefaultModelsExpandDepth(-1);
    c.IndexStream = () => typeof(Program).Assembly.GetManifestResourceStream("Passwordless.Api.OpenApi.swagger.html");
    c.InjectStylesheet("/openapi.css");
    c.SupportedSubmitMethods();
});

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
app.UseSerilogRequestLogging();
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

app.MapPasswordlessHealthChecks();

app.Run();

public partial class Program;