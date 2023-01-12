using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options
  => options.AddPolicy("default", (builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())));

// load user secrets

if(builder.Environment.IsDevelopment()) { 
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.Urls.Add("http://localhost:7001");
    app.Urls.Add("https://localhost:7002");
}
app.UseCors();

app.MapGet("/", () => "Hey, this place is for computers. Check out our docs for humans instead: https://docs.passwordless.dev");

app.MapSigninEndpoints();
app.MapRegisterEndpoints();
app.MapAliasEndpoints();
app.MapAccountEndpoints();
app.MapCredentialsEndpoints();
app.MapHealthEndpoints();

app.Run();