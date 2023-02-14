using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options
  => options.AddPolicy("default", (builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())));

// load user secrets

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false);
}

builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddSingleton<IDbTenantContextFactory, MultiTenantSqliteDbTenantContextFactory>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<IStorage>((sp) => {
    var factory = sp.GetRequiredService<IDbTenantContextFactory>();
    var tenantProvider = sp.GetRequiredService<ITenantProvider>();
    return factory.GetExistingTenant(tenantProvider.Tenant);
});
builder.Services.AddSingleton<ILogger>((sp) => {
   // TODO: Remove this and use proper Ilogger<YourType>
    return sp.GetService<ILogger<NonTyped>>();
});

//builder.Services.AddDbContextFactory<MyDataContext>(), ServiceLifetime.Transient);
builder.Services.AddDbContextFactory<MyDataContext>((ops) => { }, ServiceLifetime.Scoped);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.Urls.Add("http://localhost:7001");
    app.Urls.Add("https://localhost:7002");
}
app.UseCors();

app.UseMiddleware<TenantProviderMiddelware>();

app.MapGet("/", () => "Hey, this place is for computers. Check out our docs for humans instead: https://docs.passwordless.dev");


app.MapSigninEndpoints();
app.MapRegisterEndpoints();
app.MapAliasEndpoints();
app.MapAccountEndpoints();
app.MapCredentialsEndpoints();
app.MapHealthEndpoints();

app.Run();

public partial class Program { }