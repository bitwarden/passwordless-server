# Passwordless.AspNetCore

<!-- Add little images showing version and such -->

## Getting Started

```bash
dotnet add package Passwordless.AspNetCore
```

## Usage

In Minimal APIs

```csharp
var builder = WebApplication.CreateBuilder(args);

services.AddIdentity<MyUser, MyRole>()
      .AddEntityFrameworkStores<MyDbContext>()
      .AddPasswordless(Configuration.GetRequiredSection("Passwordless"));

var app = builder.Build();

app.MapPasswordless();

app.Run();
```

In MVC

```csharp
// In Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddIdentity<MyUser, MyRole>()
      .AddEntityFrameworkStores<MyDbContext>()
      .AddPasswordless(Configuration.GetRequiredSection("Passwordless"));

    services.AddControllers();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapPasswordless();
        endpoints.MapControllers();
    });
}
```

<!-- Link to frontend instructions -->
