using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Passwordless.Api;
using Passwordless.Api.Authorization;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Helpers;
using Passwordless.Common.Services.Mail;
using Passwordless.Server.Endpoints;
using Passwordless.Service;
using Passwordless.Service.Mail;
using Passwordless.Service.Storage.Ef;
using Serilog;
using Serilog.Sinks.Datadog.Logs;

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

    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

    IConfigurationSection ddConfig = ctx.Configuration.GetSection("Datadog");
    if (ddConfig.Exists())
    {
        var apiKey = ddConfig.GetValue<string>("ApiKey");
        if (!string.IsNullOrEmpty(apiKey))
        {
            config.WriteTo.DatadogLogs(
                ddConfig.GetValue<string>("ApiKey"),
                tags: new[] { "version:" + version },
                service: "pass-api",
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
services.AddOptions<MangementOptions>()
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


if (builder.Environment.IsDevelopment())
{
    services.AddDatabaseDeveloperPageExceptionFilter();
}

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

if (builder.Configuration.GetValue<bool>("SelfHosted"))
{
    // When self-hosting. Migrate latest database changes during startup
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DbTenantContext>();
    dbContext.Database.Migrate();
}

app.UseCors("default");
app.UseSecurityHeaders();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<FriendlyExceptionsMiddleware>();
app.MapSigninEndpoints();
app.MapRegisterEndpoints();
app.MapAliasEndpoints();
app.MapAccountEndpoints();
app.MapCredentialsEndpoints();
app.MapUsersEndpoints();
app.MapHealthEndpoints();

app.Run();

public partial class Program
{
}