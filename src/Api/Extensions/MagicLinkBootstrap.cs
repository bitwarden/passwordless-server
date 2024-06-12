using Passwordless.Service.MagicLinks;

namespace Passwordless.Api.Extensions;

public static class MagicLinkBootstrap
{
    public static void AddMagicLinks(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<MagicLinksOptions>().BindConfiguration("MagicLinks");
        builder.Services.AddScoped<MagicLinkService>();
    }
}