using System.Reflection;
using System.Text.Json;
using Passwordless.Api.Authorization;
using Passwordless.Api.Endpoints;
using Passwordless.Api.Helpers;
using Passwordless.Server.Endpoints;
using Passwordless.Service;
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

builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(Constants.Scheme)
    .AddCustomSchemes();

builder.Services.AddAuthorization(options => options.AddPasswordlessPolicies());
builder.Services.AddOptions<MangementOptions>()
    .BindConfiguration("PasswordlessManagement");

builder.Services.AddCors(options
    => options.AddPolicy("default", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Already has the built in web defaults
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

    options.SerializerOptions.Converters.Add(new AutoNumberToStringConverter());
});

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddTransient<ISharedManagementService, SharedManagementService>();
builder.Services.AddScoped<UserCredentialsService>();
builder.Services.AddScoped<IFido2ServiceFactory, DefaultFido2ServiceFactory>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton(sp =>
    // TODO: Remove this and use proper Ilogger<YourType>
    sp.GetRequiredService<ILoggerFactory>().CreateLogger("NonTyped"));


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
}

app.UseCors("default");
app.UseSecurityHeaders();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/",
    () =>
        "Hey, this place is for computers. Check out our human documentation instead: https://docs.passwordless.dev");
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