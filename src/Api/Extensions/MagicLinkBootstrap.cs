using Passwordless.Service.MagicLinks;

namespace Passwordless.Api.Extensions;

public static class MagicLinkBootstrap
{
    public static IServiceCollection AddMagicLinks(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddScoped<MagicLinkService>();
}